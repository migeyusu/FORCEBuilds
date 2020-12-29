using System.Collections.Concurrent;
using FORCEBuild.Net.Base;
using FORCEBuild.Net.Remote.DataTransferObject;

namespace FORCEBuild.Net.Remote
{
    /// <summary>
    /// 消息总线:获取和发布消息处理过程
    /// </summary>
    public class ConsumerProducePipe: MessagePipe<IMessage,IMessage>
    {
        /// <summary>
        /// 通道、消息邮箱：发布者负责创建通道和消息类型，订阅者也可以订阅未出现的通道和消息类型
        /// </summary>
        public ConcurrentDictionary<string,IMessageMail<IMessage>> MessageMails { get; set; }

        protected override IMessage InternalProcess(IMessage x)
        {
            var request = x as MessageTransferRequest;
            if (request != null) {
                var response = new MessageTransferResponse();
                if (MessageMails.TryGetValue(request.MailName, out IMessageMail<IMessage> mail)) {
                    response.Messages = mail.Pull(request);
                }
                return response;
            }
            return x;
        }
    }


}