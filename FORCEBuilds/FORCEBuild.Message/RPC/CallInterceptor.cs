using System;
using Castle.DynamicProxy;

namespace FORCEBuild.Net.RPC
{
    public class CallInterceptor : IInterceptor
    {
        private Func<CallRequest, object> RemoteProceed { get; set; }
        private Type InterfaceType { get; set; }

        public CallInterceptor(Func<CallRequest, object> callFunc,
            Type interfaceType)
        {
            RemoteProceed = callFunc;
            InterfaceType = interfaceType;
        }

        public void Intercept(IInvocation invoc)
        {
            var request = new CallRequest {
                InterfaceType = InterfaceType,
                Method = invoc.Method,
                Parameters = invoc.Arguments
            };
            invoc.ReturnValue = RemoteProceed?.Invoke(request);
        }
    }
}
