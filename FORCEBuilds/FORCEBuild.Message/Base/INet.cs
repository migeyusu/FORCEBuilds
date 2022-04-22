using System;
using System.Net;
using System.Net.Sockets;

namespace FORCEBuild.Net.Base
{
    public interface INetListener
    {
        event Action<UdpReceiveResult> OnReceive;
        event Action OnEndListen;
        int Interval { get; set; }
        void StartListen();
        void StartListen(IPEndPoint local);
        void EndListen();
    }
    public interface INetSender
    {
        void Send(IPEndPoint remote, byte[] bytes);
    }

    public interface INetCommunication : INetSender, INetListener
    {

    }
}
