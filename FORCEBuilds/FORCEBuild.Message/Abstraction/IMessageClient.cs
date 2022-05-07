using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.Abstraction
{
    /// <summary>
    /// 基于消息的客户端
    /// </summary>
    public interface IMessageClient
    {
        /// <summary>
        /// 设置对流和消息间转换的序列化器
        /// </summary>
        IFormatter Formatter { get; set; }

        TK GetResponse<T, TK>(T message) where T : IMessage
            where TK : IMessage;

        Task<TK> GetResponseAsync<T, TK>(T message, CancellationToken token) where T : IMessage
            where TK : IMessage;

        /// <summary>
        /// 检查是否已连接
        /// </summary>
        bool CanRequest { get; }
    }
}