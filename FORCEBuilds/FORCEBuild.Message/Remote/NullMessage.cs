using System;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.Remote
{
    /// <summary>
    /// ø’œ˚œ¢
    /// </summary>
    [Serializable]
    public class NullMessage:IMessage   
    {
        public NetErrorCode ErrorCode { get; set; }
    }
}