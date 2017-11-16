using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Message.Remote
{
    /// <summary>
    /// 远程对象通信
    /// </summary>
    public class RemotingCommunication : ICommunication
    {
        private readonly Dictionary<Type, Action<INetCommand>> _executeCache;
        private readonly Queue<byte[]> _instructions;
        private INetCommunication _communicator;
        private readonly object _locker = new object();

        /// <summary>
        /// 是否工作
        /// </summary>
        private bool _working;

        public INetCommunication Communicator {
            get => _communicator;
            set {
                _communicator = value;
                _communicator.OnReceive += Communicator_OnDataReceive;
                _communicator.OnEndListen += () => _working = false;
                
            }
        }

        public IFormatter Formatter { get; set; }

        private void Communicator_OnDataReceive(UdpReceiveResult urReceiveResult)
        {
            lock (_locker) {
                _instructions.Enqueue(urReceiveResult.Buffer);
            }
        }

        public IPEndPoint Local { get; set; }
        public bool Running { get; set; }

        public RemotingCommunication()
        {
            _executeCache = new Dictionary<Type, Action<INetCommand>>();
            _instructions = new Queue<byte[]>();
        }

        public void StartListen()
        {
            _working = true;
            Communicator.StartListen(Local);
            Task.Run(new Action(StreamDecode));
        }

        public void EndListen()
        {
            Communicator.EndListen();
        }

        public void Send(INetCommand cmd, IPEndPoint remote)
        {
            var ms = new MemoryStream();
            Formatter.Serialize(ms, cmd);
            Communicator.Send(remote, ms.ToArray());
            ms.Close();
        }

        public void Register(INetCommand cmd, Action<INetCommand> actionCallBack)
        {
            if (!_executeCache.Keys.Contains(cmd.GetType()))
                _executeCache.Add(cmd.GetType(), actionCallBack);
        }

        private void StreamDecode()
        {
            Running = true;
            while (_working) {
                byte[] buffer;
                if (_instructions.Count > 0) {
                    lock (_locker) {
                        buffer = _instructions.Dequeue();
                    }
                }
                else {
                    Thread.Sleep(100);
                    continue;
                }
                var request = Formatter.Deserialize(new MemoryStream(buffer));
                if (_executeCache.ContainsKey(request.GetType())) {
                    _executeCache[request.GetType()]((INetCommand) request);
                }
            }
            Running = false;
            
        }
    }

}
