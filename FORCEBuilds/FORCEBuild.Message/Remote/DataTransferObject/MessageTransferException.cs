using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using FORCEBuild.Message.Base;

namespace FORCEBuild.Message.Remote.DataTransferObject {
    /// <summary>
    /// ��ʾ��Ϣ��Ӧ����
    /// </summary>
    [Serializable]
    [Description("��ʾ������Ϣ���յ�����Ϣ�쳣")]
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