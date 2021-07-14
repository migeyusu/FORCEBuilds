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
            this._taskFactory = new TaskFactory(scheduler);
        }

        protected abstract Task ReceiveMessage(T message, CancellationToken token);

        public Task PostAsync(T message, CancellationToken token = default)
        {
            return _taskFactory.StartNew(() => { return ReceiveMessage(message, token); }, token);
        }

        /// <summary>
        /// synchronous
        /// </summary>
        public void Post(T message)
        {
            new Task(() => { ReceiveMessage(message, CancellationToken.None); }).RunSynchronously(_taskScheduler);
        }
    }
}