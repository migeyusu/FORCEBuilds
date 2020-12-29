using FORCEBuild.Net.Base;
using FORCEBuild.Net.RPC;

namespace FORCEBuild.Net.Remote
{
    /// <summary>
    /// 作为messagebusservice的contract
    /// </summary>
    [RemoteInterface]
    public interface ITcpMessageCentralizedService
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="topic">邮箱名</param>
        void SendMessage<T>(T x,string topic) where T : IMessage;

        /// <summary>
        /// 注册邮箱，可重复注册。重复注册会修改<param name="messageRouteStrategy" />,但不会修改<param name="consumerStrategy" />
        /// </summary>
        /// <param name="messageRouteStrategy"></param>
        /// <param name="consumerStrategy"></param>
        /// <param name="name">消息邮箱名称</param>
        void RegisterOrUpdateMail(MessageRouteStrategy messageRouteStrategy,
            ConsumerStrategy consumerStrategy, string name);

        /// <summary>
        /// 是否存在名为<paramref name="name"/>的邮箱名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool IsMailName(string name);

        void WrittenOffMail(string name);

    }
}