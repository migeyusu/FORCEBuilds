#define net
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using FORCEBuild.Net.Base;
using FORCEBuild.Serialization;

namespace FORCEBuild.Net.DistributedService
{
    public class ServiceFactory:IServiceFactory,IDisposable
    {
        internal const int TAG_LENGTH = 4;
        internal const string TAG = "RM";
        internal const string RPC = "RPC";

        private bool work, working;
      //  internal const string Struggle = "Struggle";
        /// <summary>
        /// 暂时只使用tcp通道
        /// </summary>
        public IPEndPoint RemoteChannel { get; set; }

        public IExceptionCatch ExceptionCatcher { get; set; }

        public IFormatter Formatter { get; set; }

        public Guid Filter { get; set; }

        private readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

        public ServiceFactory(bool isingroup,int port = 9290)
        {
            Formatter = new BinaryFormatter();
            work = isingroup;
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
                    RemoteChannel = head.RequestListenEndPoint;
                }
                working = false;
            });
        }

        //public Task<bool> TestChannel(IPEndPoint remotEndPoint)
        //{
        //    return Task.Run(() =>
        //    {
        //        var tcp = new TcpClient();
        //        try
        //        {
                   
        //            tcp.Connect(remotEndPoint);
        //            SendRequest(CallType.Test,null,tcp.Client);
        //            var head = tcp.Client.GetStruct<ResponseHead>();
        //            if (head.IsCorrect)
        //                return false;
        //            tcp.Close();
        //            return true;
        //        }
        //        catch
        //        {
        //            tcp.Close();
        //            return false;
        //        }
        //    });
        //}

        //public Task<bool> TestChannel()
        //{
        //    return TestChannel(RemoteChannel);
        //}

        public T Create<T>() where T:class 
        {
            var type = typeof(T);
            var remoteInterface = type.GetCustomAttribute<RemoteInterfaceAttribute>();
            if (remoteInterface == null)
                throw new ArgumentException("未标记的远程类型");
            var irpcIntercept =
                _proxyGenerator.CreateInterfaceProxyWithoutTarget(type,
                    new RemoteMethodInterceptor {
                        InterfaceType = typeof(T),
                        RemoteProceed = Irpc_RemoteProceed,
                        ExceptionCatch = ExceptionCatcher
                    });
            return (T) irpcIntercept;
        }

        private void SendRequest(CallType callType, object dto, Socket socket)
        {
            if (dto != null)
            {
                var memoryStream = new MemoryStream();
                Formatter.Serialize(memoryStream, dto);
                var dataBytes = memoryStream.ToArray();
                socket.SendRequest(callType,dataBytes);
            }
            else
            {
                socket.SendRequest(callType,new byte[0]);
            }
        }

        private object Irpc_RemoteProceed(InterfaceCallRequest request)
        {
            TcpClient client = null;
            try
            {
                client = new TcpClient();
                client.Connect(RemoteChannel);
                // 参数体请求
                SendRequest(CallType.Call, request, client.Client);
                
                #region 结果接收

                var responseHead = client.Client.GetStruct<ResponseHead>();//GetResponseHead(client.Client);
                if (!responseHead.IsCorrect)
                    throw new IOException("接受到的数据发生损坏！");
                var receiveStream = client.Client.GetSpecificLenStream(responseHead.LeaveLength);
                if (!responseHead.IsProcessSuccess)
                    throw (Exception) Formatter.Deserialize(receiveStream);
                return responseHead.LeaveLength == 0 ? null : Formatter.Deserialize(receiveStream);
                
                #endregion
            }
            catch (Exception exception)
            {
                
                throw new Exception("无法连上指定通道！",exception);

            }
            finally
            {
                client?.Close();
            }
        }

        public void Dispose()
        {
            if (working) {
                work = false;
            }

        }
    }
}
