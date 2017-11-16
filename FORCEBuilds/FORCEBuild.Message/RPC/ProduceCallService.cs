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
    /// 不为容器内的类对象提供线程安全
    /// </summary>
    public class ProduceCallService:ITcpServiceProvider
    {
        public ILog Log { get; set; }
        /// <summary>
        /// 生命周期为单次服务
        /// </summary>
        public Guid ServiceGuid { get; set; }

        public IPEndPoint ServiceEndPoint {
            get => _netMessageListener.EndPoint;
            set => _netMessageListener.EndPoint = value;
        }

        /// <summary>
        /// 运行状态
        /// </summary>
        public bool Working => _netMessageListener.Working;

        private readonly TcpMessageReplier _netMessageListener;

        private readonly Actuator _actuator;

        public ProduceCallService()
        {
            _actuator = new Actuator();
            var callProducePipeline = new CallProducePipe {
                Actuator = _actuator
            };
            _netMessageListener = new TcpMessageReplier {
                ProducePipe = callProducePipeline,
            };
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

        public void Start()
        {
            //实际运行时再创建endpoint
            Start(NetHelper.InstanceEndPoint);
        }

        public void Start(IPEndPoint endPoint)
        {
            if (Working) {
                return;
            }
            ServiceEndPoint = endPoint;
            ServiceGuid = Guid.NewGuid();
            _netMessageListener.Start();
        }

        public void End()
        {
            _netMessageListener.End();
        }

        public void Dispose()
        {
            this.End();
            _netMessageListener?.Dispose();
        }
    }

    

}
