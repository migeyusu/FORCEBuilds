using System;
using System.Threading;
using System.Threading.Tasks;

namespace FORCEBuild.Concurrency
{
    public class ActionTaskBasedActor : TaskBasedActor<Action>
    {
        public ActionTaskBasedActor(TaskScheduler scheduler) : base(scheduler)
        {
        }

        protected override Task ReceiveMessage(Action message, CancellationToken token)
        {
            message.Invoke();
            return Task.CompletedTask;
        }
    }
}