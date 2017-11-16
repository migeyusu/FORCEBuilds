using FORCEBuild.Core;
using FORCEBuild.Core.Interceptors;

namespace FORCEBuild.Crosscutting
{
    /// <summary>
    /// 
    /// </summary>
    public class AopGenerator
    {
        public IAfterReturningAdvice AfterReturningAdvice { get; set; }

        public IMethodBeforeAdvice MethodBeforeAdvice { get; set; }

        public void AttachFactory(ForceBuildFactory forceBuildFactory)
        {
            if (this.AfterReturningAdvice!=null)
            {
                forceBuildFactory.AgentPreparation += (arg1, arg2, arg3, arg4) =>
                {
                    arg2.Add(new AfterInvokeInterceptor {ReturningAdvice = AfterReturningAdvice});
                };
            }
            if (this.MethodBeforeAdvice!=null)
            {
                forceBuildFactory.AgentPreparation += (arg1, arg2, arg3, arg4) =>
                {
                    arg2.Add(new BeforeInvokeInterceptor { BeforeAdvice = MethodBeforeAdvice });
                };
            }
        }
    }
}