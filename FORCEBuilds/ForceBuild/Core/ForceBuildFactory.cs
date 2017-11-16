using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Castle.Core.Internal;
using Castle.DynamicProxy;
using Castle.MicroKernel.ModelBuilder.Descriptors;
using FORCEBuild.Crosscutting;
using Xunit;

namespace FORCEBuild.Core
{
    /* 代理工厂，依据不同的用途可以附加不同的拦截器*/

    /// <summary>
    /// 通用工厂，可以直接使用也可以附加到rpc、orm、messagebus等功能类
    /// </summary>
    public class ForceBuildFactory
    {
        internal event Action<GenerateEventArgs> AgentPreparation;

        private readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

        private ForceBuildFactory() { }

        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public T Get<T>(params object[] paramObjects)
        {
            var type = typeof(T);
            return (T)Get(type, paramObjects);
        }

        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public object Get(Type type, params object[] paramObjects)
        {
            var args = OnAgentPreparation(type);
            if (paramObjects.Length == 0)
            {   
                return _proxyGenerator.CreateClassProxy(args.ToProxyType,args.AdditionalInterfacesToProxy.ToArray(),
                    args.GenerationOptions,args.Interceptors.ToArray());
            }
            return _proxyGenerator.CreateClassProxy(args.ToProxyType, args.AdditionalInterfacesToProxy.ToArray(),
                args.GenerationOptions, paramObjects, args.Interceptors.ToArray());
        }

        public T GetInterface<T>()
        {
            return (T) ProxyInterface(typeof(T));
        }

        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        internal object ProxyInterface(Type type)
        {
            var args = OnAgentPreparation(type);
            return _proxyGenerator.CreateInterfaceProxyWithoutTarget(type, args.AdditionalInterfacesToProxy.ToArray(),
                args.GenerationOptions, args.Interceptors.ToArray());
        }

        /// <summary>
        /// 内部方法,添加拦截器
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="interceptors"></param>
        internal static void AddProxyInterceptor(object obj, params IInterceptor[] interceptors)
        {
            var field = obj.GetType().GetField("__interceptors");
            var listInterceptors = ((IInterceptor[]) field.GetValue(obj)).ToList();
            listInterceptors.AddRange(interceptors);
            field.SetValue(obj, listInterceptors.ToArray());
        }

        /// <summary>
        /// 添加拦截组件参数
        /// </summary>
        /// <returns></returns>
        public ForceBuildFactory Use(FactoryComponent component)
        {
            this.AgentPreparation += component.GeneratePreparation;
            return this;    
        }

        public ForceBuildFactory Retire(FactoryComponent component)
        {
            this.AgentPreparation -= component.GeneratePreparation;
            return this;
        }

        public ForceBuildFactory Add(IInterceptor interceptor)
        {
            this.AgentPreparation += args => {
                args.Interceptors.Add(interceptor);
            };
            return this;
        }

        public ForceBuildFactory Append(Action<GenerateEventArgs> action)
        {
            this.AgentPreparation += action;
            return this;
        }

        public static T GetInterceptor<T>(object obj) where T : IInterceptor
        {
            var field = obj.GetType().GetField("__interceptors");
            var interceptors = field.GetValue(obj) as IInterceptor[];
            var first = interceptors.Find(interceptor => interceptor is T);
            return (T) first;
        }

    //    private static readonly IInterceptorSelector Selector = new CoreInterceptorSelector();

        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        protected GenerateEventArgs OnAgentPreparation(Type type)
        {
            var generateEventArgs = new GenerateEventArgs() {
                ToProxyType = type,
                AdditionalInterfacesToProxy = new List<Type>(),
                GenerationOptions = new ProxyGenerationOptions(),
                Interceptors = new List<IInterceptor>()
            };
            AgentPreparation?.Invoke(generateEventArgs);
            return generateEventArgs;
        }

        public static ForceBuildFactory GetFactory()
        {
            return new ForceBuildFactory();
        }

        private static ForceBuildFactory _notifyFactory;

        public static ForceBuildFactory NotifyFactory => _notifyFactory ?? (_notifyFactory = GetFactory()
                                                             .Use(new PropertyChangedNotifyComponent()));

        public static T GetNotify<T>(params object[] paramObjects)
        {
            return NotifyFactory.Get<T>(paramObjects);
        }
    }
    
}
