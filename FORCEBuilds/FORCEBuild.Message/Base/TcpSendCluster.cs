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
    public class TcpSendCluster : INetSender
    {
        public int Interval { get; set; }
        public int BufferSize { get; set; }
        public event Action<IPAddress> OnDisconnect;
        public event Action<IPAddress, byte[]> OnDataReceive;//可供回写，数据到达
        readonly List<ClientItem> ControlTable;
        public TcpSendCluster()
        {
            ControlTable = new List<ClientItem>();
        }
        public void Send(IPEndPoint remote, byte[] bytes)
        {
            var list = from x in ControlTable
                       where x.Target.Equals(remote)
                       select x;
            var cts = list.ToList();
            if (cts.Count == 0)
            {
                var ct = new ClientItem
                {
                    Client = new TcpClient(),
                    WorkEnable = true,
                    Target = remote
                };
                Task.Run(() => SendThread(ct, bytes));
            }
            else
            {
                Task.Run(() => { SendThread(cts[0], bytes); });
            }

        }
        void SendThread(ClientItem ct, byte[] bytes)
        {
            if (!ct.Client.Connected)
                ct.Client.Connect(ct.Target);
            if (bytes.Length <= BufferSize)
                ct.Client.GetStream().Write(bytes, 0, bytes.Length);
            else
            {
                var position = 0;
                while (position + BufferSize <= bytes.Length)
                {
                    ct.Client.GetStream().Write(bytes, position, BufferSize);
                    position += BufferSize;
                }
                if (position < bytes.Length)
                    ct.Client.GetStream().Write(bytes, position, bytes.Length - position);
            }
        }
        void ReadThread(ClientItem ct)
        {
            using (var ns = ct.Client.GetStream())
            {
                var buffer = new byte[BufferSize];
                var read = 0;
                while (ct.WorkEnable && ct.Client.Connected)
                {
                    using (var ms = new MemoryStream())
                    {
                        while (ns.DataAvailable)
                        {
                            read = ns.Read(buffer, 0, BufferSize);
                            ms.Write(buffer, 0, read);
                        }
                        OnDataReceive?.Invoke(ct.Target.Address, ms.ToArray());
                    }
                    Thread.Sleep(Interval);
                }
            }
            ct.Client.Close();
            ControlTable.Remove(ct);
            OnDisconnect?.Invoke(ct.Target.Address);
        }
        ~TcpSendCluster()
        {
            foreach (var x in ControlTable)
            {
                x.WorkEnable = false;
            }
        }
    }
}
