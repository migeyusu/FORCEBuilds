using System.Collections.Concurrent;
using System.Collections.Generic;
using FORCEBuild.Net.Base;
using FORCEBuild.Net.Remote.DataTransferObject;

namespace FORCEBuild.Net.Remote.Buffer
{
    /* 邮箱，即消息缓冲区决定了对消息的处理策略，例如是否支持持久化等
     * 邮箱支持多个用户基于指针的读写，指针使用long，long=-1表示最后一个
     */

    /// <summary>
    /// 默认消息邮箱，使用队列储存，支持并发
    /// </summary>
    internal class DefaultMessageMail: IMessageMail<IMessage>
    {
        /// <summary>
        /// 邮箱名
        /// </summary>
        public string Name { get; set; }

        private readonly ConcurrentQueue<IMessage> _messagesQueue = new ConcurrentQueue<IMessage>();
       
        public MessageRouteStrategy MailStrategy { get; set; }

        /// <summary>
        /// 投递消息    
        /// </summary>
        /// <param name="message"></param>
        public void Post(IMessage message)
        {
            _messagesQueue.Enqueue(message);
        }

        public bool IsMatch(string channel)
        {
            return MailStrategy.IsMatch(channel);
        }

        public IEnumerable<IMessage> Pull(IMessageTransferRequest request)
        {
            var list = new List<IMessage>();
            while (_messagesQueue.TryDequeue(out IMessage message)) {
                list.Add(message);
            }
            return list.Count > 0 ? list : null;
        }
    }
}