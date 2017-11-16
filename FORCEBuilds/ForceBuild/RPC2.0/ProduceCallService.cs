using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Castle.Windsor;
using FORCEBuild.Crosscutting.Log;
using FORCEBuild.Message.Base;
using FORCEBuild.Message.Remote;
using FORCEBuild.Net.Base;
using FORCEBuild.Net.Service;
using FORCEBuild.Persistence.Serialization;
using FORCEBuild.RPC2._0.Interface;

namespace FORCEBuild.RPC2._0
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

        private readonly TcpMessageResponse _netMessageListener;

        private readonly Actuator _actuator;

        public ProduceCallService()
        {
            _actuator = new Actuator();
            var callProducePipeline = new CallProducePipe {
                Actuator = _actuator
            };
            _netMessageListener = new TcpMessageResponse {
                ProducePipe = callProducePipeline,
            };
        }


        /// <summary>
        /// 注册类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Register<T>() where T : class
        {
             _actuator.Register<T>();
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
