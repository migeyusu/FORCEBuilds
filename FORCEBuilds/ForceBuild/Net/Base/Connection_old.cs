using System;
using System.Net;
using System.Net.Sockets;

namespace FORCEBuild.Net.Base
{
    public class Connection : IDisposable
    {
        internal SingleSocketPool Pool { get; set; }

        internal Connection(Socket socket)
        {
            this.Socket = socket;
        }

        public Socket Socket { get; set; }

        public static Connection Create(IPEndPoint endPoint)
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect(endPoint);
            return new Connection(tcpClient.Client);
        }

        public void Dispose()
        {
            if (Pool != null && !Pool.IsClosed) {
                Pool.Add(this);
                return;
            }
            Socket.Dispose();
        }
    }
}