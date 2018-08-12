using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using FORCEBuild.Crosscutting;
using FORCEBuild.Core;
using FORCEBuild.Crosscutting.Log;
using FORCEBuild.Helper;

namespace FORCEBuild.ORM
{
    /*1 accessor use actor module but not support message command,
      so it can wrapper invoke as command send to dispatcher.
      2. isolate db from another,each field hold a dispatcher
      3.validate external operation
    */

    /// <summary>
    /// 访问入口
    /// </summary>
    public class Field
    {
        private readonly Type _parentType = typeof(IOrmModel);

        //策略上下文
        internal Accessor Accessor { get; set; }
        /* 取得和插入成功后对象都将被放入集合中,key为id
        三种策略：数据库自动生成、程序生成guid、从数据库获取的键表再累加
        插入前判定是否新对象使用containsvalue,删除成功后从dictionary删除
        更新基于containskey，取得对象基于key */

        internal AccessorDispatcher Dispatcher { get; set; }

        public ILog Log
        {
            set => this.Dispatcher.SqlLog = value;
        }

        internal Field() { }

        /// <summary>
        /// 使用工厂
        /// </summary>
        /// <param name="forceBuildFactory"></param>
        public void AttachFactory(ForceBuildFactory forceBuildFactory)
        {
            forceBuildFactory.Append(Accessor.ForceBuildFactory_AgentPreparation);
            this.Accessor.ProxyFactory = forceBuildFactory;
        }

        public void Insert(object model)
        {
            Dispatcher.RegisterUpdate(model,ModelStatus.New);
        }

        public void Update(object model)
        {
            Dispatcher.RegisterUpdate(model,ModelStatus.Modify);
        }

        public void Delete(object model)
        {
            Dispatcher.RegisterUpdate(model,ModelStatus.Delete);
        }


        public T[] Select<T>(string sql)
        {
            if (!Accessor.ObjectCache.ContainsKey(typeof(T)))
                throw new ArgumentException("未定义的类");
            return Accessor.Select<T>(sql);
        }

        public T[] Get<T>(string[] attributies, object[] parameters)
        {
            if (!Accessor.ObjectCache.ContainsKey(typeof(T)))
                throw new ArgumentException("未定义的类");
            return Accessor.GetByProperty<T>(attributies, parameters);
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
    }

}
