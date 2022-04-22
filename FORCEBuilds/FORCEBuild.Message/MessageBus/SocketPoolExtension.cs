using System.Net;
using FORCEBuild.Net.Base;
using FORCEBuild.Net.TCPChannel;

namespace FORCEBuild.Net.MessageBus
{
    public static class SocketPoolExtension
    {
        public static TcpMessageClient CreateMessageRequest(this ConnectionPool socketPool,IPEndPoint endPoint)
        {
            return new TcpMessageClient(socketPool, endPoint);
        }
    }
}