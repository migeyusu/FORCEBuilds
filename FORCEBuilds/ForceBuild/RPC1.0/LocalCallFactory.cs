//#define net

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;
using FORCEBuild.Net.Base;
using FORCEBuild.Persistence.Serialization;

namespace FORCEBuild.RPC1._0
{
    /*
     * 简单实现远程对象调用：
     * 服务端规定一组抽象类/接口
     * 客户端使用工厂类生成的代理类
     * 代理类重写并拦截方法，将参数传到服务端
     * 服务端执行后传回返回值和执行情况
     * 1.实参转流
     * 2.发送流
     * 3.接收返回流
     * 4.返回流转实参
     * 未完成的部分：实例化和回收的跟踪
     * 2017.4.2：
     * 降低复杂程度，仅考虑用于Service，
     * 由于service是无状态的，取消同步用guid,本命名空间作为保留用途
     */

    public class LocalCallFactory
    {
        internal const int TAG_LENGTH = 4;
        internal const string TAG = "RM";
        /// <summary>
        /// 暂时只使用tcp通道
        /// </summary>
        public IPEndPoint Channel { get; set; }

        //public Exception LastException { get; set; }
        readonly XSoapSerializer serializer;
        readonly ProxyGenerator pg = new ProxyGenerator();

        public LocalCallFactory(IPEndPoint channelPoint)
        {
#if net
            Channel = channelPoint;
            if (!TestChannel())
                throw new IOException("指定的通道未能连上服务");
#endif
            serializer = new XSoapSerializer();
        }

        public bool TestChannel()
        {
            var client = new TcpClient();
            try
            {
                client.Connect(Channel);
                SendRequest(new RequestHead
                {
                    Calltype =
                        CallType.Test
                }, null, client.Client);
                var response = GetResponseHead(client.Client);
                return response.IsProcessSuccess;
            }
            catch
            {
                client.Close();
                return false;
            }
        }

        /*
         * 可以在两种模式下创建：
         * 契约模式，Service工程只暴露接口
         * 引用模式，Service的具体Produce类被引用
         * 创建时首先发送实例化请求，服务端对象池保持一个实例或单例
         * 对于接口会生成一个新的interceptor
         */

        public T CreateInstance<T>(bool isSingleton = true)
        {
            #region 本地生成     

            var type = typeof(T);
            var syncGuid = Guid.NewGuid();
            var ri = type.GetCustomAttribute<RemoteInterfaceAttribute>();
            var rc = type.GetCustomAttribute<RemoteClassAttribute>();
            IRPCIntercept irpIntercept;
            if (ri != null)
            {
                irpIntercept =
                    (IRPCIntercept)
                    pg.CreateInterfaceProxyWithoutTarget(type, new[] {typeof(IRPCIntercept)}, new RPCInterceptor());
            }
            else if (rc != null)
            {
                irpIntercept =
                    (IRPCIntercept) pg.CreateClassProxy(type, new[] {typeof(IRPCIntercept)}, new RPCInterceptor());
            }
            else
            {
                throw new ArgumentException("未标记的远程类型");
            }
            irpIntercept.SyncGuid = syncGuid;

            #endregion

#if net
            irpIntercept.RemoteProceed += Irp_RemoteProceed;
            var client = new TcpClient();
            try
            {
                client.Connect(Channel);

            #region 实例化请求

                var instantiationRequest = new InstantiationRequest
                {
                    ClassType = ri != null ? Type.GetType(ri.OverrideClass) : type,
                    IsSingleton = isSingleton,
                    SyncGuid = syncGuid,
                };
                var requestHead = new RequestHead {Calltype = CallType.Ini};
                SendRequest(requestHead, instantiationRequest, client.Client);

            #endregion

            #region 实例化响应

                var responseHead = GetResponseHead(client.Client);
                if (!responseHead.IsCorrect)
                    throw new IOException("接收到的数据发生损坏！");
                if (!responseHead.IsProcessSuccess)
                {
                    throw (Exception) serializer.Deserialize(
                        NetHelper.GetStream(client.Client, responseHead.Len));
                }

            #endregion

                client.Close();
            }
            catch (Exception e)
            {
                client.Close();
                MessageBox.Show("创建对象失败:" + e.Message);
                return default(T);
            }
#endif
            return (T) irpIntercept;
        }

        void SendRequest(RequestHead request, object transfer, Socket socket)
        {
            request.Tag = Encoding.Unicode.GetBytes(TAG);
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

        private ResponseHead GetResponseHead(Socket socket)
        {
            var headsize = ResponseHead.SelfLen;
            var receiveBytes = new byte[headsize];
            socket.Receive(receiveBytes, receiveBytes.Length, SocketFlags.None);
            return receiveBytes.ToStruct<ResponseHead>();
        }

        private object Irp_RemoteProceed(Guid guid, MethodInfo arg1, object[] arg2)
        {
            var client = new TcpClient();
            try
            {
                client.Connect(Channel);

#region 参数体请求

                var serializeMethod = new MethodInvokeRequest
                {
                    Parameters = new object[arg2.Length],
                    Method = arg1,
                    SyncGuid = guid
                };
                for (var i = 0; i < arg2.Length; i++)
                    serializeMethod.Parameters[i] = arg2[i];
                var requestHead = new RequestHead { Calltype = CallType.Call };
                SendRequest(requestHead, serializeMethod, client.Client);
#endregion

#region 结果接收
                var responseHead = GetResponseHead(client.Client);
                if (!responseHead.IsCorrect)
                    throw new IOException("数据发生损坏！");
                var receiveStream = NetHelper.GetSpecificLenStream(client.Client, responseHead.Len);
                if (!responseHead.IsProcessSuccess)
                    throw (Exception) serializer.Deserialize(receiveStream);
                client.Close();
                return serializer.Deserialize(receiveStream);
#endregion
            }
            catch (Exception e)
            {
                client.Close();
                throw new Exception("服务调用失败，未能返回结果:" + e.Message);
            }
        }

        /// <summary>
        /// 预先注册参数对象的类型可以提高性能
        /// </summary>
        public void Register(Type type)
        {
            serializer.Register(type);
        }
    }
}
