using System;
using System.Collections.Generic;
using System.Net;

namespace FORCEBuild.Net.DistributedService
{
    /// <summary>
    /// 包含了服务物理信息和可用函数的节点
    /// </summary>
    [Serializable]
    public class ServiceNodeInfo
    {
        /// <summary>
        /// 提供的接口列表
        /// </summary>
        public List<string> InterfacesList { get; set; }

        /// <summary>
        /// 服务物理地址
        /// </summary>
        public IPEndPoint EndPoint { get; set; }
        /// <summary>
        /// 服务程序集
        /// </summary>
        public Guid AssemblyGuid { get; set; }
        /// <summary>
        /// 服务生命周期标识
        /// </summary>
        public Guid ServiceGuid { get; set; }


        public ServiceNodeInfo()
        {
            
        }
    }
}