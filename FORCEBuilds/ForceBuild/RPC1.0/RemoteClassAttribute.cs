using System;

namespace FORCEBuild.RPC1._0
{
    /// <summary>
    /// 标记拥有远程方法的类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RemoteClassAttribute : Attribute { }
}
