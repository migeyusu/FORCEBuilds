using System;

namespace FORCEBuild.Net.RPC
{
    /// <summary>
    /// 标记拥有远程方法的类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RemoteClassAttribute : Attribute { }
}
