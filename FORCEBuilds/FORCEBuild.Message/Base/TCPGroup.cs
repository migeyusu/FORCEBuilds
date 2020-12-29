using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FORCEBuild.Net.Base
{
    /// <summary>
    /// 支持单个监听端口，多次连接
    /// </summary>
    public class TcpGroup
    {
        public int workInterval { get; set; }
        readonly IPEndPoint LocalEndPoint;
        readonly TcpListener listener;
        bool listenning = false;
        public Action<ClientItem> OnConnectError, OnConnected, OnEndConnect;

        public Action<Exception> OnListenError;

        public Action OnEndListen;

        public TcpGroup(string localip, int localport)
        {
            LocalEndPoint = new IPEndPoint(IPAddress.Parse(localip), localport);
            listener = new TcpListener(LocalEndPoint);
            workInterval = 100;
        }

        public TcpGroup()
        {
            workInterval = 100;
        }
        
        public ClientItem Connect(string ip, int port, Action<byte[], ClientItem> OnDataReceiveCallBack)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            return Connect(endPoint, OnDataReceiveCallBack);
        }

        /// <summary>
        /// 同步连接方法
        /// </summary>
        /// <param name="remote"></param>
        /// <returns></returns>
        public ClientItem Connect(IPEndPoint remote, Action<byte[], ClientItem> OnDataReceiveCallBack)
        {
            var item = new ClientItem {
                Client = new TcpClient(),
                Target = remote
            };
            item.Client.Connect(item.Target);
            item.WorkEnable = true;
            if (OnDataReceiveCallBack != null) {
                Task.Run(() => {
                    ClientThread(item, OnDataReceiveCallBack);
                });
            }
            return item;
        }

        public void Listen(Action<byte[], ClientItem> OnDataReceiveCallBack)
        {
            if (listenning)
                return;
            listenning = true;
            Task.Run(() => {
                while (listenning) {
                    listener.Start();
                    try {
                        while (listenning) {
                            if (listener.Pending()) {
                                var item = new ClientItem {
                                    Client = listener.AcceptTcpClient()
                                };
                                OnConnected?.Invoke(item);
                                Task.Run(() => {
                                    ClientThread(item, OnDataReceiveCallBack);
                                });
                            }
                            Thread.Sleep(workInterval);
                        }
                    }
                    catch (Exception ex) {
                        OnListenError?.Invoke(ex);
                        throw;
                    }
                    finally {
                        listener.Stop();
                        OnEndListen?.Invoke();
                    }
                }
            });
        }

        /// <summary>
        /// 端口独立线程
        /// </summary>
        private void ClientThread(ClientItem item, Action<byte[], ClientItem> OnDataReceiveCallBack)
        {
            try {
                using (var steam = item.Client.GetStream()) {
                    while (item.WorkEnable) {
                        if (item.Client.Available > 0) {
                            var datas = new byte[item.Client.Available];
                            steam.Read(datas, 0, datas.Length);
                            OnDataReceiveCallBack(datas, item);
                        }
                        Thread.Sleep(workInterval);
                    }
                }
            }
            catch (Exception ex) {
                item.Error = ex;
                OnConnectError?.Invoke(item);
            }
            finally {
                item.WorkEnable = false;
                item.Client.Close();
                OnEndConnect?.Invoke(item);
            }
        }

        public void EndListen()
        {
            listenning = false;
        }
    }
}
