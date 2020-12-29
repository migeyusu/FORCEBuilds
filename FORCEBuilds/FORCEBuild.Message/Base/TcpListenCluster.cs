using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FORCEBuild.Net.Base
{
    public class ClientItem
    {
        public TcpClient Client { get; set; }
        public bool WorkEnable { get; set; }//允许运行
        public IPAddress  Remote { get; set; }//监听模式下
        public IPEndPoint Target { get; set; }//连接模式下的初始参数
        public Exception Error { get; set; }
      //  public NetworkStream ClientStream { get; set; }
      //  public bool Connected { get; set; }//是否连上
    }
    /// <summary>
    /// 
    /// </summary>
    public class TcpListenCluster
    {
        public int Interval { get; set; }
        public int BufferSize { get; set; }
        public event Action<IPAddress> OnAccept;//连接成功
        public event Action OnEndListen;//停止监听
        public event Action<IPAddress> OnDisConnect;//断开
        public event Action<IPAddress,byte[]> OnDataReceive;//可供回写，数据到达
        TcpListener Listener;
        bool listening;
        readonly List<ClientItem> ClientTable;//列表
        public int ConnectedCount//连上数量
            => ClientTable.Count;

        public TcpListenCluster(IPEndPoint local,int workinterval)
        {
            BufferSize = 2048;
            Interval = workinterval;
            Listener = new TcpListener(local);
            ClientTable = new List<ClientItem>();
        }
        public TcpListenCluster()
        {
            BufferSize = 2048;
            Interval = 500;
            Listener = new TcpListener(NetHelper.InstanceEndPoint);
            ClientTable = new List<ClientItem>();
        }
        public void StartListen()//侦听
        {
            if (listening)
                return;
            listening = true;
            Task.Run(()=>ListenThread());
        }
        public void StartListen(IPEndPoint local)
        {
            if (listening)
                return;
            listening = true;
            Listener = new TcpListener(local);
            Task.Run(() => ListenThread());
        }
        public void EndListen()
        {
            listening = false;
        }
        void ListenThread()//监听线程
        {
            Listener.Start();
            try
            {
                while (listening)
                {
                    if (Listener.Pending())
                    {
                        var td = new Thread(ClientThread) {IsBackground = true};
                        var ct = new ClientItem {Client = Listener.AcceptTcpClient()};
                        ClientTable.Add(ct);
                        td.Start(ct);   
                    }
                    Thread.Sleep(Interval);
                }
            }
            finally
            {
                Listener.Stop();
                OnEndListen?.Invoke();
            }
        }
        /// <summary>
        /// 端口独立线程，crtsnt数组序号
        /// </summary>
        void ClientThread(object clienttable)
        {
            var table = (ClientItem)clienttable;
            try
            {
               table.WorkEnable = true;
               table.Remote = IPAddress.Parse(table.Client.Client.RemoteEndPoint.ToString().Split(':')[0]);
                OnAccept?.Invoke(table.Remote);
                using (var ns = table.Client.GetStream())
               {
                   var buffer = new byte[BufferSize];
                   var read = 0;
                   while (table.WorkEnable && table.Client.Connected)
                   {
                       using (var ms = new MemoryStream())
                       {
                           while (ns.DataAvailable)//!sr.EndOfStream)
                           {
                               read = ns.Read(buffer, 0, BufferSize);
                               ms.Write(buffer, 0, read);
                           }
                            OnDataReceive?.Invoke(table.Remote, ms.ToArray());
                        }
                       Thread.Sleep(Interval);
                   }
               }
            }
            finally
            {
                table.Client.Close();
                ClientTable.Remove(table);
                OnDisConnect?.Invoke(table.Remote);
            }
        }
        //服务端写入
        public void WriteItem(ClientItem ct, byte[] data)
        {
            if (!ct.Client.Connected)
            {
                throw new InvalidOperationException("不可写入！已断开"); 
            }
            ct.Client.GetStream().Write(data, 0, data.Length);
        }
        //只能回复已连接对象
        public void Repeat(IPAddress remote, byte[] buffer)
        {
            var list = from x in ClientTable
                       where x.Remote.Equals(remote)
                       select x;
            var ctl = list.ToList<ClientItem>();
            if (ctl.Count == 0)
            {
                throw new Exception("未获得该连接或远程连接已关闭");
            }
            ctl[0].Client.GetStream().Write(buffer, 0, buffer.Length);
        }
        ~TcpListenCluster()
        {
            listening = false;
            foreach (var x in ClientTable)
                x.WorkEnable = false;
        }
    }
}