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
    * 分离写读，写操作分配到一个队列延时完成，或在一个读请求来之前立即完成
    * 对于对象的更新记录，会在对象本身的notifyproperties属性里标记，同时推送
    * 到调度器，对同一个对象的多次变化，调度器只应该保持最后一次推送
    * 为了防止可能的锁，使用actor模型，依据.net的不同版本：
    * 2.0-4.0： 原子锁
    * 4.5+：并行集合
    * 调度器使用了actor模型，这里有必要说下：actor模型的原始运行方式是给对象设置一个邮箱
    * 然后对象内部不断地从邮箱取消息—— actor模型的邮箱可以使用线程安全的队列实现，
    * 但更契合消费者-生产者模型
    */
    /// <summary>
    /// 调度器
    /// </summary>
    [Serializable]  
    internal class AccessorDispatcher
    {
        private bool _working;
        //初始间隔
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

        public void Enqueue(OrmTask task)
        {
            _tasksQueue.Add(task);
        }

        public void Enqueue(IOrmModel iOrmModel)
        {
            var ormtask = new OrmTask {
                TaskType = OrmTaskType.Update,
                MethodDelegate = () => Accessor.Update(iOrmModel)
            };
            _tasksQueue.Add(ormtask);
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

        //public void ProduceLoop()
        //{
        //    while (true)
        //    {
        //        //按序执行
        //        lock (_queuelocker)
        //        {
        //            OrmMix cell;

        //            if (_ormTasks.Count>0)
        //            {
        //                cell = _ormTasks[0];
        //                _ormTasks.RemoveAt(0);
        //            }
        //            else
        //            {
        //                return;
        //            }
        //            try
        //            {
        //                Accessor.Update(cell);
        //                cell.IsInTask = false;
        //            }
        //            catch (Exception ex)
        //            {
        //                _streamWriter?.WriteLine(
        //                    $"[{DateTime.Now}] 异常类型：{ex.GetType()}\r\n异常消息：{ex.Message}\r\n异常信息：{ex.StackTrace} \r\n ");
        //            }
        //        }
        //    }
        //}

        public void End()
        {
            _tasksQueue.CompleteAdding();
        }
    }
}
