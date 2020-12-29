using System;
using System.Collections.ObjectModel;
using FORCEBuild.Core;

namespace FORCEBuild.Persistence
{
    /*1 accessor use actor module but not support message command,
      so it can wrapper invoke as command send to dispatcher.
      2. isolate db from another,each field hold a dispatcher
      3.validate external operation
    */

    /// <summary>
    /// Unit Of Work
    /// </summary>
    public class Field:IDisposable
    {
        private readonly Type _parentType = typeof(IOrmModel);

        /// <summary>
        /// 待处理的模型
        /// </summary>
        private Collection<IOrmModel> PendingModels { get; set; }

        internal Accessor Accessor { get; set; }

        /* 取得和插入成功后对象都将被放入集合中,key为id
        三种策略：数据库自动生成、程序生成guid、从数据库获取的键表再累加
        插入前判定是否新对象使用containsvalue,删除成功后从dictionary删除
        更新基于containskey，取得对象基于key */

        internal AccessorDispatcher Dispatcher { get; set; }

        internal Field()
        {
            PendingModels = new Collection<IOrmModel>();
        }

        /// <summary>
        /// 使用工厂
        /// </summary>
        /// <param name="forceBuildFactory"></param>
        public void AttachFactory(ProxyFactory forceBuildFactory)
        {
            forceBuildFactory.UseComponent(Accessor);
            this.Accessor.ProxyFactory = forceBuildFactory;
        }

        public void Insert(object model)
        {
            this.RegisterUpdate(model,ModelStatus.New);
        }

        public void Update(object model)
        {
            this.RegisterUpdate(model,ModelStatus.Modify);
        }

        public void Delete(object model)
        {
            this.RegisterUpdate(model,ModelStatus.Delete);
        }

        public T[] Select<T>(string sql)
        {
            if (!Accessor.ObjectCache.ContainsKey(typeof(T)))
                throw new ArgumentException("未定义的类");
            return Accessor.Select<T>(sql);
        }

        public T[] Get<T>(string[] attributes, object[] parameters)
        {
            if (!Accessor.ObjectCache.ContainsKey(typeof(T)))
                throw new ArgumentException("未定义的类");
            return Accessor.GetByProperty<T>(attributes, parameters);
        }

        //public void Delete(object model, string property)
        //{
        //    var type = model.GetType();
        //    if (!Accessor.ObjectCache.ContainsKey(type))
        //        throw new ArgumentException("未定义的类");
        //    Accessor.Delete((IOrmModel)model, property);
        //}

        public T[] LoadAll<T>()
        {
            if (!Accessor.ObjectCache.ContainsKey(typeof(T)))
                throw new ArgumentException("未定义的类");
            return Accessor.LoadAll<T>();
        }

        //public void ClearTable(Type type)
        //{
        //    if (!Accessor.ObjectCache.ContainsKey(type))
        //        throw new ArgumentException("未定义的类");
        //    var task = new OrmTask {
        //        TaskType = OrmTaskType.Delete,
        //        MethodDelegate = () =>
        //        {
        //            Accessor.ClearTable(type);
        //        }
        //    };
        //    Dispatcher.EnqueueTask(task);
        //}
        
        internal void RegisterUpdate(object model,ModelStatus status)
        {
            if (!(model is IOrmModel)) {
                throw new ArgumentException("未定义的类");
            }
            var ormModel = (IOrmModel)model;
            ormModel.ModelStatus = ModelStatus.Modify;
            PendingModels.Add(ormModel);
        }

        /// <summary>
        /// 在一个事务中提交所有更新
        /// </summary>
        public void Commit()
        {
            using (var transaction = Accessor.BeginTransaction()) {
                try {
                    foreach (var model in PendingModels) {
                        switch (model.ModelStatus) {
                            case ModelStatus.New:
                                Accessor.Insert(model);
                                break;
                            case ModelStatus.Modify:
                                Accessor.Update(model);
                                break;
                            case ModelStatus.Delete:
                                Accessor.Delete(model);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception) {
                    transaction.Rollback();
                    throw;
                }
                finally {
                    Accessor.Close();
                }
            }
        }

        public void Dispose() { }
    }

}
