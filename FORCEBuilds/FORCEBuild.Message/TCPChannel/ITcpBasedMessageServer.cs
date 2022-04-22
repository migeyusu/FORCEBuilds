using System.Net;
using FORCEBuild.Net.Abstraction;

namespace FORCEBuild.Net.TCPChannel
{
    /// <summary>
    /// 基于tcp的消息服务器
    /// </summary>
    public interface ITcpBasedMessageServer:IMessageServer
    {
        /// <summary>
        /// 服务终结点
        /// </summary>  
        IPEndPoint ServiceEndPoint { get; set; }
    }
}