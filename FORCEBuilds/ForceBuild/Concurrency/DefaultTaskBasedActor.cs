using System;
using System.Threading;
using System.Threading.Tasks;

namespace FORCEBuild.Concurrency
{
    public class DefaultTaskBasedActor<T> : TaskBasedActor<T>
    {
        private readonly Action<T, CancellationToken> _receiveMessageAction;

        public DefaultTaskBasedActor(TaskScheduler scheduler, Action<T, CancellationToken> receiveMessageAction) :
            base(scheduler)
        {
            this._receiveMessageAction = receiveMessageAction;
        }

        protected override Task ReceiveMessage(T message, CancellationToken token)
        {
            _receiveMessageAction.Invoke(message, token);
            return Task.CompletedTask;
        }
    }
}