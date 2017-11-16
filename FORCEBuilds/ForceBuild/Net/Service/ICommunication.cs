using System;
using System.Net;
using System.Runtime.Serialization;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Message.Remote
{
    /// <summary>
    /// 轻型对象通信框架
    /// </summary>
    public interface ICommunication
    {
        INetCommunication Communicator { get; set; }
        IFormatter Formatter { get; set; }
        IPEndPoint Local { get; set; }
        bool Running { get; set; }
        void StartListen(); 
        void EndListen();
        void Send(INetCommand cmd,IPEndPoint remote);
        void Register(INetCommand cmd, Action<INetCommand> actionCallBack);
    }
    /// <summary>
    /// 远程通信命令
    /// </summary>
    public interface INetCommand
    {

    }
}
