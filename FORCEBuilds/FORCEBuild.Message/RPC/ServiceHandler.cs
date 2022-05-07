﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.Windsor;

namespace FORCEBuild.Net.RPC
{
    /// <summary>
    /// 服务调用处理
    /// </summary>
    public class ServiceHandler
    {
        /// <summary>
        /// 服务容器
        /// </summary>
        private readonly IServiceProvider _container;

        public ServiceHandler(IServiceProvider container)
        {
            this._container = container;
        }

        private readonly MethodInfo _baseCastMethod = typeof(Enumerable).GetMethod("ToArray");

        //todo:支持缓存
        //todo:支持复杂对象的返回
        /// <summary>
        /// 处理调用消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public object Handle(CallRequest request)
        {
            var resolve = _container.GetService(request.InterfaceType);
            if (resolve == null)
            {
                throw new Exception($"服务容器没有注册接口{request.InterfaceType}对应的类");
            }

            var result = request.Method.Invoke(resolve, request.Parameters);
            var returnType = request.Method.ReturnType;
            if (returnType.FullName.StartsWith("System.Collections.Generic.IEnumerable`1"))
            {
                var genericType = returnType.GenericTypeArguments[0];
                var genericMethod = _baseCastMethod.MakeGenericMethod(genericType);
                result = genericMethod.Invoke(null, new[] { result });
            }

            return result;
        }
    }
}