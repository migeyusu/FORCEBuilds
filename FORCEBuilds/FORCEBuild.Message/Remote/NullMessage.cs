using System;
using FORCEBuild.Message.Base;

namespace FORCEBuild.Message.Remote
{
    /// <summary>
    /// ����Ϣ
    /// </summary>
    [Serializable]
    public class NullMessage:IMessage   
    {
        public NetErrorCode ErrorCode { get; set; }
    }
}