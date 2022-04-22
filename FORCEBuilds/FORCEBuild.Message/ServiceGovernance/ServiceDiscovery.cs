using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Net.TCPChannel;
using FORCEBuild.Serialization;

namespace FORCEBuild.Net.ServiceGovernance
{

    /// <summary>
    /// 自主服务发现，允许绑定服务消费者
    /// </summary>
    public class ServiceDiscovery
    {
        private bool _listenservicing, _listenservice;

        public event Action<ServiceDescription> NewServiceAdded;

        public event Action<ServiceDescription> ServiceRemoved;

        public ConcurrentDictionary<Guid,ServiceDescription> ServiceList { get; set; }
        /// <summary>
        /// 过滤标识
        /// </summary>
        public Guid Filter { get; set; }

        private readonly ITcpBasedMessageClient _bindServiceFactory;

        public ServiceDiscovery() { }

        /// <summary>
        /// 绑定一个继承于ITcpServiceCustomer接口的服务消费者
        /// </summary>
        /// <param name="filterUid">过滤标识，建议使用“程序集”GUID</param>
        /// <param name="bindServiceCustomer">绑定是Lazy的</param>
        public ServiceDiscovery(string filterUid, ITcpBasedMessageClient bindServiceCustomer = null)
        {
            ServiceList = new ConcurrentDictionary<Guid, ServiceDescription>();
            _bindServiceFactory = bindServiceCustomer;
            Filter = Guid.Parse(filterUid);
        }
        
        public void Start(int port = ServiceBroadcaster.BroadcastPort)  
        {
            if (_listenservicing)
                return;
            _listenservicing = true;
            _listenservice = true;
            Task.Run(() => {
              //  Debug.WriteLine($"service-listener start at port{port}");
                var udp = new UdpClient(port);
                try {
                    IPEndPoint ipEndPoint = null;
                    while (_listenservice) {
                        var serviceInfo = udp.Receive(ref ipEndPoint).ToStruct<ServiceDescriptionDto>();
                        //Debug.WriteLine($"get service_info form {ipEndPoint},length={Marshal.SizeOf(head)}"); 
                        if (!serviceInfo.GetIsCorrect(Filter)) continue;
                        //   Debug.WriteLine($"validate service_info form {ipEndPoint}");
                        if (ServiceList.TryGetValue(serviceInfo.ServiceUid, out ServiceDescription serviceDefine))
                        {
                            serviceDefine.LastTime = DateTime.Now;
                        }
                        else
                        {
                            var define = new ServiceDescription
                            {
                                Guid = serviceInfo.ServiceUid,
                                LastTime = DateTime.Now,
                                ProviderIpEndPoint = new IPEndPoint(new IPAddress(serviceInfo.IpBytes), serviceInfo.ServicePort)
                            };
                            ServiceList.TryAdd(serviceInfo.ServiceUid, define);
                            //     Debug.WriteLine(
                            //       $"new service_info add uid:{define.Guid}\r\n endpoint:{define.ProviderIpEndPoint}");
                            if (_bindServiceFactory != null && _bindServiceFactory.RemoteChannel == null)
                            {
                                _bindServiceFactory.RemoteChannel = define.ProviderIpEndPoint;
                            }
                            OnNewServiceAdded(define);
                        }
                    }
                    //维护服务列表
                    foreach (var guid in ServiceList.Keys)
                    {
                        if ((DateTime.Now - ServiceList[guid].LastTime).TotalSeconds > 2)
                        {
                            if (ServiceList.TryRemove(guid, out ServiceDescription define))
                            {
                                //Debug.WriteLine(
                                //$"old service_info removed uid:{define.Guid}\r\n endpoint:{define.ProviderIpEndPoint}");
                                if (_bindServiceFactory != null &&
                                    Equals(_bindServiceFactory.RemoteChannel, define.ProviderIpEndPoint))
                                {
                                    _bindServiceFactory.RemoteChannel = null;
                                }
                                OnServiceRemoved(define);
                            }
                        }
                    }
                }
                finally {
                //    Debug.WriteLine($"service-listener closed at {port}");
                    udp.Close();
                    _listenservicing = false;
                }
            });
        }
        
        /// <summary>
        /// 等待指定时间直到获得服务列表
        /// </summary>
        /// <returns></returns> 
        public bool WaitAll(int seed)
        {
            int total = 0;
            do {
                Thread.Sleep(50);
                if (!ServiceList.IsEmpty) {
                    return true;
                }
            } while ((total += total) <= seed);
            return false;
        }

        public void End()
        {
            if (_listenservicing) {
                _listenservice = false;
            }
        }

        protected virtual void OnNewServiceAdded(ServiceDescription obj)
        {
            NewServiceAdded?.Invoke(obj);
        }

        protected virtual void OnServiceRemoved(ServiceDescription obj)
        {
            ServiceRemoved?.Invoke(obj);
        }
    }
}