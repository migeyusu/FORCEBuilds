using System;
using FORCEBuild.Message.Base;

namespace FORCEBuild.RPC2._0
{
    [Serializable]
    public class CallResponse:IMessage
    {
        public bool IsProcessSucceed { get; set; }

        public object Transfer { get; set; } 
    }
}