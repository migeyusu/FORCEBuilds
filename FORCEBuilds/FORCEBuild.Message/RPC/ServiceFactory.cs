﻿#define factorymethod
using System;
using System.Reflection;
using Castle.DynamicProxy;
using FORCEBuild.Net.Base;
using FORCEBuild.Net.RPC.Interface;

namespace FORCEBuild.Net.RPC
{

    /* 2017.5 不使用修改后的castle，从而取消领域对象序列化传输，因为一方面不兼容wcf，
     * 另一方面，日益膨胀的domain对象加重了网络负担，而改用dto */

    public class ServiceFactory:IServiceFactory
    {
        private MessagePipe<CallRequest, CallRequest> _beforeRequest;

        private MessagePipe<CallResponse, CallResponse> _afterResponse;

        public bool CanCreate => _messageRequester == null || !_messageRequester.CanRequest;

        private readonly IMessageRequester _messageRequester;

        public IException ExceptionCatcher { get; set; }
        
        private readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();
        
        public ServiceFactory(IMessageRequester requester)
        {
            ExceptionCatcher = new ThrowException();
            _messageRequester = requester;
        }

        /// <summary>
        /// 创建接口对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
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
                    new CallInterceptor(Intercept_RemoteProceed, type) {ExceptionCatcher = this.ExceptionCatcher});

#if factorymethod

            var dynamicType = serviceInvoker.GetType();
            var field = dynamicType.GetField("__target",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            field.SetValue(serviceInvoker, new Object());

#endif
            return (T) serviceInvoker;
        }

        /// <summary>
        ///  对执行响应处理
        /// </summary> 
        public void AddProduceAfterReceive(MessagePipe<CallResponse,CallResponse> pipe)
        {
            _afterResponse = pipe;
        }

        public void AddProduceBeforeRequest(MessagePipe<CallRequest,CallRequest> pipe)
        {
            _beforeRequest = pipe;
        }

        /// <summary>
        /// 调用请求使用同一个终结点;好处是远程服务切换后，已创建的接口对象可以继续使用
        /// </summary>
        /// <param name="request">请求</param>
        /// <returns></returns>
        private object Intercept_RemoteProceed(CallRequest request)
        {
            try {
                if (_beforeRequest != null) {
                    request = _beforeRequest.Process(request);
                }
                var response = _messageRequester.GetResponse(request) as CallResponse;
                if (_afterResponse != null)
                    response = _afterResponse.Process(response);
                if (!response.IsProcessSucceed)
                    throw (Exception)response.Transfer;
                return response.Transfer;
            }
            catch (Exception e) {
                if (ExceptionCatcher == null) throw;
                ExceptionCatcher.Catch(e, null, request.Method);
                return null;
            }
        }

        /// <summary>
        /// 异常捕获处理，默认rethrow
        /// </summary>
        public interface IException
        {
            bool Catch(Exception exception, object source, MethodInfo method);
        }

        public class ThrowException : IException
        {
            public bool Catch(Exception exception, object source, MethodInfo method)
            {
                throw exception;
            }
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
