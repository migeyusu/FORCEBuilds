using FORCEBuild.Net.Service;

namespace FORCEBuild.Message.Base
{
    /// <summary>
    /// 消息应答器
    /// </summary>
    public interface IMessageReplier
    {
        MessagePipe<IMessage,IMessage> ProducePipe { get; set; }
    }
}