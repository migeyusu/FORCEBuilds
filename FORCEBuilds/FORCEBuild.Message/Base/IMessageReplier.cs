namespace FORCEBuild.Net.Base
{
    /// <summary>
    /// 消息应答器
    /// </summary>
    public interface IMessageReplier
    {
        MessagePipe<IMessage,IMessage> ProducePipe { get; set; }
    }
}