using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Net.Base;
using FORCEBuild.Persistence.Serialization;

namespace FORCEBuild.DistributedService
{
    public class ServiceRegistry : IServiceProvider
    {
        public ServiceContainer Container { get; set; }

        public Guid Filter { get; set; }

        public bool Providing { get; set; }

        private BinaryFormatter Formatter;

        private bool work, working;

        private readonly Guid assemblyGuid;


        public ServiceRegistry()
        {
            var atris = Assembly.GetEntryAssembly().
                GetCustomAttributes(typeof(GuidAttribute), true);
            if (atris.Length!=0) {
                assemblyGuid = new Guid(((GuidAttribute)atris[0]).Value);
            }
      
        }

        public void Start(int port)
        {
            if (Container == null || Filter == null) {
                throw new NullReferenceException("服务未初始化");
            }
            if (working) {
                work = false;
            }
            Task.Run(() => {
                working = true;
                var udp = new UdpClient(port);
                while (work) {
                    IPEndPoint ipEndPoint = null;
                    if (udp.Available == 0) {
                        Thread.Sleep(200);
                        continue;
                    }
                    var head = udp.Receive(ref ipEndPoint).ToStruct<RegistryInfo>();
                    if (!head.IsCorrect && head.Filter != Filter) continue;
                    var tcp = new TcpClient();
                    tcp.Connect(head.ServiceListenEndPoint);
                    var socket = tcp.Client;
                    var types = Container.InterfaceList.Select(type => type.FullName);
                    var servicenodeinfo = new ServiceNodeInfo()
                    {
                        AssemblyGuid = assemblyGuid,
                        EndPoint = Container.ServiceEndPoint,
                        ServiceGuid = Container.ServiceGuid,
                        InterfacesList = types.ToList()
                    };
                    SendRequest(CallType.Info, servicenodeinfo, socket);
                    //心跳+状态
                    using (var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total")) {
                        using (var ramCounter = new PerformanceCounter("Memory", "Available MBytes")) {
                            while (socket.Connected && work)
                            {
                                Thread.Sleep(1100);
                                var info = new ServiceNodeRealTimeInfo()
                                {
                                    CpuUsage = cpuCounter.NextValue(),
                                    RamUsage = ramCounter.NextValue()
                                };
                                SendRequest(CallType.Heart, info, socket);
                            }
                        }
                    }
                    socket.Close();
                    socket.Dispose();

                }
                udp.Close();
                working = false;
            });
        }

        public void Start()
        {
            Start(9290);
        }

        private void SendRequest(CallType callType, object dto, Socket socket)
        {
            if (dto != null)
            {
                var memoryStream = new MemoryStream();
                Formatter.Serialize(memoryStream, dto);
                var dataBytes = memoryStream.ToArray();
                socket.SendRequest(callType, dataBytes);
            }
            else
            {
                socket.SendRequest(callType, new byte[0]);
            }
        }

        public void End()
        {
            if (working) {
                work = false;
            }   

        }
    }
}