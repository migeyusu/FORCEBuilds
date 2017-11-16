using Castle.DynamicProxy;

namespace FORCEBuild.Crosscutting
{
    public class AfterInvokeInterceptor : IInterceptor
    {
        public IAfterReturningAdvice ReturningAdvice { get; set; }

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();
            var methodName = invocation.Method.Name;
            if (!methodName.StartsWith("set_") && !methodName.StartsWith("get_"))
            {
                ReturningAdvice?.AfterReturning(invocation.ReturnValue, invocation.Method,
                    invocation.Arguments, invocation.InvocationTarget);
            }
        }

    }

}