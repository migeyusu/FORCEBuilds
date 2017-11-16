using System;
using System.Reflection;

namespace FORCEBuild.RPC1._0
{
    public interface IRPCIntercept
    {
        /// <summary>
        /// 传送方法参数
        /// </summary>
        event Func<Guid, MethodInfo, object[], object> RemoteProceed;
        /// <summary>
        /// 远程对象同步
        /// </summary>
        Guid  SyncGuid{ get; set; }
    }
}
