using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FORCEBuild.Concurrency
{
    public class CustomSynchronizationContextTaskScheduler : TaskScheduler
    {
        private readonly SynchronizationContext _mSynchronizationContext;

        public CustomSynchronizationContextTaskScheduler(SynchronizationContext mSynchronizationContext)
        {
            _mSynchronizationContext = mSynchronizationContext;
        }

        protected override void QueueTask(Task task) =>
            this._mSynchronizationContext.Post((state =>
            {
                var taskParameterState = ((Task) state);
                this.TryExecuteTask(taskParameterState);
            }), (object) task);

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) =>
            SynchronizationContext.Current == this._mSynchronizationContext && this.TryExecuteTask(task);

        protected override IEnumerable<Task> GetScheduledTasks() => (IEnumerable<Task>) null;

        public override int MaximumConcurrencyLevel => 1;
    }
}