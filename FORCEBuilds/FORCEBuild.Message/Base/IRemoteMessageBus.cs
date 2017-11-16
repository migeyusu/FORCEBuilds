using FORCEBuild.Message.Remote;

namespace FORCEBuild.Message.Base
{
    public interface IRemoteMessageBus
    {


        /// <summary>
        /// 订阅的邮箱，仅用于远程
        /// </summary>
        string MailName { get; set; }

        MessageMailStrategy Strategy { get; set; }

        bool Working { get; }

        void Start();

        void End();
    }
}