using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Net.Base;
using FORCEBuild.Persistence.Serialization;

namespace FORCEBuild.RPC3._0
{
    /* 工厂阶段:1.服务验证权限。
     * 当向服务注册表订阅时，会验证客户端能够调用的服务类
     */
    public class ClientSubscriber:IServiceCustomer
    {
        private Dictionary<Type, ServiceUri> urisdDictionary;

        private ServiceUri uri;

        private bool work, working;

        private IPEndPoint subscribEndPoint;

        public Guid Filter { get; set; }

        public ClientSubscriber()
        {
            urisdDictionary = new Dictionary<Type, ServiceUri>();
            subscribEndPoint=new IPEndPoint(NetHelper.InstanceIpv4,NetHelper.AviliblePort);
        }

        public void Start()
        {
            Start(1080);
        }

        public void Start(int port)
        {
            if (working) {
                return;
            }
            work = true;
            Task.Run(() => {
                var client = new UdpClient(port);
                working = true;
                IPEndPoint endPoint = null;
                while (work) {
                    if (client.Available==0) {
                        Thread.Sleep(200);
                        continue;
                    }
                    var data = client.Receive(ref endPoint);
                    var head = data.ToStruct<RegistryInfo>();
                    if (!head.IsCorrect) continue;
                }
                working = false;
            });
        }



        
        public void End()
        {
            if (working) {
                work = false;
            }
        }

        public void Subscribe(Action<ServiceNotifyType, List<ServiceNodeInfo>> callbackAction)
        {
            
        }



        public ServiceUri GetServiceUri(Type type)
        {
            return null;
        }
    }
}