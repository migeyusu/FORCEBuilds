using System;

namespace FORCEBuild.RPC1._0
{
    /// <summary>
    /// 仅用于标记远程类的方法
    /// </summary>
    public class RemoteMethodAttribute:Attribute
    {
        public bool IsAsync { get; set; }

        public RemoteMethodAttribute(bool isAsync=false)
        {
            IsAsync = isAsync;
        }
    }
}
