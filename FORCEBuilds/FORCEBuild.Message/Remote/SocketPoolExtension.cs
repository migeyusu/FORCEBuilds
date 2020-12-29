using System.Net;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.Remote
{
    public static class SocketPoolExtension
    {
        public static TcpMessageRequester CreateMessageRequest(this SocketPool socketPool,IPEndPoint endPoint)
        {
            return new TcpMessageRequester(socketPool, endPoint);
        }
    }
}