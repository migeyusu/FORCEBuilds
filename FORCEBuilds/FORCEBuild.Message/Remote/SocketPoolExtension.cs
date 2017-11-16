using System.Net;
using FORCEBuild.Message.Base;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Message.Remote
{
    public static class SocketPoolExtension
    {
        public static TcpMessageRequester CreateMessageRequest(this SocketPool socketPool,IPEndPoint endPoint)
        {
            return new TcpMessageRequester(socketPool, endPoint);
        }
    }
}