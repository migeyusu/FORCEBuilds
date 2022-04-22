using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.Abstraction
{
    /// <summary>
    /// 消息的客户端
    /// </summary>
    public interface IMessageClient
    {
        /// <summary>
        /// 设置对流和消息间转换的序列化器
        /// </summary>
        IFormatter Formatter { get; set; }

        IMessage GetResponse(IMessage message);

        Task<IMessage> GetResponseAsync(IMessage message, CancellationToken token);

        /// <summary>
        /// 检查是否已连接
        /// </summary>
        bool CanRequest { get; }
    }
}