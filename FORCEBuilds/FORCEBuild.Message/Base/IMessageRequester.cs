using System.Runtime.Serialization;

namespace FORCEBuild.Net.Base
{
    public interface IMessageRequester
    {
        /// <summary>
        /// 设置对流和消息间转换的序列化器
        /// </summary>
        IFormatter Formatter { get; set; }

        IMessage GetResponse(IMessage message);

        bool CanRequest { get;}
    }
}