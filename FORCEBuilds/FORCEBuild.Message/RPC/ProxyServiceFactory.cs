#define factorymethod
using System;
using System.Reflection;
using Castle.DynamicProxy;
using FORCEBuild.Net.Abstraction;
using FORCEBuild.Net.Base;
using FORCEBuild.Net.RPC.Interface;

namespace FORCEBuild.Net.RPC
{
    /// <summary>
    /// 代理服务工厂
    /// </summary>
    public class ProxyServiceFactory : IServiceFactory
    {
        private readonly IMessageClient _messageRequester;

        private readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

        public ProxyServiceFactory(IMessageClient requester)
        {
            _messageRequester = requester ?? throw new ArgumentNullException(nameof(requester));
        }

        /// <summary>
        /// 创建接口对象
        /// </summary>
        /// <typeparam name="T">限定为接口</typeparam>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">RemoteChannel==null.</exception>
        /// <exception cref="ArgumentException">RemoteInterface==null.</exception>
        public T CreateService<T>()
        {
            if (_messageRequester == null)
                throw new NullReferenceException($"{_messageRequester}不能为空！");
            var type = typeof(T);
            var remoteInterface = type.GetCustomAttribute<RemoteInterfaceAttribute>();
            if (remoteInterface == null)
                throw new ArgumentException($"接口{nameof(T)}没有标记为远程接口");
            var serviceInvoker =
                _proxyGenerator.CreateInterfaceProxyWithoutTarget(type,
                    new CallInterceptor(Intercept_RemoteProceed, type));

#if factorymethod

            var dynamicType = serviceInvoker.GetType();
            var field = dynamicType.GetField("__target",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            field.SetValue(serviceInvoker, new Object());

#endif
            return (T)serviceInvoker;
        }

        /// <summary>
        /// 调用请求使用同一个终结点;好处是远程服务切换后，已创建的接口对象可以继续使用
        /// </summary>
        /// <param name="request">请求</param>
        /// <returns></returns>
        private object Intercept_RemoteProceed(CallRequest request)
        {
            if (!(_messageRequester.GetResponse(request) is CallResponse response))
                throw new Exception($"Failed to proxy method {request.Method.Name}!");
            if (!response.IsProcessSucceed)
                throw (Exception)response.Transfer;
            return response.Transfer;
        }
    }
}

/* 如果要作为windsor的factorymethod，直接调用会抛出错误：
           * Can not apply commission concerns to component Late bound because it appears to be a target-less proxy. Currently those are not supported
           * 查到源码产生的原因：
           * protected virtual void ApplyCommissionConcerns(object instance)
             {
              if (Model.Lifecycle.HasCommissionConcerns == false)
              {
                  return;
              }
              instance = ProxyUtil.GetUnproxiedInstance(instance);
              if (instance == null)
              {
                  // see http://issues.castleproject.org/issue/IOC-332 for details
                  throw new NotSupportedException(string.Format("Can not apply commission concerns to component {0} because it appears to be a target-less proxy. Currently those are not supported.", Model.Name));
              }
           ApplyConcerns(Model.Lifecycle.CommissionConcerns, instance);
           return instance;
            }
           ProxyUtil.GetUnproxiedInstance(instance)的代码是：
           public static object GetUnproxiedInstance(object instance)
           {
  #if FEATURE_REMOTING
             if (!RemotingServices.IsTransparentProxy(instance))
  #endif
              { 
              var accessor = instance as IProxyTargetAccessor;
              if (accessor != null)
              {
                  instance = accessor.DynProxyGetTarget();
              }
          }
          改进方式为人为填充__target为new object
           */