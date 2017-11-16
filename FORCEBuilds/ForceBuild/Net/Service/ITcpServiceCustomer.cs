using System.Net;
using FORCEBuild.Crosscutting.Log;

namespace FORCEBuild.Net.Service
{
    /// <summary>
    /// 联网服务消费者
    /// </summary>
    public interface ITcpServiceCustomer
    {
        /// <summary>
        /// 用于属性注入，表示所消费服务的终结点
        /// </summary>
        IPEndPoint RemoteChannel { get; set; }

        ILog Log { get; set; }

    }
}