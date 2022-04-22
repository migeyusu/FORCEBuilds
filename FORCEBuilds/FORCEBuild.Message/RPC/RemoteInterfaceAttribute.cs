using System;

namespace FORCEBuild.Net.RPC
{
    /// <summary>
    /// 标记该接口可被远程调用
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class RemoteInterfaceAttribute : Attribute
    {
        public RemoteInterfaceAttribute()
        {
        }
    }
}