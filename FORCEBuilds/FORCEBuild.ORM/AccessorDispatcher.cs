using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Crosscutting;
using FORCEBuild.Crosscutting.Log;

namespace FORCEBuild.ORM
{
    /* 2017.2:
    * 以INotify接口为核心，更新交由更新线程自动化完成
    * （由于只负责读取值同步到数据库，不需要额外的锁）
    * 2017.3
    * 区分读写方式：更改操作会发射到一个队列异步完成，或在接下来的读请求执行之前完成
    * 对象的更新记录会在它的notifyproperties里标记，同时发射到调度器，
    * 对同一个对象的多次变化，调度器只应该标记到最后一次变化
    * 为了尽可能提高性能，使用actor模型；
    * 依据.net的不同版本，对消息并发的资源竞争方式：
    * 2.0-4.0： 原子标记/自旋锁/系统锁
    * 4.5+：并发集合/直接消费者生产者模型
    * 调度器使用了actor模型：对象拥有一个线程安全的邮箱，接受外部消息的投递，
    * 然后对象内部不断地从邮箱取消息执行
    */
    /// <summary>
    /// 调度器
    /// </summary>
    [Serializable]  
    internal class AccessorDispatcher
    {
        private volatile bool _working;
        //刷新间隔
        //private readonly int _interval = 3000; 

        internal Accessor Accessor { get; set; }

        public ILog SqlLog {
            get { return _sqlLog; }
            set {
                _sqlLog = value;
                Accessor.Log = value;
            }
        }

        private readonly BlockingCollection<OrmTask> _tasksQueue;
        private ILog _sqlLog;

        public AccessorDispatcher()
        {
            _tasksQueue = new BlockingCollection<OrmTask>();
        }

        public void Send(OrmTask task)
        {
            _tasksQueue.Add(task);
        }

        public void Update(IOrmModel model)
        {
            var taskmaster = new OrmTask {
                TaskType = OrmTaskType.Update,
                MethodDelegate = () => Accessor.Update(model)
            };
            _tasksQueue.Add(taskmaster);
        }

        public void Start() {
            if (_working)
                return;
            Task.Run(() => {
                _working = true;
                foreach (var ormTask in _tasksQueue.GetConsumingEnumerable()) {
                    try {
                        ormTask.MethodDelegate();
                    }
                    catch (Exception e) {
                        _sqlLog.Write(e);
                    }

                    finally {
                        if (ormTask.TaskType == OrmTaskType.Read)
                            ormTask.AutoResetEvent.Set();
                    }
                }
                _working = false;
                Accessor.Close();
            });
        }

        public void End()
        {
            _tasksQueue.CompleteAdding();
        }
    }
}
