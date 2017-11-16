using System;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace FORCEBuild.DistributedService
{
    [Serializable]
    public class ServiceNode:ServiceNodeInfo,IRealtimeInfo,IDisposable
    {
        //public List<IPEndPoint> CustomerEndPoints { get; set; } 
        /// <summary>
        /// 复用连接
        /// </summary>
        public ConcurrentStack<Socket> TcpSockets { get; set; }

        public ServiceNode()
        {
          //  CustomerEndPoints = new List<IPEndPoint>();
        }

        public float CpuUsage { get; set; }

        public float NetUsage { get; set; }

        public void Dispose()
        {
            foreach (var socket in TcpSockets) {
                if (socket.Connected) {
                    socket.Close();
                }
            }
        }
    }
}