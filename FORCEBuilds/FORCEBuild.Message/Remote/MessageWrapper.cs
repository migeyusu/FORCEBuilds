namespace FORCEBuild.Message.Base
{

    /// <summary>
    /// 消息包装器
    /// </summary>
    public class MessageWrapper:IMessage
    {
        public long Index { get; set; }

        public IMessage Message { get; set; }   
    }
}