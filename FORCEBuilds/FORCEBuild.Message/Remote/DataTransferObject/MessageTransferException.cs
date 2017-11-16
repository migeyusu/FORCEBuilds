using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using FORCEBuild.Message.Base;

namespace FORCEBuild.Message.Remote.DataTransferObject {
    /// <summary>
    /// 表示消息响应出错
    /// </summary>
    [Serializable]
    [Description("表示接收消息端收到的消息异常")]
    public class MessageTransferException : Exception
    {
        public NetErrorCode NetErrorCode { get; set; }  

        public MessageTransferException() { }
        public MessageTransferException(string message) : base(message) { }
        public MessageTransferException(string message, Exception inner) : base(message, inner) { }

        protected MessageTransferException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}