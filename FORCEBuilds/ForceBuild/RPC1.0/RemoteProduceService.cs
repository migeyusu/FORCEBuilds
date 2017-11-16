using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FORCEBuild.Net.Base;
using FORCEBuild.Persistence.Serialization;

namespace FORCEBuild.RPC1._0
{
    /*
     * 线程安全由实现类完成
     * 通过guid同步，可保持
     */

    public class RemoteProduceService
    {
        public IPEndPoint ServiceEndPoint { get; private set; }

        private bool listening, listen;

        private readonly XSoapSerializer serializer;
        /// <summary>
        /// 包含的所有请求的对象
        /// </summary>
        private readonly Dictionary<Guid, object> RemoteObjects;

        private readonly Dictionary<Type, object> RemoteSingletonObjects;
        /// <summary>
        /// 远程服务对象
        /// </summary>
        private Dictionary<Type, Stack<object>> RemoteServicePools;
        /// <summary>
        /// 自建ioc
        /// </summary>
        public Func<Type, object> CreateInstance;

        public RemoteProduceService()
        {
            listening = false;
            serializer = new XSoapSerializer();
            RemoteObjects = new Dictionary<Guid, object>();
            RemoteSingletonObjects = new Dictionary<Type, object>();
            //原IOC提供，2017.5.21去除
         //   CreateInstance = XActivator.GetInstance;
        }

        public void Start(IPEndPoint ipEndPoint)
        {
            if (listening)
                return;
            listen = true;
            ServiceEndPoint = ipEndPoint;
            Task.Run(() =>
            {
                TcpListener listener = null;
                try
                {
                    listener = new TcpListener(ipEndPoint);
                    listener.Start();
                    listening = true;
                    while (listen)
                    {
                        if (listener.Pending())
                            Task.Run(() => Produce(listener.AcceptTcpClient().Client));
                        else
                            Thread.Sleep(100);
                    }
                    listener.Stop();
                    listening = false;
                }
                catch (Exception e)
                {
                    if (listening)
                    {
                        listener.Stop();
                    }
                    MessageBox.Show("服务将关闭：" + e.Message);
                }

            });
        }

        public void End()
        {
            listen = false;
        }

        private void Produce(Socket socket)
        {
            var requestHead = GetRequestHead(socket);
            if (!requestHead.IsCorrect)
            {
                SendResponse(new ResponseHead {IsProcessSuccess = false}, new IOException("接收的数据发生损坏！"), socket);
                return;
            }
            try
            {
                switch (requestHead.Calltype)
                {
                    case CallType.Call:
                        var callrequest = serializer.Deserialize<MethodInvokeRequest>
                            (NetHelper.GetSpecificLenStream(socket, requestHead.Len));
                        if (RemoteObjects.ContainsKey(callrequest.SyncGuid))
                        {
                            var callobj = RemoteObjects[callrequest.SyncGuid];
                            try
                            {
                                var result = callrequest.Method.Invoke(callobj, callrequest.Parameters);
                                SendResponse(new ResponseHead {IsProcessSuccess = true}, result, socket);
                            }
                            catch (Exception e)
                            {
                                SendResponse(new ResponseHead {IsProcessSuccess = false}, e, socket);
                            }
                        }
                        else
                            SendResponse(new ResponseHead {IsProcessSuccess = false},
                                new NullReferenceException("对象已经被释放或空的引用"), socket);
                        break;
                    case CallType.Dispose:
                        var disposerequest = serializer.Deserialize<DisposedRequest>
                            (NetHelper.GetSpecificLenStream(socket, requestHead.Len));
                        if (RemoteObjects.Keys.Contains(disposerequest.SyncGuid))
                        {
                            var oc = RemoteObjects[disposerequest.SyncGuid] as IDisposable;
                            RemoteObjects.Remove(disposerequest.SyncGuid);
                            if (RemoteSingletonObjects.Keys.Contains(disposerequest.ClassType))
                                RemoteSingletonObjects.Remove(disposerequest.ClassType);
                            oc.Dispose();
                        }
                        break;
                    case CallType.Ini:
                        var inirequest = serializer.Deserialize<InstantiationRequest>
                            (NetHelper.GetSpecificLenStream(socket, requestHead.Len));
                        object Iniobj;
                        if (inirequest.IsSingleton && RemoteSingletonObjects.ContainsKey(inirequest.ClassType))
                        {
                            Iniobj = RemoteSingletonObjects[inirequest.ClassType];
                        }
                        else
                        {
                            Iniobj = CreateInstance(inirequest.ClassType);
                            RemoteObjects.Add(inirequest.SyncGuid, Iniobj);
                            if (inirequest.IsSingleton)
                                RemoteSingletonObjects.Add(inirequest.ClassType, Iniobj);
                        }
                        var response = new ResponseHead {IsProcessSuccess = true};
                        SendResponse(response, null, socket);

                        break;
                    case CallType.Test:
                        SendResponse(new ResponseHead {IsProcessSuccess = true}, null, socket);
                        break;
                }
            }
            catch (Exception e)
            {
                SendResponse(new ResponseHead {IsProcessSuccess = false}, e, socket);
            }
            socket.Close();
        }

        void SendResponse(ResponseHead request, object transfer, Socket socket)
        {
            request.Tag = Encoding.Unicode.GetBytes("RM");
            if (transfer != null)
            {
                var dataBytes = serializer.Serialize(transfer).ToArray();
                request.NextLen = BitConverter.GetBytes(dataBytes.Length);
                socket.Send(request.ToBytes());
                socket.Send(dataBytes);
            }
            else
                socket.Send(request.ToBytes());
        }

        RequestHead GetRequestHead(Socket socket)
        {
            var headsize = RequestHead.SelfLen;
            var receiveBytes = new byte[headsize];
            socket.Receive(receiveBytes, receiveBytes.Length, SocketFlags.None);
            return receiveBytes.ToStruct<RequestHead>();
        }
    }
}
