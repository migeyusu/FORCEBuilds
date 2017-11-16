using System;

namespace FORCEBuild.RPC1._0
{
    /// <summary>
    /// 标记该接口可被远程调用
    /// </summary>
    public class RemoteInterfaceAttribute:Attribute
    {
        public string Implement { get; set; }

        public RemoteInterfaceAttribute(string implementname = "")
        {
            Implement = implementname;
        }
    }
}
