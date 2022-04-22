using System;
using System.Runtime.Serialization;

namespace FORCEBuild.Net.Abstraction
{
    /// <summary>
    /// 消息服务器，负责作为具体服务的宿主
    /// </summary>
    public interface IMessageServer : IDisposable
    {
        IFormatter Formatter { get; set; }

        /// <summary>
        /// 服务路由
        /// </summary>
        IMessageProcessRoutine Routine { get; }

        /// <summary>
        /// 服务标识,随生命周期更新
        /// </summary>
        Guid ServiceGuid { get; }

        /// <summary>
        /// 服务运行状态
        /// </summary>
        bool IsRunning { get; }

        void Start();

        void Stop();
    }
}