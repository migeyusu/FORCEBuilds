using System;
using FORCEBuild.Message.Base;

namespace FORCEBuild.Message.RPC
{
    [Serializable]
    public class CallResponse:IMessage
    {
        public bool IsProcessSucceed { get; set; }

        public object Transfer { get; set; } 
    }
}