using Castle.DynamicProxy;

namespace FORCEBuild.Crosscutting
{
    public class PropertyNotifyInterceptor : IInterceptor {
        private readonly NotifyBase _notifyBase;

        public PropertyNotifyInterceptor(NotifyBase notifyBase) {
            _notifyBase = notifyBase;
        }

        public void Intercept(IInvocation invocation) {
            invocation.Proceed();
            if (invocation.Method.Name.StartsWith("set_")) {
                var methodName = invocation.Method.Name.Substring(4);
                _notifyBase.OnPropertyChanged(invocation.Proxy, methodName);
            }
        }
    }


}


