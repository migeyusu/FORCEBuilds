using System;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace FORCEBuild.RPC1._0
{
    /*
     * 拦截已实例化对象的方法
     * 带同步的guid
     */

    public class RPCInterceptor : IInterceptor, IRPCIntercept
    {
        public event Func<Guid, MethodInfo, object[], object> RemoteProceed;

        public Guid SyncGuid { get; set; }

        public void Intercept(IInvocation invoc)
        {
            if (RemoteProceed == null)
            {
                invoc.Proceed();
                return;
            }
            var remoteMethod = invoc.Method.GetCustomAttribute<RemoteMethodAttribute>();
            if (remoteMethod == null)
            {
                //调用了本地方法
                invoc.Proceed();
            }
            else if (remoteMethod.IsAsync)
            {
                invoc.ReturnValue =
                    Task.Run(() => RemoteProceed?.Invoke(SyncGuid, invoc.Method, invoc.Arguments));
            }
            else
            {
                invoc.ReturnValue = RemoteProceed?.Invoke(SyncGuid, invoc.Method, invoc.Arguments);
            }
        }
    }
}
