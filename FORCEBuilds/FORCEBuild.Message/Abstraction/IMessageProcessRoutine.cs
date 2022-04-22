using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.Abstraction
{
    /// <summary>
    /// 中间件形式的消息处理，可以自定义消息的回路
    /// </summary>
    public interface IMessageProcessRoutine
    {
        /// <summary>
        /// 可配置的消息处理管道
        /// </summary>
        MessagePipe<IMessage,IMessage> ProducePipe { get; set; }
    }
}