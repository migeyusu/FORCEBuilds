using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using FORCEBuild.Crosscutting;
using FORCEBuild.Core;
using FORCEBuild.Crosscutting.Log;

namespace FORCEBuild.ORM
{
    /// <summary>
    /// 访问入口
    /// </summary>
    public class Field
    {
        private readonly Type _parentType = typeof(IOrmModel);

        //策略模式，作为ORM的Context保持对accessor的引用
        internal Accessor Accessor { get; set; }
        /* 取得和插入成功后对象都将被放入集合中,key为id
        三种策略：数据库自动生成、程序生成guid、从数据库获取的键表再累加
        插入前判定是否新对象使用containsvalue,删除成功后从dictionary删除
        更新基于containskey，取得对象基于key */

        internal AccessorDispatcher Scheduler { get; set; }

        public ILog Log
        {
            set => this.Scheduler.SqlLog = value;
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

        public void Start()
        {
            Scheduler.Start();
        }

        public void Insert(object model) //入口函数
        {
            var type = model.GetType();
            if (!_parentType.IsAssignableFrom(type))
                throw new ArgumentException("未定义的类");
            var task = new OrmTask {
                MethodDelegate = new Action(() => Accessor.Insert(model as IOrmModel)),
                TaskType = OrmTaskType.Create
            };
            Scheduler.Enqueue(task);

        }

        public void Update(object oc)
        {
            var type = oc.GetType();
            if (!_parentType.IsAssignableFrom(type))
                throw new ArgumentException("未定义的类");
            var task = new OrmTask {
                MethodDelegate = new Action(() =>
                {
                    Accessor.Update(oc as IOrmModel);
                }),
                TaskType = OrmTaskType.Update
            };
            Scheduler.Enqueue(task);
        }

        public void Delete(object t)
        {
            var type = t.GetType();
            if (!_parentType.IsAssignableFrom(type))
                throw new ArgumentException("未定义的类");
            var task = new OrmTask {
                MethodDelegate = new Action((() =>
                {
                    Accessor.Delete(t as IOrmModel);
                })),
                TaskType = OrmTaskType.Delete
            };
            Scheduler.Enqueue(task);
        }

        public T[] Select<T>(string sql)
        {
            if (!Accessor.Cache.ContainsKey(typeof(T)))
                throw new ArgumentException("未定义的类");
            var task = new OrmTask {
                TaskType = OrmTaskType.Read
            };
            task.MethodDelegate = new Action((() =>
            {
                task.ReturnValue = Accessor.Select<T>(sql);
            }));
            Scheduler.Enqueue(task);
            task.AutoResetEvent.WaitOne();
            return task.ReturnValue;
        }

        public T[] Get<T>(string[] attributies, object[] parameters)
        {
            if (!Accessor.Cache.ContainsKey(typeof(T)))
                throw new ArgumentException("未定义的类");
            var task = new OrmTask {
                TaskType = OrmTaskType.Read,
            };
            task.MethodDelegate = new Action((() =>
            {
                task.ReturnValue = Accessor.GetByProperty<T>(attributies, parameters);
            }));
            Scheduler.Enqueue(task);
            task.AutoResetEvent.WaitOne();
            return task.ReturnValue;

        }

        public void Delete(object model, string property)
        {
            var type = model.GetType();
            if (!Accessor.Cache.ContainsKey(type))
                throw new ArgumentException("未定义的类");
            var task = new OrmTask {
                TaskType = OrmTaskType.Delete,
                MethodDelegate = new Action((() =>
                {
                    Accessor.Delete((IOrmModel) model, property);
                }))
            };
            Scheduler.Enqueue(task);
        }

        public T[] LoadAll<T>()
        {
            if (!Accessor.Cache.ContainsKey(typeof(T)))
                throw new ArgumentException("未定义的类");
            var task = new OrmTask {
                TaskType = OrmTaskType.Read,
            };
            task.MethodDelegate = new Action((() =>
            {
                task.ReturnValue = Accessor.LoadAll<T>();
            }));
            Scheduler.Enqueue(task);
            task.AutoResetEvent.WaitOne();
            return task.ReturnValue;

        }

        public void ClearTable(Type type)
        {
            if (!Accessor.Cache.ContainsKey(type))
                throw new ArgumentException("未定义的类");
            var task = new OrmTask {
                TaskType = OrmTaskType.Delete,
                MethodDelegate = new Action(() =>
                {
                    Accessor.ClearTable(type);
                })
            };
            Scheduler.Enqueue(task);

        }

        public void Close()
        {
            Scheduler.End();
        }
    }

}
