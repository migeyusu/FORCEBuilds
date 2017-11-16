using System.Net;

namespace FORCEBuild.Net.Service
{
    /// <summary>
    /// 规范的TCP服务
    /// </summary>
    public interface ITcpServiceProvider:IServiceProvider
    {

        /// <summary>
        /// 服务终结点
        /// </summary>  
        IPEndPoint ServiceEndPoint { get; set; }

    }
}