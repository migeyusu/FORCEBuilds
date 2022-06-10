using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Linq;
using FORCEBuild.Core;
using FORCEBuild.Crosscutting.Log;
using FORCEBuild.Helper;
using FORCEBuild.Persistence.Configuration;
using Microsoft.Extensions.Logging;

namespace FORCEBuild.Persistence
{
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
    /// 连接数据库并执行sql，实现类将对应不同的数据库
    /// </summary>
    internal abstract class Accessor : IFactoryProxyPreparation
    {
        /// <summary>
        /// 对应数据库中的id列名，用于跟踪对象
        /// </summary>
        public const string IdColumnName = "guid";

        public OrmConfig Config
        {
            set
            {
                IsLinked = value.IsLinked;
                ClassDefines = value.ClassDefines;
                ConnectionStringBuilder = value.ConnectionStringBuilder;
            }
        }

        /// <summary>
        /// 是否绑定model的id。如果绑定则更新
        /// </summary>
        public bool IsLinked { get; set; }

        public ILogger<Accessor> Log { get; set; }

        public DbConnectionStringBuilder ConnectionStringBuilder { get; set; }

        private ConcurrentDictionary<Type, ClassDefine> _classDefines;

        public ConcurrentDictionary<Type, ClassDefine> ClassDefines
        {
            private get => _classDefines;
            set
            {
                _classDefines = value;
                if (ObjectCache == null)
                    ObjectCache = new ConcurrentDictionary<Type, Dictionary<Guid, object>>();
                else
                    ObjectCache.Clear();
                foreach (var x in value)
                    ObjectCache.TryAdd(x.Key, new Dictionary<Guid, object>());
            }
        }

        //保持已插入和取出对象的引用
        public ConcurrentDictionary<Type, Dictionary<Guid, object>> ObjectCache { get; set; }

        public ProxyFactory ProxyFactory { private get; set; }

        protected abstract void ExecuteSql(DataBaseCommand ac);

        public abstract IDbTransaction BeginTransaction();

        protected abstract DataTable Read(SelectCommand ac);

        protected abstract DataTable Read(string sql);

        public virtual void Insert(IOrmModel model)
        {
            var interceptor = model.OrmInterceptor; //IForceBuildFactory.GetInterceptor<OrmInterceptor>(model);
            var type = model.ClassDefine.ClassType;
            if (interceptor.ID != Guid.Empty)
                return;
            var classModel = model.ClassDefine;

            var cmd = new InsertCommand();

            #region 多对一

            //首先写入外键类型对应的已存在对象外键
            foreach (var manytoOne in classModel.ManyToOne.Values)
            {
                var value = manytoOne.PropertyInfo.GetValue(model);
                if (value != null) //对象为空不插入，相应的，数据库需要允许外键为空
                {
                    var iOrmModel = value as IOrmModel;
                    if (iOrmModel.OrmInterceptor.ID.IsEmpty())
                    {
                        Insert(iOrmModel);
                    }

                    cmd.InsertPairs.Add(new ColumnValuePair
                    {
                        Column = manytoOne.Column,
                        Value = iOrmModel.OrmInterceptor.ID
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
                    if (iOrmModel.OrmInterceptor.ID.IsEmpty())
                        Insert(iOrmModel);
                    cmd.InsertPairs.Add(new ColumnValuePair
                    {
                        Column = foreignOne.Column,
                        Value = iOrmModel.OrmInterceptor.ID
                    });
                }
            }

            #endregion

            //防止对象在之前操作中被写入
            if (!model.OrmInterceptor.ID.IsEmpty())
                return;

            #region 属性

            foreach (var pe in classModel.Property)
            {
                var value = pe.Value.PropertyInfo.GetValue(model);
                var notify = interceptor.NotifyProperties[pe.Key];
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

            var primaryKey = Guid.NewGuid();
            cmd.InsertPairs.Add(new ColumnValuePair
            {
                Column = IdColumnName,
                Value = primaryKey
            });
            cmd.TableName = classModel.Table;
            ExecuteSql(cmd);
            if (IsLinked)
            {
                classModel.IdPropertyInfo.SetValue(model, primaryKey);
            }

            interceptor.ID = primaryKey;

            ObjectCache[type].Add(primaryKey, model);

            #endregion

            #region 多对多

            foreach (var manyToMany in classModel.ManyToMany.Values)
            {
                var value = manyToMany.PropertyInfo.GetValue(model);
                if (value == null)
                    continue;
                if (manyToMany.IsNeedUpdate)
                    InsertAllManyToMany(manyToMany, (IEnumerable)value, primaryKey);
            }

            #endregion

            #region 一对一主键

            foreach (var oneToOne in classModel.OneToOne.Values)
            {
                var value = oneToOne.PropertyInfo.GetValue(model);
                if (value == null)
                    continue;
                if (oneToOne.IsNeedUpdate)
                    InsertOneToOne(oneToOne, primaryKey, value);
            }

            #endregion

            #region 一对多

            foreach (var oneToMany in classModel.OneToMany.Values)
            {
                var value = (IEnumerable)oneToMany.PropertyInfo.GetValue(model);
                if (oneToMany.IsNeedUpdate)
                    InsertAllOneToMany(oneToMany, value, primaryKey);
            }

            #endregion

            foreach (var notifyProperty in interceptor.NotifyProperties.Values)
            {
                notifyProperty.IsChanged = false;
                notifyProperty.OperatersList.Clear();
            }
        }

        // 防止拦截器自动发射更改
        private object DeSerialize(DataRow record, Type type)
        {
            var mainClassDefine = ClassDefines[type];
            var mainId = (Guid)record[IdColumnName];

            if (ObjectCache[type].ContainsKey(mainId))
                return ObjectCache[type][mainId];
            //生成不记录更改的对象
            var instance = ProxyFactory.CreateProxyClass(type, false);
            var interceptor = ProxyFactory.GetInterceptor<OrmInterceptor>(instance);
            interceptor.IsRecordable = false;
            if (IsLinked)
                mainClassDefine.IdPropertyInfo.SetValue(instance, mainId);
            foreach (var propertyElement in mainClassDefine.Property.Values)
            {
                if (record[propertyElement.Column] is DBNull)
                    continue;
                var targetType = propertyElement.PropertyInfo.PropertyType;
                var data = record[propertyElement.Column];
                object value = null;
                if (propertyElement.IsEnum)
                {
                    value = Enum.Parse(targetType, data.ToString());
                }
                else if (propertyElement.IsNullable)
                {
                    value = Convert.ChangeType(data, propertyElement.NullableBaseType);
                }
                else
                {
                    value = Convert.ChangeType(data, propertyElement.PropertyInfo.PropertyType);
                }

                propertyElement.PropertyInfo.SetValue(instance, value);
            }

            //放入cache，标记该对象已被取出
            ObjectCache[type].Add(mainId, instance);

            #region 一对一外键

            foreach (var foreignOne in mainClassDefine.ForeignOne.Values)
            {
                var cellValue = record[foreignOne.Column];
                //mssql和datatable读上来的int类型为空则为空字符串
                if (cellValue is DBNull)
                    continue;
                var propertyValue = Get((Guid)cellValue, foreignOne.ReferClass.ClassType);
                foreignOne.PropertyInfo.SetValue(instance, propertyValue);
            }

            #endregion

            #region 多对一

            foreach (var manytoOne in mainClassDefine.ManyToOne.Values)
            {
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

            foreach (var onetoOne in mainClassDefine.OneToOne.Values)
            {
                var classDefine = onetoOne.ReferClass;
                var selectCommand = new SelectCommand { TableName = classDefine.Table };
                selectCommand.ConditionPairs.Add(new ColumnValuePair
                {
                    Column = onetoOne.Column,
                    Value = mainId
                });
                var dataTable = Read(selectCommand);
                if (dataTable.Rows.Count == 0)
                    continue;
                var dataline = dataTable.Rows[0];
                var primaryKey = (Guid)dataline[IdColumnName];
                onetoOne.PropertyInfo.SetValue(instance,
                    ObjectCache[classDefine.ClassType].ContainsKey(primaryKey)
                        ? ObjectCache[classDefine.ClassType][primaryKey]
                        : DeSerialize(record: dataline, type: classDefine.ClassType));
            }

            #endregion

            #region 一对多

            foreach (var onetoMany in mainClassDefine.OneToMany.Values)
            {
                var referClassClassType = onetoMany.ReferClass.ClassType;
                var selectCommand = new SelectCommand { TableName = onetoMany.ReferClass.Table };
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

            foreach (var manytoMany in mainClassDefine.ManyToMany.Values)
            {
                var referClassClassType = manytoMany.ReferClass.ClassType;
                var selectCommand = new SelectCommand { TableName = manytoMany.Table };
                selectCommand.ConditionPairs.Add(new ColumnValuePair
                {
                    Column = manytoMany.Column,
                    Value = mainId
                });
                var dataTable = Read(selectCommand);
                if (dataTable.Rows.Count == 0)
                    continue;
                var list = new ArrayList(dataTable.Rows.Count);
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
            ((IOrmModel)instance).OrmInterceptor.ID = mainId;
            return instance;
        }

        public void GeneratePreparation(PreProxyEventArgs args)
        {
            if (!ClassDefines.ContainsKey(args.ToProxyType))
            {
                throw new ArgumentException("未定义的类，无法创建实例");
            }

            var define = ClassDefines[args.ToProxyType];
            var list = define.AllProperties.ToConcurrencyDictionary(property => property.Key,
                property => new NotifyProperty { PropertyElement = property.Value });
            var interceptor = new OrmInterceptor
            {
                NotifyProperties = list,
                IsRecordable = true
            };
            var mix = new OrmMix(interceptor, define);
            args.Interceptors.Add(interceptor);
            args.GenerationOptions.AddMixinInstance(mix);
        }

        public virtual object Get(Guid ormid, Type type)
        {
            if (ObjectCache[type].ContainsKey(ormid))
                return ObjectCache[type][ormid];
            var classModel = ClassDefines[type];
            var selectCommand = new SelectCommand { TableName = classModel.Table };
            selectCommand.ConditionPairs.Add(new ColumnValuePair
            {
                Column = IdColumnName,
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

        public virtual void Update(IOrmModel model)
        {
            var primaryKey = model.OrmInterceptor.ID;
            //防止被未插入对象使用
            if (primaryKey.IsEmpty())
                return;
            var notifyCell = model.OrmInterceptor; //IForceBuildFactory.GetInterceptor<OrmInterceptor>(model);
            var classmodel = model.ClassDefine;
            var command = new UpdateCommand { TableName = classmodel.Table };
            //分离interceptor
            foreach (var property in notifyCell.NotifyProperties.Values)
            {
                if (property.IsChanged)
                {
                    var element = property.PropertyElement;
                    var value = property.PropertyElement.PropertyInfo.GetValue(model);
                    property.IsChanged = false;

                    #region 值属性

                    if (element.RelationType == RelationType.Value)
                    {
                        command.UpdatePairs.Add(new ColumnValuePair
                        {
                            Column = element.Column,
                            Value = value
                        });
                    }

                    #endregion

                    #region 一对一外键,只更新自身

                    else if (element.RelationType == RelationType.ForeignOne)
                    {
                        if (value != null)
                        {
                            var iOrmCell = value as IOrmModel;
                            if (iOrmCell.OrmInterceptor.ID.IsEmpty())
                                Insert(iOrmCell);
                            command.UpdatePairs.Add(new ColumnValuePair
                            {
                                Column = element.Column,
                                Value = iOrmCell.OrmInterceptor.ID
                            });
                        }
                        else
                        {
                            command.UpdatePairs.Add(new ColumnValuePair
                            {
                                Column = element.Column,
                                Value = DBNull.Value
                            });
                        }
                    }

                    #endregion

                    #region 多对一 ，只更新自身

                    else if (element.RelationType == RelationType.ManyToOne)
                    {
                        if (value != null)
                        {
                            var iOrmModel = value as IOrmModel;
                            if (iOrmModel.OrmInterceptor.ID.IsEmpty())
                                Insert(iOrmModel);
                            command.UpdatePairs.Add(new ColumnValuePair
                            {
                                Column = element.Column,
                                Value = iOrmModel.OrmInterceptor.ID
                            });
                        }
                        else
                        {
                            command.UpdatePairs.Add(new ColumnValuePair
                            {
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
                        if (onetoOne.IsNeedUpdate)
                        {
                            var beforeUpdate = new UpdateCommand { TableName = referDefine.Table };
                            beforeUpdate.ConditionPairs.Add(new ColumnValuePair
                            {
                                Column = onetoOne.Column,
                                Value = primaryKey
                            });
                            beforeUpdate.UpdatePairs.Add(new ColumnValuePair
                            {
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

                    else if (element.RelationType == RelationType.OneToMany)
                    {
                        var onetoMany = element as OnetoMany;
                        if (onetoMany.IsNeedUpdate)
                        {
                            DeleteAllOneToMany(onetoMany, primaryKey);
                            if (value != null)
                                InsertAllOneToMany(onetoMany, (IEnumerable)value, primaryKey);
                        }

                        property.OperatersList.Clear();
                    }

                    #endregion

                    #region 多对多 集合重新赋值

                    else
                    {
                        var manytoMany = element as ManytoMany;
                        if (manytoMany.IsNeedUpdate)
                        {
                            DeleteAllManyToMany(manytoMany, primaryKey);
                            if (value != null)
                                InsertAllManyToMany(manytoMany, (IEnumerable)value, primaryKey);
                        }

                        property.OperatersList.Clear();
                    }

                    #endregion
                }
                else
                {
                    if (property.OperatersList.Count != 0)
                    {
                        var value = property.PropertyElement.PropertyInfo.GetValue(model);
                        var element = property.PropertyElement;
                        //有operate说明val不为null

                        #region 多对多

                        if (element.RelationType == RelationType.ManyToMany)
                        {
                            var manytoMany = element as ManytoMany;
                            if (manytoMany.IsNeedUpdate)
                            {
                                //检查是否有reset标志
                                if (property.OperatersList.Any(eventArgse =>
                                        eventArgse.Action == NotifyCollectionChangedAction.Reset))
                                {
                                    DeleteAllManyToMany(manytoMany, primaryKey);
                                    InsertAllManyToMany(manytoMany, (IEnumerable)value, primaryKey);
                                }
                                else
                                {
                                    //   var referclassDefine = ((ManytoMany) element).ReferClass;
                                    //根据event做原子操作
                                    foreach (var argse in property.OperatersList)
                                    {
                                        switch (argse.Action)
                                        {
                                            case NotifyCollectionChangedAction.Add:
                                                InsertAllManyToMany(manytoMany, argse.NewItems, primaryKey);
                                                break;
                                            case NotifyCollectionChangedAction.Remove:
                                                foreach (var oldItem in argse.OldItems)
                                                {
                                                    var id = ((IOrmModel)oldItem).OrmInterceptor.ID;
                                                    if (id.IsEmpty())
                                                        continue;
                                                    //舊對象不存在不需要更新
                                                    var deleteCommand =
                                                        new DeleteCommand { TableName = manytoMany.Table };
                                                    deleteCommand.ConditionPairs.Add(new ColumnValuePair
                                                    {
                                                        Column = manytoMany.Column,
                                                        Value = primaryKey
                                                    });
                                                    deleteCommand.ConditionPairs.Add(new ColumnValuePair
                                                    {
                                                        Column = manytoMany.ReferColumn,
                                                        Value = id
                                                    });
                                                    ExecuteSql(deleteCommand);
                                                }

                                                break;
                                            case NotifyCollectionChangedAction.Replace:
                                                foreach (var oldItem in argse.OldItems)
                                                {
                                                    var id = ((IOrmModel)oldItem).OrmInterceptor.ID;
                                                    if (id.IsEmpty())
                                                        continue;
                                                    var deleteCommand =
                                                        new DeleteCommand { TableName = manytoMany.Table };
                                                    deleteCommand.ConditionPairs.Add(new ColumnValuePair
                                                    {
                                                        Column = manytoMany.Column,
                                                        Value = primaryKey
                                                    });
                                                    deleteCommand.ConditionPairs.Add(new ColumnValuePair
                                                    {
                                                        Column = manytoMany.ReferColumn,
                                                        Value = id.IsEmpty()
                                                    });
                                                    ExecuteSql(deleteCommand);
                                                }

                                                InsertAllManyToMany(manytoMany, argse.NewItems, primaryKey);
                                                break;
                                        }
                                    }
                                }
                            }
                        }

                        #endregion

                        #region 一对多

                        else
                        {
                            var onetoMany = element as OnetoMany;
                            if (onetoMany.IsNeedUpdate)
                            {
                                //检查reset
                                if (property.OperatersList.Any(eventArgse =>
                                        eventArgse.Action == NotifyCollectionChangedAction.Reset))
                                {
                                    DeleteAllOneToMany(onetoMany, primaryKey);
                                    InsertAllOneToMany(onetoMany, (IEnumerable)value, primaryKey);
                                }
                                else
                                {
                                    foreach (var argse in property.OperatersList)
                                    {
                                        switch (argse.Action)
                                        {
                                            case NotifyCollectionChangedAction.Add:
                                                InsertAllOneToMany(onetoMany, argse.NewItems, primaryKey);
                                                break;
                                            case NotifyCollectionChangedAction.Remove:
                                                foreach (var oldItem in argse.OldItems)
                                                {
                                                    var updateCommand = new UpdateCommand
                                                    {
                                                        TableName = onetoMany.ReferClass.Table,
                                                    };
                                                    updateCommand.UpdatePairs.Add(new ColumnValuePair
                                                    {
                                                        Column = onetoMany.ReferColumn,
                                                        Value = DBNull.Value
                                                    });
                                                    updateCommand.ConditionPairs.Add(new ColumnValuePair
                                                    {
                                                        Column = onetoMany.ReferColumn,
                                                        Value = ((IOrmModel)oldItem).OrmInterceptor.ID
                                                    });
                                                    ExecuteSql(updateCommand);
                                                }

                                                break;
                                            case NotifyCollectionChangedAction.Replace:
                                                InsertAllOneToMany(onetoMany, argse.NewItems, primaryKey);
                                                foreach (var oldItem in argse.OldItems)
                                                {
                                                    var updateCommand = new UpdateCommand
                                                    {
                                                        TableName = onetoMany.ReferClass.Table,
                                                    };
                                                    updateCommand.UpdatePairs.Add(new ColumnValuePair
                                                    {
                                                        Column = onetoMany.ReferColumn,
                                                        Value = DBNull.Value
                                                    });
                                                    updateCommand.ConditionPairs.Add(new ColumnValuePair
                                                    {
                                                        Column = onetoMany.ReferColumn,
                                                        Value = ((IOrmModel)oldItem).OrmInterceptor.ID
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

            if (command.UpdatePairs.Count > 0)
            {
                command.ConditionPairs.Add(new ColumnValuePair
                {
                    Column = IdColumnName,
                    Value = primaryKey
                });
                ExecuteSql(command);
            }
        }

        private void InsertOneToOne(OnetoOne oneToOne, Guid pk, object oc)
        {
            var iOrmModel = oc as IOrmModel;
            if (iOrmModel.OrmInterceptor.ID.IsEmpty())
                Insert(iOrmModel);
            var updateCommand = new UpdateCommand { TableName = oneToOne.ReferClass.Table };
            updateCommand.ConditionPairs.Add(new ColumnValuePair
            {
                Column = IdColumnName,
                Value = iOrmModel.OrmInterceptor.ID
            });
            updateCommand.UpdatePairs.Add(new ColumnValuePair
            {
                Column = oneToOne.Column,
                Value = pk
            });
            ExecuteSql(updateCommand);
        }

        private void DeleteAllOneToMany(OnetoMany onetoMany, Guid pk)
        {
            var updateCommand = new UpdateCommand { TableName = onetoMany.ReferClass.Table, };
            updateCommand.UpdatePairs.Add(new ColumnValuePair
            {
                Column = onetoMany.ReferColumn,
                Value = DBNull.Value
            });
            updateCommand.ConditionPairs.Add(new ColumnValuePair
            {
                Column = onetoMany.ReferColumn,
                Value = pk
            });
            ExecuteSql(updateCommand);
        }

        private void InsertAllOneToMany(OnetoMany onetoMany, IEnumerable preCollection, Guid pk)
        {
            foreach (var item in preCollection)
            {
                var iOrmModel = item as IOrmModel;
                var updateCommand = new UpdateCommand { TableName = onetoMany.ReferClass.Table };
                if (iOrmModel.OrmInterceptor.ID.IsEmpty())
                    Insert(iOrmModel);
                updateCommand.ConditionPairs.Add(new ColumnValuePair
                {
                    Column = IdColumnName,
                    Value = iOrmModel.OrmInterceptor.ID
                });
                updateCommand.UpdatePairs.Add(new ColumnValuePair
                {
                    Column = onetoMany.ReferColumn,
                    Value = pk
                });
                ExecuteSql(updateCommand);
            }
        }

        private void DeleteAllManyToMany(ManytoMany manytoMany, Guid pk)
        {
            var deleteCommand = new DeleteCommand { TableName = manytoMany.Table };
            deleteCommand.ConditionPairs.Add(new ColumnValuePair
            {
                Column = manytoMany.Column,
                Value = pk
            });
            ExecuteSql(deleteCommand);
        }

        private void InsertAllManyToMany(ManytoMany manytoMany, IEnumerable collection, Guid pk)
        {
            foreach (var item in collection)
            {
                var ormid = ((IOrmModel)item).OrmInterceptor.ID;
                if (ormid.IsEmpty())
                    Insert(item as IOrmModel);
                var insertCommand = new InsertCommand { TableName = manytoMany.Table, };
                insertCommand.InsertPairs.Add(new ColumnValuePair
                {
                    Column = manytoMany.Column,
                    Value = pk
                });
                insertCommand.InsertPairs.Add(new ColumnValuePair
                {
                    Column = manytoMany.ReferColumn,
                    Value = ormid
                });
                ExecuteSql(insertCommand);
            }
        }

        public virtual void Delete(IOrmModel model)
        {
            var classDefine = model.ClassDefine;
            var ormid = model.OrmInterceptor.ID;
            var objects = ObjectCache[classDefine.ClassType];
            if (objects.TryGetValue(ormid, out object value) && value == model)
            {
                var deleteCommand = new DeleteCommand { TableName = classDefine.Table };
                deleteCommand.ConditionPairs.Add(new ColumnValuePair
                {
                    Column = IdColumnName,
                    Value = ormid
                });
                ExecuteSql(deleteCommand);
                objects.Remove(ormid);
            }
        }

        public virtual void Delete(Guid id, Type type)
        {
            var objects = ObjectCache[type];
            if (objects.TryGetValue(id, out object value))
            {
                var classDefine = ClassDefines[type];
                var command = new DeleteCommand() { TableName = classDefine.Table };
                command.ConditionPairs.Add(new ColumnValuePair()
                {
                    Column = IdColumnName,
                    Value = id
                });
                objects.Remove(id);
            }
        }

        public virtual T[] Select<T>(string sql)
        {
            var dataTable = Read(sql);
            return dataTable.Rows.Count == 0
                ? null
                : dataTable.Rows.Cast<DataRow>().Select(dr => (T)DeSerialize(dr, typeof(T))).ToArray();
        }

        public virtual void ClearTable(Type type)
        {
            var classModel = ClassDefines[type];
            var command = new DeleteCommand { TableName = classModel.Table };
            ExecuteSql(command);
        }

        public virtual T[] LoadAll<T>()
        {
            var dt = Read(new SelectCommand { TableName = ClassDefines[typeof(T)].Table });
            return dt.Rows.Cast<DataRow>().Select(x => (T)DeSerialize(x, typeof(T))).ToArray();
        }

        public virtual T[] GetByProperty<T>(string[] attributes, object[] parameters)
        {
            var dc = ClassDefines[typeof(T)];
            var sc = new SelectCommand { TableName = dc.Table };
            for (var i = 0; i < attributes.Length; i++)
            {
                sc.ConditionPairs.Add(new ColumnValuePair
                {
                    Column = attributes[i],
                    Value = parameters[i]
                });
            }

            var dt = Read(sc);
            return (from DataRow x in dt.Rows select DeSerialize(x, typeof(T)) into obj select (T)obj).ToArray();
        }

        public virtual void Delete(IOrmModel model, string property)
        {
            var deleteCommand = new DeleteCommand();
            var select = model.ClassDefine.AllProperties.Keys.FirstOrDefault(key => key == property);
            if (select == null)
            {
                throw new ArgumentException("要删除的属性不存在");
            }

            var propertyElement = model.ClassDefine.AllProperties[select];
            switch (propertyElement.RelationType)
            {
                case RelationType.OneToMany:
                    var oneToMany = propertyElement as OnetoMany;
                    deleteCommand.TableName = oneToMany.ReferClass.Table;
                    deleteCommand.ConditionPairs.Add(new ColumnValuePair
                    {
                        Column = oneToMany.ReferColumn,
                        Value = model.OrmInterceptor.ID
                    });
                    break;
                case RelationType.ManyToMany:
                    var manyToMany = propertyElement as ManytoMany;
                    deleteCommand.TableName = manyToMany.Table;
                    deleteCommand.ConditionPairs.Add(new ColumnValuePair
                    {
                        Column = manyToMany.Column,
                        Value = model.OrmInterceptor.ID
                    });
                    break;
                default:
                    return;
                    throw new ArgumentException("此方法仅用于集合关系删除");
            }

            ExecuteSql(deleteCommand);
        }

        public virtual void Close()
        {
        }
    }
}