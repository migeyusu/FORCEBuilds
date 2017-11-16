using System;
using Castle.DynamicProxy;

namespace FORCEBuild.DistributedService
{
    public class RemoteMethodInterceptor : IInterceptor
    {
        public Func<InterfaceCallRequest, object> RemoteProceed { get; set; }

        public Type InterfaceType { get; set; }

        public IExceptionCatch ExceptionCatch { get; set; }

        public void Intercept(IInvocation invocation)
        {
            var request = new InterfaceCallRequest
            {
                InterfaceType = InterfaceType.FullName,
                Method = invocation.Method,
                Parameters = invocation.Arguments
            };
            try {
                invocation.ReturnValue = RemoteProceed?.Invoke(request);
            }
            catch (Exception e) {
                if (ExceptionCatch != null) {
                    if (ExceptionCatch.Catch(e, invocation.Proxy, invocation.Method))
                        throw;
                }
                else
                    throw;
            }
        }
    }
}