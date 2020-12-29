using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.Local
{
    public class LocalMessageBus:IMessageBus
    {
        private readonly ConcurrentDictionary<Type, IList<IMessageSubscriber<IMessage>>> _messageSubscribers
            = new ConcurrentDictionary<Type, IList<IMessageSubscriber<IMessage>>>();

        public virtual void Subscribe<T>(IMessageSubscriber<T> subscriber) where T : IMessage
        {
            _messageSubscribers.AddOrUpdate(typeof(T),
                type => new List<IMessageSubscriber<IMessage>>() { (IMessageSubscriber<IMessage>)subscriber }
                , (type, list) =>
                {
                    list.Add((IMessageSubscriber<IMessage>)subscriber);
                    return list;
                });
        }

        public virtual void UnSubscribe<T>(IMessageSubscriber<T> subscriber) where T : IMessage
        {
            if (_messageSubscribers.TryGetValue(typeof(T), out IList<IMessageSubscriber<IMessage>> list))
            {
                list.Remove((IMessageSubscriber<IMessage>)subscriber);
            }
        }

        public virtual void Publish<T>(T t) where T : IMessage
        {
            if (_messageSubscribers.TryGetValue(typeof(T), out IList<IMessageSubscriber<IMessage>> subscribers )) {
                foreach (var subscriber in subscribers) {
                    subscriber.OnNext(t);
                }
            }
        }
    }
}