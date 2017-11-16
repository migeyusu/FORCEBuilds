using System.Net;
using FORCEBuild.Message.Base;

namespace FORCEBuild.Message.Remote
{
    public interface ITcpMessageMail<T>:IMessageMail<T> where T:IMessage 
    {
        /// <summary>
        /// 查询端口
        /// </summary>
        IPEndPoint EndPoint { get; set; }
    }
}