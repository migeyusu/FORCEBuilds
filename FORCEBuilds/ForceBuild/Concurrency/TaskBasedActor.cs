using System;
using System.Collections.Concurrent;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace FORCEBuild.Concurrency
{
    /// <summary>
    /// new actor by publish/subscribe
    /// support thread 
    /// </summary>
    public abstract class TaskBasedActor<T>
    {
        private readonly TaskFactory _taskFactory;

        private readonly TaskScheduler _taskScheduler;

        protected TaskBasedActor(TaskScheduler scheduler)
        {
            this._taskScheduler = scheduler;
            _taskFactory = new TaskFactory(scheduler);
        }

        protected abstract void ReceiveMessage(T message);

        public Task PostAsync(T message, CancellationToken token = default)
        {
            return _taskFactory.StartNew(() => { ReceiveMessage(message); }, token);
        }

        /// <summary>
        /// synchronous
        /// </summary>
        public void Post(T message)
        {
            _taskFactory.StartNew(() => { ReceiveMessage(message); }).RunSynchronously(_taskScheduler);
        }
    }
}