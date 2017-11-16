using System.ComponentModel;
using Castle.DynamicProxy;
using FORCEBuild.ORM;

namespace FORCEBuild.Core.Old
{
    /* 
     * Observation Mode
     */
    public class Interceptor : IInterceptor,INotifyPropertyChanging,INotifyPropertyChanged,INotifyMethodInvoking,INotifyMethodInvoked
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangingEventHandler PropertyChanging;

        public event MethodInvokedEventHandler MethodInvoked;

        public event MethodInvokingEventHandler MethodInvoking;

        public IOrmCell OrmCell { get; set; }

        public virtual void Intercept(IInvocation invocation)
        {
            var methodName = invocation.Method.Name;
            if (methodName.StartsWith("set_"))
            {
                var propertyName = methodName.Substring(4);
                PropertyChanging?.Invoke(invocation.Proxy,new PropertyChangingEventArgs(propertyName));
                invocation.Proceed();
                PropertyChanged?.Invoke(invocation.Proxy,new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                MethodInvoking?.Invoke(invocation.Proxy,new MethodInvokingEventArgs(methodName));
                invocation.Proceed();
                MethodInvoked?.Invoke(invocation.Proxy,new MethodInvokedEventArgs(methodName));
            }
        }
    }
}
