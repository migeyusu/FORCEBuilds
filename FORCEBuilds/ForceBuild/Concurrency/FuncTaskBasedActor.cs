using System;
using System.Threading;
using System.Threading.Tasks;

namespace FORCEBuild.Concurrency
{
    public class FuncTaskBasedActor: TaskBasedActor<Func<CancellationToken, Task>>
    {
        public FuncTaskBasedActor(TaskScheduler scheduler) : base(scheduler)
        {
            
        }

        protected override Task ReceiveMessage(Func<CancellationToken,Task> message, CancellationToken token)
        {
            return message.Invoke(token);
        }
    }
}