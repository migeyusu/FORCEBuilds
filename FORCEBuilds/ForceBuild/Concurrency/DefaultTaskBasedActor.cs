using System;
using System.Threading.Tasks;

namespace FORCEBuild.Concurrency
{
    public class DefaultTaskBasedActor<T> : TaskBasedActor<T>
    {
        private readonly Action<T> _receiveMessageAction;

        public DefaultTaskBasedActor(TaskScheduler scheduler, Action<T> receiveMessageAction) : base(scheduler)
        {
            this._receiveMessageAction = receiveMessageAction;
        }

        protected override void ReceiveMessage(T message)
        {
            _receiveMessageAction.Invoke(message);
        }
    }
}