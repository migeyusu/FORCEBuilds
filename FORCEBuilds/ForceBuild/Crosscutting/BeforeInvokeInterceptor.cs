using Castle.DynamicProxy;

namespace FORCEBuild.Crosscutting
{
    public class BeforeInvokeInterceptor : IInterceptor
    {
        public IMethodBeforeAdvice BeforeAdvice { get; set; }

        public void Intercept(IInvocation invocation)
        {
            var methodName = invocation.Method.Name;
            if (!methodName.StartsWith("set_") && !methodName.StartsWith("get_"))
            {
                BeforeAdvice?.Before(invocation.Method, invocation.Arguments, invocation.MethodInvocationTarget);
            }
            invocation.Proceed();
        }
    }
}