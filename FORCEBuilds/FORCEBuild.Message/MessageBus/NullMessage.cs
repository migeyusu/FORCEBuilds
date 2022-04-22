using System;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.MessageBus
{
    /// <summary>
    /// 空消息
    /// </summary>
    [Serializable]
    public class NullMessage:IMessage   
    {
        public NetErrorCode ErrorCode { get; set; }
    }
}