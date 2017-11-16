namespace FORCEBuild.Message.Base
{
    /// <summary>
    /// 实现该接口即可完成消息的订阅和处理
    /// </summary>
    public interface IMessageSubscriber<T> where T:IMessage
    {
        /// <summary>
        /// 响应消息
        /// </summary>
        /// <param name="x"></param>
        void OnNext(T x);
    }
}