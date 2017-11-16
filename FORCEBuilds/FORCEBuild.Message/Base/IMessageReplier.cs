namespace FORCEBuild.Message.Base
{
    public interface IMessageReplier
    {
        bool Working { get; }
        void Start();
        void End();
        MessagePipe<IMessage,IMessage> ProducePipe { get; set; }
    }
}