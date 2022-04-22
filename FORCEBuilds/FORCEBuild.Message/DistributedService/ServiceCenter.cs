using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FORCEBuild.Crosscutting.Log;
using FORCEBuild.Net.Base;
using FORCEBuild.Serialization;

namespace FORCEBuild.Net.DistributedService
{
    /* 2017.6.6：订阅目前基于地址的，完善的订阅需要经过消息处理，允许自定义订阅
     * 后期将添加消息处理过程
     * 控制台广播自己所在的位置，等待tcp连接，订阅和取消订阅都需要指令，
     * 但是因为可能的网络错误，订阅列表的更新采用lazy update
     * 只有当需要通知时才检查该调用方是否可用
     * 2017.6.7：取消订阅/发布机制，改为代理机制，修改请求头实现代理
     * 为较远的将来的均衡负载做准备（先期演示）
     */

    /// <summary>
    /// 服务中心
    /// </summary>
    public class ServiceCenter
    {
        private bool broadcast, broadcasting, work, working;

        private const int buffersize = 4096;

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, ServiceNode>> serviceNodes;

        private readonly ConcurrentDictionary<Guid, ServiceNode> allServiceNodes;
        ///// <summary>
        ///// 等待通知列表
        ///// </summary>
        //private SynchronizedCollection<IPEndPoint> waitNotifyEndPoints;

        public ILog Log { get; set; }

        private readonly BinaryFormatter formatter;

        public IPEndPoint RequestEndPoint { get; set; }

        public IPEndPoint ServiceListenEndPoint { get; set; }

        /// <summary>
        /// 生命周期标识
        /// </summary>
        public Guid RegistryGuid { get; set; }

        /// <summary>
        /// 服务中心标识
        /// </summary>
        public Guid Filter { get; set; }

        public int BroadcastPort { get; set; }
        private IMapper _mapper;

        public ServiceCenter()
        {
            serviceNodes = new ConcurrentDictionary<string, ConcurrentDictionary<Guid, ServiceNode>>();
            allServiceNodes = new ConcurrentDictionary<Guid, ServiceNode>();
            formatter = new BinaryFormatter();
            // waitNotifyEndPoints = new SynchronizedCollection<IPEndPoint>();
            var mapperConfiguration =
                new MapperConfiguration(expression => expression.CreateMap<ServiceNodeInfo, ServiceNode>());
            _mapper = mapperConfiguration.CreateMapper();
        }

        public void Start(int broadcastport = 9290)
        {
            if (working)
            {
                return;
            }

            BroadcastPort = broadcastport;
            work = true;
            RegistryGuid = Guid.NewGuid();
            var ipa = NetHelper.InstanceIpv4;
            RequestEndPoint = new IPEndPoint(ipa, NetHelper.AvailablePort);
            ServiceListenEndPoint = new IPEndPoint(ipa, NetHelper.AvailablePort);
            Task.Run(new Action(ListenService));
            Task.Run(new Action(ForwardRequest));
            Task.Run(new Action(Broadcast));
        }

        /// <summary>
        /// 广播线程
        /// </summary>
        private void Broadcast()
        {
            //   broadcasting = true;
            working = true;
            var client = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
            var target = new IPEndPoint(IPAddress.Broadcast, BroadcastPort);
            var time = DateTime.Now.ToBinary();
            var registryBytes =
                new RegistryInfo(RequestEndPoint, ServiceListenEndPoint, RegistryGuid, time, Filter).ToBytes();
            while (work)
            {
                client.Send(registryBytes, registryBytes.Length, target);
                Thread.Sleep(200);
            }

            client.Close();
            working = false;
            // broadcasting = false;
        }


        private void ListenService()
        {
            working = true;
            var listener = new TcpListener(ServiceListenEndPoint);
            listener.Start();
            while (work)
            {
                if (listener.Pending())
                {
                    var socket = listener.AcceptSocket();
                    Task.Run(() => { ServiceAcquire(socket); });
                }
                else
                {
                    Thread.Sleep(200);
                }
            }

            listener.Stop();
            working = false;
        }

        /// <summary>
        /// 服务获取
        /// </summary>
        /// <param name="socket"></param>
        private void ServiceAcquire(Socket socket)
        {
            var head = socket.ReadStruct<StreamMessageHeader>();
            if (!head.Verify())
                return;
            //设置node
            var nodeInfo =
                formatter.Deserialize(socket.GetSpecificLenStream(head.Length)) as ServiceNodeInfo;
            var serviceNode = _mapper.Map<ServiceNodeInfo, ServiceNode>(nodeInfo);
            foreach (var str in nodeInfo.InterfacesList)
            {
                if (!serviceNodes.ContainsKey(str))
                {
                    serviceNodes.TryAdd(str, new ConcurrentDictionary<Guid, ServiceNode>());
                }

                if (!serviceNodes[str].ContainsKey(nodeInfo.ServiceGuid))
                {
                    serviceNodes[str].TryAdd(serviceNode.ServiceGuid, serviceNode);
                }
            }

            if (!allServiceNodes.ContainsKey(nodeInfo.ServiceGuid))
            {
                allServiceNodes.TryAdd(nodeInfo.ServiceGuid, serviceNode);
            }

            //if (node==null) {
            //    //没有实际接口功能将退出
            //    return;
            //}
            var lastheartTime = DateTime.Now;
            //心跳+信息
            while (socket.Connected && work)
            {
                Thread.Sleep(1000);
                if (socket.Available > 0)
                {
                    var head1 = socket.ReadStruct<RequestHead>();
                    if (!head1.IsCorrect || head1.Calltype != CallType.Heart) continue;
                    var realinfo = formatter.Deserialize(socket.GetSpecificLenStream(head1.LeaveLength))
                        as ServiceNodeRealTimeInfo;
                    _mapper.Map(realinfo, serviceNode);
                }

                if ((DateTime.Now - lastheartTime).TotalSeconds > 3) break;
            }

            socket.Close();
            socket.Dispose();
            //清理
            if (allServiceNodes.ContainsKey(nodeInfo.ServiceGuid))
                allServiceNodes.TryRemove(nodeInfo.ServiceGuid, out serviceNode);
            foreach (var nodedic in serviceNodes.Values)
                nodedic.TryRemove(nodeInfo.ServiceGuid, out serviceNode);
        }

        /// <summary>
        /// 转发请求
        /// </summary>
        /// <param name="socket"></param>
        private void ForwardRequest()
        {
            working = true;
            var listener = new TcpListener(RequestEndPoint);
            listener.Start();
            while (work)
            {
                if (listener.Pending())
                {
                    var socket = listener.AcceptSocket();
                    Task.Run(() =>
                    {
                        try
                        {
                            var head = socket.ReadStruct<RequestHead>();
                            if (head.IsCorrect)
                            {
                                var steam = socket.GetSpecificLenStream(head.LeaveLength);
                                var call = formatter.Deserialize(steam) as InterfaceCallRequest;
                                serviceNodes.TryGetValue(call.InterfaceType,
                                    out ConcurrentDictionary<Guid, ServiceNode> nodes);
                                //取得服务器的连接
                                if (nodes == null || nodes.Count == 0)
                                {
                                    var exp = new Exception("需要的服务未开启");
                                    SendResponse(false, exp, socket);
                                }
                                else
                                {
                                    var select = nodes.Values.OrderBy(node => node.CpuUsage).First();
                                    select.TcpSockets.TryPop(out Socket child);
                                    if (child == null)
                                    {
                                        var client = new TcpClient();
                                        client.Connect(select.EndPoint);
                                        child = client.Client;
                                    }

                                    child.Send(head.ToBytes());
                                    child.Send(steam.ToArray());
                                    var response = child.ReadStruct<ResponseHead>();
                                    var datas = new byte[response.LeaveLength];
                                    child.Receive(datas, 0, response.LeaveLength, SocketFlags.None);
                                    socket.Send(response.ToBytes());
                                    socket.Send(datas);
                                    select.TcpSockets.Push(child);
                                }
                            }
                            else
                            {
                                throw new Exception("收到的请求错误");
                            }
                        }
                        catch (Exception e)
                        {
                            SendResponse(false, new Exception("网络通信错误", e), socket);
                            Log.Write(e);
                        }
                        finally
                        {
                            socket.Close();
                            socket.Dispose();
                        }
                    });
                }
            }

            listener.Stop();
            working = false;
        }

        private void SendResponse(bool isExecuted, object dto, Socket socket)
        {
            if (dto != null)
            {
                var memoryStream = new MemoryStream();
                formatter.Serialize(memoryStream, dto);
                var dataBytes = memoryStream.ToArray();
                socket.SendResponse(isExecuted, dataBytes);
            }
            else
            {
                socket.SendResponse(isExecuted, new byte[] { });
            }
        }

        public void End()
        {
            if (working)
            {
                work = false;
                broadcast = false;
            }
        }

    }
} /// <summary>
/// 服务订阅/取消订阅
/// 2017.6.7 取消该机制，因为难以实现均衡负载，改为转发机制
/// </summary>
//private void ListenSubscribe()
//{
//    working = true;
//    var listener = new TcpListener(SubscribeEndPoint);
//    listener.Start();
//    while (work) {
//        if (listener.Pending()) {
//            var socket = listener.AcceptSocket();
//            Task.Run(() => {
//                //初次分配
//                var head = socket.GetStruct<RequestHead>();
//                if (head.IsCorrect) {
//                    var request =
//                        formatter.Deserialize(NetHelper.GetSpecificLenStream(socket, head.LeaveLength))
//                            as SubscribeRequest;
//                    if (request.IsCancel) {
//                        waitNotifyEndPoints.Remove(request.EndPoint);
//                    }
//                    else {
//                        waitNotifyEndPoints.Add(request.EndPoint);
//                        var clientServiceUris = new Dictionary<string, ServiceUri>();
//                        foreach (var interfaceName in request.InterfaceList)
//                        {
//                            serviceNodes.TryGetValue(interfaceName,
//                                out ConcurrentDictionary<Guid, ServiceNode> nodes);
//                            /* 决定最优节点
//                             * 2017.6.7:默认平均分配
//                             */
//                            if (nodes == null || nodes.Count == 0) continue;
//                            float usage = 100;
//                            ServiceUri uri = null;
//                            var guid = Guid.Empty;
//                            foreach (var node in nodes) {
//                                var val = node.Value;
//                                if (!(usage < val.CpuUsage)) continue;
//                                uri = val.Uri;
//                                usage = val.CpuUsage;
//                                guid = node.Key;
//                            }
//                            nodes[guid].CustomerEndPoints.Add();
//                            clientServiceUris.Add(interfaceName, uri);
//                        }
//                        //响应
//                        var response = new SubscribeResponse() { ServiceUris = clientServiceUris };
//                        var stream = new MemoryStream();
//                        formatter.Serialize(stream, response);
//                        socket.SendResponse(true, stream.ToArray());
//                    }
//                }
//                else {
//                    var ex = new Exception("请求格式错误");
//                    var stream = new MemoryStream();
//                    formatter.Serialize(stream, ex);
//                    socket.SendResponse(false, stream.ToArray());
//                }
//                //结束通信，关闭socket
//                socket.Close();
//            });
//        }
//    }
//    working = false;
//}