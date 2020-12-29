using System;
using System.Collections.Generic;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.Remote.DataTransferObject
{
    /// <summary>
    /// 消息响应，负责运输消息
    /// </summary>
    [Serializable]
    public class MessageTransferResponse:IMessage
    {
        /// <summary>
        /// 下一个消息指针
        /// </summary>
        //public int NextHandle { get; set; }

        public IEnumerable<IMessage> Messages { get; set; } 
    }
}