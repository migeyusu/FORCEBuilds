using System;
using System.Net;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FORCEBuild.Crosscutting.Log;
using FORCEBuild.Message.Base;
using FORCEBuild.Message.Remote;
using FORCEBuild.Net.Base;
using FORCEBuild.Net.Service;

namespace FORCEBuild.Message.RPC
{

    /// <summary>
    /// 过程调用服务
    /// </summary>
    public class ProcedureCallService
    {
        private readonly Actuator _actuator = new Actuator();

        public ProcedureCallService(IMessageReplier netMessageListener)
        {
            var callProducePipeline = new CallProducePipe {
                Actuator = _actuator
            };
            if (netMessageListener.ProducePipe == null) {
                netMessageListener.ProducePipe = callProducePipeline;
            }
            else {
                netMessageListener.ProducePipe.Append(callProducePipeline);
            }
        }

        /// <summary>
        /// 注册类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Register<T>(T x = null) where T : class
        {
            if (x == null) {
                _actuator.Register<T>();
            }
            else {
                _actuator.Container.Register(Component.For<T>()
                    .Instance(x));
            }
        }
        
        /// <summary>
        /// 注册接口-类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TK"></typeparam>
        /// <param name="singleton"></param>
        public void Register<T, TK>(bool singleton = false) where T : class where TK : T
        {
            _actuator.Register<T,TK>(singleton);
        }

        public void Initialize(IWindsorContainer container)
        {
            _actuator.Container = container;
        }

    }

    

}
