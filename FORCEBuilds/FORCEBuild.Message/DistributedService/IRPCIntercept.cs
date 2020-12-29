using System;

namespace FORCEBuild.Net.DistributedService
{
    /// <summary>
    /// 2017.5停用
    /// </summary>
    public interface IRPCIntercept
    {
        /// <summary>
        /// 传送方法参数:服务类类型、服务方法
        /// </summary>
        Func<InterfaceCallRequest, object> RemoteProceed { get; set; }

        Type InterfaceType { get; set; }
    }
}
