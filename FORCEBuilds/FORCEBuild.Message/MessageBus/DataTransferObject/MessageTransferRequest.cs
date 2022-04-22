using System;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.MessageBus.DataTransferObject
{
    /// <summary>
    /// 消息传输请求
    /// </summary>
    [Serializable]
    public class MessageTransferRequest : IMessage, IMessageTransferRequest
    {

        /// <summary>
        /// 请求的消息起始 MailBufferType.IndexQueue时起效
        /// </summary>
        //public int Index { get; set; }
        /// <summary>
        /// 请求的消息邮箱
        /// </summary>
        public string MailName { get; set; }
    }


}