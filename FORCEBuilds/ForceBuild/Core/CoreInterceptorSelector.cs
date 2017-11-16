using System;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using FORCEBuild.Crosscutting;
using FORCEBuild.ORM;
using FORCEBuild.RPC2._0;

namespace FORCEBuild.Core
{
    //由于不明原因的失效，已停用
    public class CoreInterceptorSelector:IInterceptorSelector
    {
        public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
        {
            if (method.Name.StartsWith("set_"))
            {
              return interceptors.Where(interceptor => interceptor is OrmInterceptor 
              || interceptor is PropertyNotifyInterceptor|| interceptor is ValidateInterceptor).ToArray();
            }
            return interceptors.Where(interceptor => interceptor is AfterInvokeInterceptor ||
                                                     interceptor is CallInterceptor||interceptor is BeforeInvokeInterceptor)
                .ToArray();
        }
    }
}