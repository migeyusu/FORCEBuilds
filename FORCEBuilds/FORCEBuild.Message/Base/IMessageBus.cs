namespace FORCEBuild.Message.Base
{
    /* 消息总线使用单独接口，.net中也有相应的接口实现Observer pattern（IObservable）
     * 但较为简单（没有取消订阅，非中心化）
     * 
     */

    /// <summary>
    /// 观察者模型
    /// </summary>
    public interface IMessageBus
    {
        void Subscribe<T>(IMessageSubscriber<T> subscriber) where T : IMessage;
        void UnSubscribe<T>(IMessageSubscriber<T> subscriber) where T : IMessage;
        void Publish<T>(T t) where T : IMessage;
    }
}       