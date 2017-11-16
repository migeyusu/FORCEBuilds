using System;
using FORCEBuild.Crosscutting.Log;

namespace FORCEBuild.Net.Service
{
    /// <summary>
    /// 基于网络的服务提供者（多例的本地服务也可以使用）
    /// </summary>
    public interface IServiceProvider:IDisposable
    {
        ILog Log { get; set; }

        /// <summary>
        /// 服务标识,依赖生命周期
        /// </summary>
        Guid ServiceGuid { get; set; }
        /// <summary>
        /// 服务运行状态
        /// </summary>
        bool Working { get; }

        void Start();

        void End();
    }
}