using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Net.TCPChannel;
using FORCEBuild.Serialization;

namespace FORCEBuild.Net.ServiceGovernance
{
    /// <summary>
    /// 服务广播，在局域网内播报服务服务提供者
    /// </summary>
    public class ServiceBroadcaster
    {
        public const int BroadcastPort = 9800;

        public const string TAG = "BD";

        internal const int TAG_LENGTH = 4;

        private bool _broadcast;

        public bool Working { get; set; }

        public ITcpBasedMessageServer ServiceProvider { get; set; }

        private Guid Filter { get; set; }

        /// <summary>
        /// 无参构造调用程序集的guid，如果不存在会抛出异常。
        /// </summary>
        public ServiceBroadcaster():this(
            ((GuidAttribute)Assembly.GetEntryAssembly().
                GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value)
        {
        }

        public ServiceBroadcaster(string guidstring)
        {
            Filter = new Guid(guidstring);
        }

        public void Start(int port = BroadcastPort)
        {
            //Debug.WriteLine($"entry method");
            if (port < 1 || port > 65535) throw new Exception("端口数值错误");
            if (Working)
                return;
            _broadcast = true;
            Task.Run(() => {
                Working = true;
                //Debug.WriteLine($"broadcast start running");
                UdpClient udp = null;
                try {
                    udp = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
                    var target = new IPEndPoint(IPAddress.Broadcast, port);
                    //Debug.WriteLine($"broadcast target:{target}");
                    while (_broadcast)
                    {
                        if (ServiceProvider.IsRunning)
                        {
                            var info = new ServiceDescriptionDto(ServiceProvider.ServiceEndPoint,
                                ServiceProvider.ServiceGuid, Filter);
                            var datas = info.ToBytes();
                            udp.Send(datas, datas.Length, target);
                            //Debug.WriteLine($"broadcast sended");
                        }
                        Thread.Sleep(150);
                    }
                }
                finally {
                    //Debug.WriteLine($"broadcast closed at {port}");
                    udp?.Close();
                    Working = false;
                }
                
            });
        }

        public void End()
        {
            if (Working)
                _broadcast = false;
        }

    }
}