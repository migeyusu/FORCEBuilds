using System;
using Castle.DynamicProxy;
using FORCEBuild.Message.RPC.Interface;

namespace FORCEBuild.Message.RPC
{
    public class CallInterceptor : IInterceptor
    {
        private Func<CallRequest, object> RemoteProceed { get; set; }

        private Type InterfaceType { get; set; }

        public ServiceFactory.IException ExceptionCatcher { get; set; }

        public CallInterceptor(Func<CallRequest, object> callFunc,
            Type interfacType)
        {
            RemoteProceed = callFunc;
            InterfaceType = interfacType;
        }

        public void Intercept(IInvocation invoc)
        {
            var request = new CallRequest {
                InterfaceType = InterfaceType,
                Method = invoc.Method,
                Parameters = invoc.Arguments
            };
            try {
                invoc.ReturnValue = RemoteProceed?.Invoke(request);
            }
            catch (Exception exception) {
                ExceptionCatcher.Catch(exception, invoc.MethodInvocationTarget, invoc.Method);
            }
        }
    }
}
