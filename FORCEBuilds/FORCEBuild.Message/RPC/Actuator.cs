using System;
using System.Linq;
using System.Reflection;
using Castle.Windsor;

namespace FORCEBuild.Net.RPC
{
    /// <summary>
    /// 基于消息的执行器
    /// </summary>
    public class Actuator:IDisposable
    {
        /// <summary>
        /// 对象容器
        /// </summary>
        public IWindsorContainer Container { get; set; }

        public Actuator()
        {
            Container = new WindsorContainer();
        }

        /// <summary>
        /// 注册类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public virtual void Register<T>() where T : class
        {
            Container.Register(Castle.MicroKernel.Registration.Component.For<T>());
        }

        /// <summary>
        /// 注册接口-类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TK"></typeparam>
        /// <param name="singleton"></param>
        public virtual void Register<T, TK>(bool singleton = false) where T : class where TK : T
        {
            Container.Register(singleton
                ? Castle.MicroKernel.Registration.Component.For<T>()
                    .ImplementedBy<TK>()
                    .LifestyleSingleton()
                : Castle.MicroKernel.Registration.Component.For<T>()
                    .ImplementedBy<TK>());
        }

        private readonly MethodInfo _baseCastMethod = typeof(Enumerable).GetMethod("ToArray");

        public virtual object Execute(CallRequest request)
        {
            var resolve = Container.Resolve(request.InterfaceType);
            if (resolve == null) {
                throw new Exception($"服务容器没有注册接口{request.InterfaceType}对应的类");
            }
            var result = request.Method.Invoke(resolve, request.Parameters);
            var returnType = request.Method.ReturnType;
            if (returnType.FullName.StartsWith("System.Collections.Generic.IEnumerable`1")) {
                var genericType = returnType.GenericTypeArguments[0];
                var genericMethod = _baseCastMethod.MakeGenericMethod(genericType);
                result = genericMethod.Invoke(null, new[] {result});
            }
            return result;
        }

        public void Dispose()
        {
            Container?.Dispose();
        }
    }
}