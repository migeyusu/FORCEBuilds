using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Threading;

namespace FORCEBuild.Concurrency
{
    public class LightThreadPool:IDisposable
    {
        private readonly BlockingCollection<Action> _actions;

        private volatile int _maxThreadCount = 0;

        private volatile int _waitingThreadCount = 0;

        private volatile int _preThreadCount = 0;

        public LightThreadPool(int maxthreadcount = 256)
        {
            _maxThreadCount = maxthreadcount;
            _actions = new BlockingCollection<Action>();
        }

        public void TaskIn(Action action)
        {
            _actions.Add(action);
            if (Interlocked.CompareExchange(ref _preThreadCount, _maxThreadCount, _maxThreadCount) == _maxThreadCount) {
                return;
            }
            if (Interlocked.CompareExchange(ref _waitingThreadCount, 1, 0) == 0) {
                Interlocked.Increment(ref _preThreadCount);
                new Thread((() => {
                        Interlocked.Decrement(ref _waitingThreadCount);
                        foreach (var @action1 in _actions.GetConsumingEnumerable()) {
                            @action1.Invoke();
                            Interlocked.Increment(ref _waitingThreadCount);
                        }
                    }))
                    {IsBackground = true}.Start();
            }
        }
        
        public void Dispose()
        {
            _actions.CompleteAdding();
        }
    }

    //public class LightThreadPool : IDisposable
    //{
    //    private readonly ConcurrentStack<ThreadPoolCell> _threadPoolCells;
    //    private bool _work = true;
    //    private readonly int _max;

    //    public LightThreadPool(int max = 256)
    //    {
    //        _max = max;
    //        _threadPoolCells = new ConcurrentStack<ThreadPoolCell>();
    //        for (var i = 0; i < max; ++i)
    //        {
    //            var cell = new ThreadPoolCell { AutoResetEvent = new AutoResetEvent(false) };
    //            var thread = new Thread(() =>
    //                {
    //                    while (_work)
    //                    {
    //                        cell.AutoResetEvent.WaitOne();
    //                        cell.Action?.Invoke();
    //                        _threadPoolCells.Push(cell);
    //                    }
    //                })
    //                { IsBackground = true };
    //            cell.Thread = thread;
    //            cell.Thread.Start();
    //            _threadPoolCells.Push(cell);
    //        }
    //    }



    //    public void TaskIn(Action action)
    //    {
    //        _threadPoolCells.TryPop(out ThreadPoolCell tpc);
    //        if (tpc == null)//.Equals(default(ThreadPoolCell)))
    //        {
    //            var cell = new ThreadPoolCell
    //            {
    //                AutoResetEvent = new AutoResetEvent(false),
    //                Action = action
    //            };
    //            var thread = new Thread(() =>
    //                {
    //                    while (_work)
    //                    {
    //                        cell.AutoResetEvent.WaitOne();
    //                        cell.Action?.Invoke();
    //                        _threadPoolCells.Push(cell);
    //                    }
    //                })
    //                { IsBackground = true };
    //            cell.Thread = thread;
    //            cell.Thread.Start();
    //            cell.AutoResetEvent.Set();
    //        }
    //        else
    //        {
    //            tpc.Action = action;
    //            tpc.AutoResetEvent.Set();
    //        }
    //    }

    //    public void Close()
    //    {
    //        _work = false;
    //        foreach (var cell in _threadPoolCells)
    //            cell.AutoResetEvent.Set();
    //    }


    //    public void Dispose()
    //    {
    //        Close();
    //    }

    //    private class ThreadPoolCell
    //    {
    //        public AutoResetEvent AutoResetEvent;
    //        public Thread Thread;
    //        public Action Action;
    //    }
    //}
}
