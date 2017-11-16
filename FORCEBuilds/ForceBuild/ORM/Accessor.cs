using Castle.DynamicProxy;
using FORCEBuild.Core;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using FORCEBuild.Crosscutting;
using FORCEBuild.Crosscutting.Log;
using FORCEBuild.Helper;

namespace FORCEBuild.ORM {

    /* 部分属性更新为空时，使用dbnull。（相应的数据库更新规则）
     * 为了防止重复更新，需检测可能的反向引用，考虑三种方法：
     * 1.会话机制(同nhibernate)，设立修改标志,修改后置1,预先检查防止反向引用
     * 2.每次更新前读数据库值，比较后更新
     * 3.利用先前保持的引用列表对比更新
     * 2017.2：
     * 配置文档新增是否存在双向引用属性
     * 为了防止双向引用造成的循环插入，同时采取两种方法：
     * 1.缓存保持对象，每次插入前查询缓存
     * （2017.2：添加接口ID检测，大于0即表示已插入）
     * 2.方向性写入，先插入外键链接对象，然后写入数据库占位生成guid，提供给主键链接
     * 2017.3.18:
     * 反射性能改进,dynamic或delegate或expression
     * 2017.4.7
     * 对于值类型，总是不为null，当类型为datetime时，插入数据库时出错
     * 检查value==default(t)
     * public static object GetDefaultValue(this Type t)
       {
            if (t.IsValueType && Nullable.GetUnderlyingType(t) == null)
                return Activator.CreateInstance(t);
            else
                return null;
       }但是性能太低
       因此在整个生命周期跟踪对象，从构造函数开始跟踪属性变化
       2017.4.8
       允许批量删除一个对象的部分属性
       2017.5.21
       加锁，保证操作顺序性和单个操作的原子性
       2.17.6.1
       ORMID改为Guid实现，关联Model的ID或单独使用
       2017.6.20
       由于当非双向引用时，插入一个对象时，如果不插入外键对象，便会丢失对象的引用历史。
       所以不管是否双向引用，都应该插入引用对象，只是应该注意双向引用，只在需要更新的一端更新。
       在更新时，外键对象想要获得对应键无法直接通过引用获取，但拥有该对象的对象会保留引用更新，在下一次更新时完成写入。
       如果此时更新依然只更新已插入对象，依然会丢失引用。
       所以得出一个结论:不管是更新还是插入，都依据update标识。
       同理，对于必须插入数据库的一边，即外键对象插入时必须写入外键情况
     */

    /// <summary>
    /// 访问器
    /// </summary>
    internal abstract class Accessor {

        /// <summary>
        /// 对应数据库中的id列名，用于跟踪对象，保持固定
        /// </summary>
        public const string IdColumn = "guid";
        /// <summary>
        /// 是否链接model中的属性
        /// </summary>
        public bool IsLinked { get; set; }

        public ILog Log { get; set; }

        //public string SpecificProperty { get; set; }
        //  public st Type { get; set; }
        
        protected abstract void ExecuteSql(DataBaseCommand ac);
        /// <summary>
        /// 执行写入后返回自增长的id
        /// </summary>
        /// <param name="ac"></param>
        /// <returns></returns>
        protected abstract int InsertSql(InsertCommand ac);
        protected abstract DataTable Read(SelectCommand ac);
        protected abstract DataTable Read(string sql);
        public string ConnectionString { get; set; }
        public AccessorDispatcher Scheduler { get; set; }

      //  private readonly object _locker = new object();

        /// <summary>
        /// 指示是否由内存分配id 
        /// 注：在orm第二阶段取消
        /// </summary>
       // public bool EmptyId { get; set; }

//        public int DefaultId { get; set; }

        private ConcurrentDictionary<Type, ClassDefine> _classmodels;

        public ConcurrentDictionary<Type, ClassDefine> ClassModels {
            private get => _classmodels;
            set {
                _classmodels = value;
                if (Cache == null)
                    Cache = new ConcurrentDictionary<Type, Dictionary<Guid, object>>();
                else
                    Cache.Clear();
                foreach (var x in value)
                    Cache.TryAdd(x.Key, new Dictionary<Guid, object>());
            }
        }

        //保持已插入和取出对象的引用
        public ConcurrentDictionary<Type, Dictionary<Guid, object>> Cache { get; set; }

        public ForceBuildFactory ProxyFactory { private get; set; }

        public virtual void Insert(IOrmModel model) 
        {
            var interceptor = model.OrmInterceptor;//IForceBuildFactory.GetInterceptor<OrmInterceptor>(model);
            var type = model.ClassDefine.ClassType;
            if (interceptor.ORMID != Guid.Empty)
                return;
            var classModel = model.ClassDefine;
            Guid primaryKey;
            
            var cmd = new InsertCommand();

            #region 多对一

            //首先写入外键类型对应的已存在对象外键
            foreach (var manytoOne in classModel.ManyToOne.Values)
            {
                var value = manytoOne.PropertyInfo.GetValue(model);
                if (value != null) //对象为空不插入，相应的，数据库需要允许外键为空
                {
                    var iOrmModel = value as IOrmModel;
                    if (iOrmModel.OrmInterceptor.ORMID.IsEmpty())
                    {
                        Insert(iOrmModel);
                    }
                    cmd.InsertPairs.Add(new ColumnValuePair
                    {
                        Column = manytoOne.Column,
                        Value = iOrmModel.OrmInterceptor.ORMID
                    });
                    
                }
            }

            #endregion

            #region 一对一外键

            foreach (var foreignOne in classModel.ForeignOne.Values)
            {
                var value = foreignOne.PropertyInfo.GetValue(model);
                if (value != null)
                {
                    var iOrmModel = value as IOrmModel;
                    if (iOrmModel.OrmInterceptor.ORMID.IsEmpty())
                        Insert(iOrmModel);
                    cmd.InsertPairs.Add(new ColumnValuePair
                    {
                        Column = foreignOne.Column,
                        Value = iOrmModel.OrmInterceptor.ORMID
                    });
                }
            }

            #endregion

            //防止对象在之前操作中被寫入
            if (!model.OrmInterceptor.ORMID.IsEmpty())
                return;

            #region 属性

            foreach (var pe in classModel.Property)
            {
                var value = pe.Value.PropertyInfo.GetValue(model);
                var notify = interceptor.NotifyProperites[pe.Key];
                if (notify.IsChanged)
                {
                    //如果是default属性则不插入，对于valuetype的default，
                    //在查询时，对于dbnull类型不操作
                    cmd.InsertPairs.Add(new ColumnValuePair
                    {
                        Column = pe.Value.Column,
                        Value = value
                    });
                }
            }

            #endregion

            #region 完成写入

            var guid = Guid.NewGuid();
            cmd.InsertPairs.Add(new ColumnValuePair
            {
                Column = IdColumn,
                Value = guid
            });
            cmd.TableName = classModel.Table;
            ExecuteSql(cmd);
            if (IsLinked)
            {
               classModel.IdPropertyInfo.SetValue(model,guid);  
            }
            interceptor.ORMID = guid;
            primaryKey = guid;
            Cache[type].Add(guid, model);

            #endregion

            #region 多对多

            foreach (var manytoMany in classModel.ManyToMany.Values)
            {
                var value = manytoMany.PropertyInfo.GetValue(model);
                if (value == null)
                    continue;
                if (manytoMany.IsNeedUpdate)
                    InsertAllManytoMany(manytoMany, (IEnumerable) value, primaryKey);
            }

            #endregion

            #region 一对一主键

            foreach (var onetoOne in classModel.OneToOne.Values)
            {
                var value = onetoOne.PropertyInfo.GetValue(model);
                if (value == null)
                    continue;
                if (onetoOne.IsNeedUpdate)
                    InsertOneToOne(onetoOne, primaryKey, value);
            }

            #endregion

            #region 一对多

            foreach (var onetoMany in classModel.OneToMany.Values)
            {
                var value = (IEnumerable) onetoMany.PropertyInfo.GetValue(model);
                if (onetoMany.IsNeedUpdate)
                    InsertAllOnetoMany(onetoMany, value, primaryKey);
            }

            #endregion

            foreach (var notifyProperty in interceptor.NotifyProperites.Values)
            {
                notifyProperty.IsChanged = false;
                notifyProperty.OperatersList.Clear();
            }
        }

        
        // 防止拦截器自动发射更改
        private object DeSerialize(DataRow record, Type type) {
            //if (type.Name== "Examination") {
            //    Debugger.Break();
            //}
            var mainClassDefine = ClassModels[type];
            var mainId = (Guid)record[IdColumn];
           
            if (Cache[type].ContainsKey(mainId))
                return Cache[type][mainId];
            //生成不记录更改的对象
            var instance = ProxyFactory.Get(type, false);
            var interceptor = ForceBuildFactory.GetInterceptor<OrmInterceptor>(instance);
            interceptor.IsRecordable = false;
            if (IsLinked)
                mainClassDefine.IdPropertyInfo.SetValue(instance, mainId);
            foreach (var propertyElement in mainClassDefine.Property.Values) {
                if (record[propertyElement.Column] is DBNull)
                    continue;
                var targetType = propertyElement.PropertyInfo.PropertyType;
                var data = record[propertyElement.Column];
                object value = null;
                if (propertyElement.IsEnum) {
                    value = Enum.Parse(targetType, data.ToString());
                }
                else if (propertyElement.IsNullable) {
                    value=Convert.ChangeType(data,propertyElement.NullableBaseType);
                }
                else {
                    value=Convert.ChangeType(data, propertyElement.PropertyInfo.PropertyType);
                }
                propertyElement.PropertyInfo.SetValue(instance, value);
            }
            //放入cache，标记该对象已被取出
            Cache[type].Add(mainId, instance);

            #region 一对一外键

            foreach (var foreignOne in mainClassDefine.ForeignOne.Values) {
                var cellValue = record[foreignOne.Column];
                //mssql和datatable读上来的int类型为空则为空字符串
                if (cellValue is DBNull)
                    continue;
                var propertyValue = Get((Guid)cellValue, foreignOne.ReferClass.ClassType);
                foreignOne.PropertyInfo.SetValue(instance, propertyValue);
            }

            #endregion

            #region 多对一

            foreach (var manytoOne in mainClassDefine.ManyToOne.Values) {
                var cellValue = record[manytoOne.Column];
                if (cellValue is DBNull)
                    continue;
                var foreignkey = (Guid)cellValue;
                var referClassClassType = manytoOne.ReferClass.ClassType;
                var value = Get(foreignkey, referClassClassType);
                manytoOne.PropertyInfo.SetValue(instance, value);
            }

            #endregion

            #region 一对一主键

            foreach (var onetoOne in mainClassDefine.OneToOne.Values) {
                var classDefine = onetoOne.ReferClass;
                var selectCommand = new SelectCommand {TableName = classDefine.Table};
                selectCommand.ConditionPairs.Add(new ColumnValuePair {
                    Column = onetoOne.Column,
                    Value = mainId
                });
                var dataTable = Read(selectCommand);
                if (dataTable.Rows.Count == 0)
                    continue;
                var dataline = dataTable.Rows[0];
                var primaryKey = (Guid)dataline[IdColumn];
                onetoOne.PropertyInfo.SetValue(instance,
                    Cache[classDefine.ClassType].ContainsKey(primaryKey)
                        ? Cache[classDefine.ClassType][primaryKey]
                        : DeSerialize(dataline, classDefine.ClassType));
            }

            #endregion

            #region 一对多

            foreach (var onetoMany in mainClassDefine.OneToMany.Values)
            {
                var referClassClassType = onetoMany.ReferClass.ClassType;
                var selectCommand = new SelectCommand {TableName = onetoMany.ReferClass.Table};
                selectCommand.ConditionPairs.Add(new ColumnValuePair
                {
                    Column = onetoMany.ReferColumn,
                    Value = mainId
                });
                var dataTable = Read(selectCommand);
                if (dataTable.Rows.Count == 0)
                    continue;
                //var value = onetoMany.PropertyInfo.GetValue(instance);
                //if (value!=null)
                //{
                /* 添加到集合,考虑三种方案：
                 * 一是调用ilist接口的add或ilist的add
                 * 二是使用集合类型往往会具有的构造函数注入
                 * 由于对象在首次创建过程中的更改即会被发射到更新器里，
                 * 所以不用担心取出时对构造函数操作的覆盖；
                 * 三是根据字典确定方法{类似hibernate，对指定的集合有对应的实现}
                */
                var list = new ArrayList(dataTable.Rows.Count);
                foreach (DataRow dataline in dataTable.Rows)
                {
                    var childobj = DeSerialize(dataline, referClassClassType);
                    list.Add(childobj);
                }
                onetoMany.PropertyInfo
                    .SetValue(instance,
                        list.ToSpecificCollection(onetoMany.PropertyInfo
                            .PropertyType));
            }

            #endregion

            #region 多对多

            foreach (var manytoMany in mainClassDefine.ManyToMany.Values) {
                var referClassClassType = manytoMany.ReferClass.ClassType;
                var selectCommand = new SelectCommand {TableName = manytoMany.Table};
                selectCommand.ConditionPairs.Add(new ColumnValuePair {
                    Column = manytoMany.Column,
                    Value = mainId
                });
                var dataTable = Read(selectCommand);
                if (dataTable.Rows.Count == 0)
                    continue;
                var list=new ArrayList(dataTable.Rows.Count);
                foreach (DataRow row in dataTable.Rows)
                {
                    var foreignKey = (Guid)row[manytoMany.ReferColumn];
                    var childobj = Get(foreignKey, referClassClassType);
                    list.Add(childobj);
                }
                manytoMany.PropertyInfo.SetValue(instance,
                    list.ToSpecificCollection(manytoMany.PropertyInfo.PropertyType));
            }

            #endregion

            //允许记录更改
            
            interceptor.IsRecordable = true;
            //允许发射更改
            ((IOrmModel)instance).OrmInterceptor.ORMID = mainId;
            return instance;
        }

        public void ForceBuildFactory_AgentPreparation(GenerateEventArgs args) {
            if (!ClassModels.ContainsKey(args.ToProxyType)) {
                throw new ArgumentException("未定义的类，无法创建实例");
            }
            var define = ClassModels[args.ToProxyType];
            var list = define.AllProperties.ToConcurrencyDictionary(property => property.Key,
                property => new NotifyProperty {PropertyElement = property.Value});
            var interceptor = new OrmInterceptor {
                NotifyProperites = list,
                Dispatcher = Scheduler,
                IsRecordable = true
            };
            var mix = new OrmMix(interceptor,define);
            args.Interceptors.Add(interceptor);
            args.GenerationOptions.AddMixinInstance(mix);
        }
        
        private OrmInterceptor GetOrmInterceptor(Type type) {
            var define = ClassModels[type];
            var list = define.AllProperties.ToConcurrencyDictionary(property => property.Key,
                property => new NotifyProperty {PropertyElement = property.Value});
            var interceptor = new OrmInterceptor {
                NotifyProperites = list,
                Dispatcher = Scheduler,
                IsRecordable = true
            };
            return interceptor;
        }

        public virtual object Get(Guid ormid, Type type) {
                if (Cache[type].ContainsKey(ormid))
                    return Cache[type][ormid];
                var classModel = ClassModels[type];
                var selectCommand = new SelectCommand {TableName = classModel.Table};
                selectCommand.ConditionPairs.Add(new ColumnValuePair {
                    Column = IdColumn,
                    Value = ormid
                });
                var dt = Read(selectCommand);
                return dt.Rows.Count == 0 ? null : DeSerialize(dt.Rows[0], type);
        }

        /* 对于集合引用更新，四个策略：
        * 1.删除所有数据库现存关系，然后写入内存中现存关系
        * 2.去重保存的collectionchanged操作后写入数据库
        * 3.(已过时)同内存中保留的以前引用集合id表对比，对比后决定操作
        * 4.(已过时)先读入当前数据库id，对比后决定操作
        reset和propertychanged后，第一种较好，较少操作时，第二种较好*/

        public virtual void Update(IOrmModel model) {
                var primaryKey = model.OrmInterceptor.ORMID;
                //防止被未插入对象使用
                if (primaryKey.IsEmpty())
                    return;
            var notifyCell = model.OrmInterceptor;//IForceBuildFactory.GetInterceptor<OrmInterceptor>(model);
                var classmodel = model.ClassDefine;
                var command = new UpdateCommand {TableName = classmodel.Table};
                //分离interceptor
                foreach (var property in notifyCell.NotifyProperites.Values) {
                    if (property.IsChanged) {
                        var element = property.PropertyElement;
                        var value = property.PropertyElement.PropertyInfo.GetValue(model);
                        property.IsChanged = false;

                        #region 值属性

                        if (element.RelationType == RelationType.Value) {
                            command.UpdatePairs.Add(new ColumnValuePair {
                                Column = element.Column,
                                Value = value
                            });
                        }

                        #endregion

                        #region 一对一外键,只更新自身

                        else if (element.RelationType == RelationType.ForeignOne) {
                            if (value != null) {
                                var iOrmCell = value as IOrmModel;
                                if (iOrmCell.OrmInterceptor.ORMID.IsEmpty())
                                    Insert(iOrmCell);
                                command.UpdatePairs.Add(new ColumnValuePair {
                                    Column = element.Column,
                                    Value = iOrmCell.OrmInterceptor.ORMID
                                });
                            }
                            else {
                                command.UpdatePairs.Add(new ColumnValuePair {
                                    Column = element.Column,
                                    Value = DBNull.Value
                                });
                            }
                        }

                        #endregion

                        #region 多对一 ，只更新自身

                        else if (element.RelationType == RelationType.ManyToOne) {
                            if (value != null) {
                                var iOrmModel = value as IOrmModel;
                                if (iOrmModel.OrmInterceptor.ORMID.IsEmpty())
                                    Insert(iOrmModel);
                                command.UpdatePairs.Add(new ColumnValuePair {
                                    Column = element.Column,
                                    Value = iOrmModel.OrmInterceptor.ORMID
                                });
                            }
                            else {
                                command.UpdatePairs.Add(new ColumnValuePair {
                                    Column = element.Column,
                                    Value = DBNull.Value
                                });
                            }
                        }

                        #endregion

                        #region 一对一主键

                        else if (element.RelationType == RelationType.OneToOne) //
                        {
                            var onetoOne = element as OnetoOne;
                            var referDefine = onetoOne.ReferClass;
                            //先将原先一对一值置为空，然后更新关系，因为一对一主键可能是以外键形式约束
                            if (onetoOne.IsNeedUpdate) {
                                var beforeUpdate = new UpdateCommand {TableName = referDefine.Table};
                                beforeUpdate.ConditionPairs.Add(new ColumnValuePair {
                                    Column = onetoOne.Column,
                                    Value = primaryKey
                                });
                                beforeUpdate.UpdatePairs.Add(new ColumnValuePair {
                                    Column = onetoOne.Column,
                                    Value = DBNull.Value
                                });
                                ExecuteSql(beforeUpdate);
                                if (value != null)
                                    InsertOneToOne(onetoOne, primaryKey, value);
                            }
                        }

                        #endregion

                        #region 一对多 集合重新赋值

                        else if (element.RelationType == RelationType.OneToMany) {
                            var onetoMany = element as OnetoMany;
                            if (onetoMany.IsNeedUpdate) {
                                DeleteAllOnetoMany(onetoMany, primaryKey);
                                if (value != null)
                                    InsertAllOnetoMany(onetoMany, (IEnumerable) value, primaryKey);
                            }
                            property.OperatersList.Clear();
                        }

                        #endregion

                        #region 多对多 集合重新赋值

                        else {
                            var manytoMany = element as ManytoMany;
                            if (manytoMany.IsNeedUpdate) {
                                DeleteAllManytoMany(manytoMany, primaryKey);
                                if (value != null)
                                    InsertAllManytoMany(manytoMany, (IEnumerable) value, primaryKey);
                            }
                            property.OperatersList.Clear();
                        }

                        #endregion
                    }
                    else {
                        if (property.OperatersList.Count != 0) {
                            var value = property.PropertyElement.PropertyInfo.GetValue(model);
                            var element = property.PropertyElement;
                            //有operate说明val不为null

                            #region 多对多

                            if (element.RelationType == RelationType.ManyToMany) {
                                var manytoMany = element as ManytoMany;
                                if (manytoMany.IsNeedUpdate) {
                                    //检查是否有reset标志
                                    if (property.OperatersList.Any(eventArgse =>
                                        eventArgse.Action == NotifyCollectionChangedAction.Reset)) {
                                        DeleteAllManytoMany(manytoMany, primaryKey);
                                        InsertAllManytoMany(manytoMany, (IEnumerable) value, primaryKey);
                                    }
                                    else {
                                        //   var referclassDefine = ((ManytoMany) element).ReferClass;
                                        //根据event做原子操作
                                        foreach (var argse in property.OperatersList) {
                                            switch (argse.Action) {
                                                case NotifyCollectionChangedAction.Add:
                                                    InsertAllManytoMany(manytoMany, argse.NewItems, primaryKey);
                                                    break;
                                                case NotifyCollectionChangedAction.Remove:
                                                    foreach (var oldItem in argse.OldItems) {
                                                        var id = ((IOrmModel)oldItem).OrmInterceptor.ORMID;
                                                        if (id.IsEmpty())
                                                            continue;
                                                        //舊對象不存在不需要更新
                                                        var deleteCommand = new DeleteCommand {TableName = manytoMany.Table};
                                                        deleteCommand.ConditionPairs.Add(new ColumnValuePair {
                                                            Column = manytoMany.Column,
                                                            Value = primaryKey
                                                        });
                                                        deleteCommand.ConditionPairs.Add(new ColumnValuePair {
                                                            Column = manytoMany.ReferColumn,
                                                            Value = id
                                                        });
                                                        ExecuteSql(deleteCommand);
                                                    }
                                                    break;
                                                case NotifyCollectionChangedAction.Replace:
                                                    foreach (var oldItem in argse.OldItems) {
                                                        var id = ((IOrmModel)oldItem).OrmInterceptor.ORMID;
                                                        if (id.IsEmpty())
                                                            continue;
                                                        var deleteCommand = new DeleteCommand {TableName = manytoMany.Table};
                                                        deleteCommand.ConditionPairs.Add(new ColumnValuePair {
                                                            Column = manytoMany.Column,
                                                            Value = primaryKey
                                                        });
                                                        deleteCommand.ConditionPairs.Add(new ColumnValuePair {
                                                            Column = manytoMany.ReferColumn,
                                                            Value = id.IsEmpty()
                                                        });
                                                        ExecuteSql(deleteCommand);
                                                    }
                                                    InsertAllManytoMany(manytoMany, argse.NewItems, primaryKey);
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }

                            #endregion

                            #region 一对多

                            else {
                                var onetoMany = element as OnetoMany;
                                if (onetoMany.IsNeedUpdate) {
                                    //检查reset
                                    if (property.OperatersList.Any(eventArgse =>
                                        eventArgse.Action == NotifyCollectionChangedAction.Reset)) {
                                        DeleteAllOnetoMany(onetoMany, primaryKey);
                                        InsertAllOnetoMany(onetoMany, (IEnumerable) value, primaryKey);
                                    }
                                    else {
                                        foreach (var argse in property.OperatersList) {
                                            switch (argse.Action) {
                                                case NotifyCollectionChangedAction.Add:
                                                    InsertAllOnetoMany(onetoMany, argse.NewItems, primaryKey);
                                                    break;
                                                case NotifyCollectionChangedAction.Remove:
                                                    foreach (var oldItem in argse.OldItems) {
                                                        var updateCommand = new UpdateCommand {TableName = onetoMany.ReferClass.Table,};
                                                        updateCommand.UpdatePairs.Add(new ColumnValuePair {
                                                            Column = onetoMany.ReferColumn,
                                                            Value = DBNull.Value
                                                        });
                                                        updateCommand.ConditionPairs.Add(new ColumnValuePair {
                                                            Column = onetoMany.ReferColumn,
                                                            Value = ((IOrmModel) oldItem).OrmInterceptor.ORMID
                                                        });
                                                        ExecuteSql(updateCommand);
                                                    }
                                                    break;
                                                case NotifyCollectionChangedAction.Replace:
                                                    InsertAllOnetoMany(onetoMany, argse.NewItems, primaryKey);
                                                    foreach (var oldItem in argse.OldItems) {
                                                        var updateCommand = new UpdateCommand {TableName = onetoMany.ReferClass.Table,};
                                                        updateCommand.UpdatePairs.Add(new ColumnValuePair {
                                                            Column = onetoMany.ReferColumn,
                                                            Value = DBNull.Value
                                                        });
                                                        updateCommand.ConditionPairs.Add(new ColumnValuePair {
                                                            Column = onetoMany.ReferColumn,
                                                            Value = ((IOrmModel) oldItem).OrmInterceptor.ORMID
                                                        });
                                                        ExecuteSql(updateCommand);
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }

                            #endregion

                            property.OperatersList.Clear();
                        }

                    }
                }
                if (command.UpdatePairs.Count > 0) {
                    command.ConditionPairs.Add(new ColumnValuePair {
                        Column = IdColumn,
                        Value = primaryKey
                    });
                    ExecuteSql(command);
                }
        }

        private void InsertOneToOne(OnetoOne onetoOne, Guid pk, object oc) {
            var iOrmModel = oc as IOrmModel;
            if (iOrmModel.OrmInterceptor.ORMID.IsEmpty())
                Insert(iOrmModel);
            var updateCommand = new UpdateCommand {TableName = onetoOne.ReferClass.Table};
            updateCommand.ConditionPairs.Add(new ColumnValuePair {
                Column = IdColumn,
                Value = iOrmModel.OrmInterceptor.ORMID
            });
            updateCommand.UpdatePairs.Add(new ColumnValuePair {
                
                Column = onetoOne.Column,
                Value = pk
            });
            ExecuteSql(updateCommand);
        }

        private void DeleteAllOnetoMany(OnetoMany onetoMany, Guid pk) {
            var updateCommand = new UpdateCommand {TableName = onetoMany.ReferClass.Table,};
            updateCommand.UpdatePairs.Add(new ColumnValuePair {
                Column = onetoMany.ReferColumn,
                Value = DBNull.Value
            });
            updateCommand.ConditionPairs.Add(new ColumnValuePair {
                Column = onetoMany.ReferColumn,
                Value = pk
            });
            ExecuteSql(updateCommand);
        }

        private void InsertAllOnetoMany(OnetoMany onetoMany, IEnumerable preCollection, Guid pk) {
           
            foreach (var item in preCollection) {
                var iOrmModel = item as IOrmModel;
                var updateCommand = new UpdateCommand {TableName = onetoMany.ReferClass.Table};
                if (iOrmModel.OrmInterceptor.ORMID.IsEmpty())
                    Insert(iOrmModel);
                updateCommand.ConditionPairs.Add(new ColumnValuePair {
                    Column = IdColumn,
                    Value = iOrmModel.OrmInterceptor.ORMID
                });
                updateCommand.UpdatePairs.Add(new ColumnValuePair {
                    Column = onetoMany.ReferColumn,
                    Value = pk
                });
                ExecuteSql(updateCommand);
            }

        }

        private void DeleteAllManytoMany(ManytoMany manytoMany, Guid pk) {
            var deleteCommand = new DeleteCommand {TableName = manytoMany.Table};
            deleteCommand.ConditionPairs.Add(new ColumnValuePair {
                Column = manytoMany.Column,
                Value = pk
            });
            ExecuteSql(deleteCommand);
        }

        private void InsertAllManytoMany(ManytoMany manytoMany, IEnumerable collection, Guid pk) {
            foreach (var item in collection) {
                var ormid = ((IOrmModel)item).OrmInterceptor.ORMID;
                if (ormid.IsEmpty())
                    Insert(item as IOrmModel);
                var insertCommand = new InsertCommand {TableName = manytoMany.Table,};
                insertCommand.InsertPairs.Add(new ColumnValuePair {
                    Column = manytoMany.Column,
                    Value = pk
                });
                insertCommand.InsertPairs.Add(new ColumnValuePair {
                    Column = manytoMany.ReferColumn,
                    Value = ormid
                });
                ExecuteSql(insertCommand);
            }
        }

        public virtual void Delete(IOrmModel model) {
                var classDefine = model.ClassDefine;
                if (!Cache[classDefine.ClassType].ContainsValue(model))
                    return;
                var ormid = model.OrmInterceptor.ORMID;
                var deleteCommand = new DeleteCommand {TableName = classDefine.Table};
                deleteCommand.ConditionPairs.Add(new ColumnValuePair {
                    Column = IdColumn,
                    Value = ormid
                });
                ExecuteSql(deleteCommand);
                Cache[classDefine.ClassType].Remove(ormid);
            
        }

        public virtual T[] Select<T>(string sql) {
                var dataTable = Read(sql);
                return dataTable.Rows.Count == 0
                    ? null
                    : dataTable.Rows.Cast<DataRow>().Select(dr => (T) DeSerialize(dr, typeof(T))).ToArray();
            
        }

        public virtual void ClearTable(Type type) {
            var classModel = ClassModels[type];
            var command = new DeleteCommand {TableName = classModel.Table};
            ExecuteSql(command);
        }

        public virtual T[] LoadAll<T>() {
            
                var dt = Read(new SelectCommand {TableName = ClassModels[typeof(T)].Table});
                return dt.Rows.Cast<DataRow>().Select(x => (T) DeSerialize(x, typeof(T))).ToArray();
            
        }

        public virtual T[] GetByProperty<T>(string[] attributies, object[] parameters) {
            
                var dc = ClassModels[typeof(T)];
                var sc = new SelectCommand {TableName = dc.Table};
                for (var i = 0; i < attributies.Length; i++) {
                    sc.ConditionPairs.Add(new ColumnValuePair {
                        Column = attributies[i],
                        Value = parameters[i]
                    });
                }
                var dt = Read(sc);
                return (from DataRow x in dt.Rows select DeSerialize(x, typeof(T)) into obj select (T) obj).ToArray();
            
        }

        public virtual void Delete(IOrmModel model, string property) {
            
                var deleteCommand = new DeleteCommand();
                var select = model.ClassDefine.AllProperties.Keys.FirstOrDefault(key => key == property);
                if (select == null) {
                    throw new ArgumentException("要删除的属性不存在");
                }
                var propertyElement = model.ClassDefine.AllProperties[select];
                switch (propertyElement.RelationType) {
                    case RelationType.OneToMany:
                        var onetoMany = propertyElement as OnetoMany;
                        deleteCommand.TableName = onetoMany.ReferClass.Table;
                        deleteCommand.ConditionPairs.Add(new ColumnValuePair {
                            Column = onetoMany.ReferColumn,
                            Value = model.OrmInterceptor.ORMID
                        });
                        break;
                    case RelationType.ManyToMany:
                        var manytoMany = propertyElement as ManytoMany;
                        deleteCommand.TableName = manytoMany.Table;
                        deleteCommand.ConditionPairs.Add(new ColumnValuePair {
                            Column = manytoMany.Column,
                            Value = model.OrmInterceptor.ORMID
                        });
                        break;
                    default:
                        return;
                        throw new ArgumentException("此方法仅用于集合关系删除");
                }
                ExecuteSql(deleteCommand);
        }

        public virtual void Close() {

        }
    }
}

//public T[] Select<T>(T pattern,string[] attributes)
//{
//    Type type = typeof(T);
//    DryClass dc=ClassModelCollection[type];
//    SelectCommand sc = new SelectCommand() { tableName = dc.Table };
//    List<string> values = new List<string>();
//    List<string> columns = new List<string>();
//    foreach (var str in attributes)
//    {
//        values.Add("'" + type.GetProperty(str).GetValue(pattern).ToString() + "'");
//        columns.Add()
//    }
//}

//#region 
//foreach (var fm in ClassModel.ForeignMany)//数组形式
//{
//    PropertyInfo pi = type.GetProperty(fm.Key);
//    ICollection co = (ICollection)pi.GetValue(model);
//    DryClass dc = fm.Value.referclass;
//    PropertyInfo cp = dc.ClassType.GetProperty(dc.ID.name);
//    int referID;
//    foreach (var x in co)
//    {
//        if (!Cache[fm.Value.referclass.ClassType].ContainsValue(x))
//        {
//            Insert(x);
//        }
//        referID = (int)cp.GetValue(x);
//        sql = SQLSentence.Insert(fm.Value.table, new string[2] { fm.Value.column, fm.Value.refercolumn }, new string[2] { guid.ToString(), referID.ToString()});
//        ExecuteSQL(sql);
//    }
//}
//#endregion
///// <summary>
///// 基于orm id，已弃用
///// </summary>
///// <param name="type"></param>
///// <param name="id"></param>
//protected void Delete(Type type, int id)
//{
//    if (!Cache[type].ContainsKey(id))
//        return;
//    var classmodel = ClassModels[type];
//    var dc = new DeleteCommand {TableName = classmodel.Table,};
//    dc.ConditionPairs.Add(new ColumnValuePair
//    {
//        Column = Idcolumn,
//        Value = id
//    });
//    ExecuteSql(dc);
//    Cache[type].Remove(id);
//}