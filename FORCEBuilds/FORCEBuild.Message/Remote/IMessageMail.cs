using System.Collections.Generic;
using FORCEBuild.Message.Base;
using FORCEBuild.Message.Remote.DataTransferObject;

namespace FORCEBuild.Message.Remote
{
    /// <summary>
    /// 消息邮箱，在服务端作为消息Consumer的代理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMessageMail<in T> where T : IMessage
    {
        MessageRouteStrategy MailStrategy { get; set; }
        /// <summary>
        /// 投递
        /// </summary>
        /// <param name="message"></param>
        void Post(T message);

        bool IsMatch(string topic);

        IEnumerable<IMessage> Pull(IMessageTransferRequest request);
    }
}   