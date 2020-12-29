using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Castle.DynamicProxy;
using Castle.MicroKernel.ModelBuilder.Descriptors;
using FORCEBuild.Crosscutting;


namespace FORCEBuild.Core
{
    /// <summary>
    /// 代理工厂，可以直接使用也可以附加到rpc、orm、messagebus等模块
    /// </summary>
    public class ProxyFactory
    {
        private event Action<PreProxyEventArgs> _agentPreparationEvent;

        private readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

        private ProxyFactory() { }

        public T CreateProxyClass<T>(params object[] paramObjects)
        {
            var type = typeof(T);
            return (T)CreateProxyClass(type, paramObjects);
        }

        public object CreateProxyClass(Type type, params object[] paramObjects)
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

        public T CreateProxyInterface<T>()
        {
            return (T) CreateProxyInterface(typeof(T));
        }

        public object CreateProxyInterface(Type type)
        {
            var args = OnAgentPreparation(type);
            return _proxyGenerator.CreateInterfaceProxyWithoutTarget(type, args.AdditionalInterfacesToProxy.ToArray(),
                args.GenerationOptions, args.Interceptors.ToArray());
        }

        /// <summary>
        /// 添加拦截器
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="interceptors"></param>
        public static void AddProxyInterceptor(object obj, params IInterceptor[] interceptors)
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
        public ProxyFactory UseComponent(IFactoryProxyPreparation component)
        {
            this._agentPreparationEvent += component.GeneratePreparation;
            return this;    
        }

        public ProxyFactory Retire(IFactoryProxyPreparation component)
        {
            this._agentPreparationEvent -= component.GeneratePreparation;
            return this;
        }

/*        public ForceBuildFactory Add(IInterceptor interceptor)
        {
            this._agentPreparationEvent += args => {
                args.Interceptors.Add(interceptor);
            };
            return this;
        }*/

        public static T GetInterceptor<T>(object obj) where T : IInterceptor
        {
            var field = obj.GetType().GetField("__interceptors");
            var interceptors = field.GetValue(obj) as IInterceptor[];
            var first = interceptors.Find(interceptor => interceptor is T);
            return (T) first;
        }

        protected PreProxyEventArgs OnAgentPreparation(Type type)
        {
            var generateEventArgs = new PreProxyEventArgs() {
                ToProxyType = type,
                AdditionalInterfacesToProxy = new List<Type>(),
                GenerationOptions = new ProxyGenerationOptions(),
                Interceptors = new List<IInterceptor>()
            };
            _agentPreparationEvent?.Invoke(generateEventArgs);
            return generateEventArgs;
        }

        public static ProxyFactory GetFactory()
        {
            return new ProxyFactory();
        }

        private static ProxyFactory _notifyFactory;

        public static ProxyFactory NotifyFactory => _notifyFactory ?? (_notifyFactory = GetFactory()
                                                             .UseComponent(new PropertyChangedNotifyPreparation()));

        public static T GetNotify<T>(params object[] paramObjects)
        {
            return NotifyFactory.CreateProxyClass<T>(paramObjects);
        }
    }
    
}
