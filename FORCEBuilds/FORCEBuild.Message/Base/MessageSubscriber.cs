using System;

namespace FORCEBuild.Message.Base
{
    public class MessageSubscriber<T>:IMessageSubscriber<T> where T:IMessage
    {
        private readonly Action<T> _action;

        public MessageSubscriber(Action<T> action)
        {
            this._action = action;
        }

        public virtual void OnNext(T x) => _action?.Invoke(x);

        public static IMessageSubscriber<T> Create<T>(Action<T> action) where T : IMessage
        {
            return new MessageSubscriber<T>(action);
        }
    }
}