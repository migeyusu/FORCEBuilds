using System;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.RPC
{
    [Serializable]
    public class CallResponse:IMessage
    {
        public bool IsProcessSucceed { get; set; }

        public object Transfer { get; set; } 
    }
}