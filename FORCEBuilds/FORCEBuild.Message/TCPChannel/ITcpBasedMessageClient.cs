using System.Net;
using FORCEBuild.Net.Abstraction;

namespace FORCEBuild.Net.TCPChannel
{
    /// <summary>
    /// 联网服务消费者
    /// </summary>
    public interface ITcpBasedMessageClient: IMessageClient
    {
        /// <summary>
        /// 服务器终结点
        /// </summary>
        IPEndPoint RemoteChannel { get; set; }
    }
}