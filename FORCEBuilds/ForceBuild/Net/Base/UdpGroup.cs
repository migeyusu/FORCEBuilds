using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FORCEBuild.Net.Base
{
    public class UdpGroup:INetListenr,INetSender
    {
        UdpClient _listener;
        readonly UdpClient _sender;
        bool _listening = false;
        IPEndPoint _listenPc;
        bool _sending = false, _broadcasting = false;
        byte[] _broadcastdata;
        readonly object _dataLocker;
        public byte[] Broadcastdata
        {
            set
            {
                lock (_dataLocker)
                {
                    _broadcastdata = value;
                }
            }
            get
            {
                return _broadcastdata;
            }
        }
        public event Action OnEndSend;
        public event Action OnEndListen;
        public event Action<UdpReceiveResult> OnReceive;
        public int Interval { get; set; }
        public Encoding StringEncoder { get; set; }
        public UdpGroup()
        {
            _dataLocker = new object();
            _sender = new UdpClient(NetHelper.InstanceEndPoint);
            Interval = 100;
            _sending = false;
        }
        //public UdpGroup(IPEndPoint local)
        //{
        //    _listener = new UdpClient(local);
        //    _sender=new UdpClient(NetHelper.AviliblePort);
        //    Interval = 100;
        //    _sending = false;
        //    _dataLocker = new object();
        //}
        public void StartListen()
        {
            try
            {
                if (_listening)
                    return;
                _listening = true;
                _listener = new UdpClient(_listenPc);
                Task.Run(new Action(ListenThread));
            }
            catch (Exception)
            {
                _listening = false;
                throw;
            }
        }
        public void StartListen(IPEndPoint localPc)
        {
            try
            {
                if (_listening)
                    return;
                _listenPc = localPc;
                _listening = true;
                _listener = new UdpClient(localPc);
                Task.Run(() => ListenThread());
            }
            catch (Exception)
            {
                _listening = false;
                throw;
            }
        }
        public void EndListen()
        {
            if (!_listening)
                return;
            _listening = false;
        }

        private void ListenThread()
        {
            IPEndPoint remoted = null;
            try
            {
                while (_listening)
                {
                    if (_listener.Available > 0)
                    {
                        var bytes = _listener.Receive(ref remoted);
                        OnReceive?.Invoke(new UdpReceiveResult(bytes,remoted));
                    }
                    else
                    {
                        Thread.Sleep(Interval);
                    }
                }
            }
            finally
            {
                _listener.Close();
                _listening = false;
                OnEndListen?.Invoke();
            }
        }
        /// <summary>
        /// 每次使用固定端口
        /// </summary>
        /// <param name="remote"></param>
        /// <param name="datas"></param>
        public void Send(IPEndPoint remote, string datas)
        {
            var data = StringEncoder.GetBytes(datas);
            _sender.Send(data, data.Length, remote);
        }
        /// <summary>
        /// 每次使用固定端口
        /// </summary>
        /// <param name="remote"></param>
        /// <param name="datas"></param>
        public void Send(IPEndPoint remote, byte[] datas)
        {
            _sender.Send(datas, datas.Length, remote);
        }
        public void StartLoopSend(IPEndPoint remote, byte[] datas, int sendintval)
        {
            if (_sending)
                return;
            _sending = true;
            Task.Run(() =>
            {
                var uc = new UdpClient();
                try
                {
                    while (_sending)
                        uc.Send(datas, datas.Length, remote);
                }
                finally
                {
                    uc.Close();
                    _sending = false;
                    OnEndSend?.Invoke();
                }
            });

        }
        public void StartLoopSend(IPEndPoint remote, byte[] datas, int sendintval, IPEndPoint local)
        {
            if (_sending)
                return;
            _sending = true;
            Task.Run(() =>
            {
                var uc = new UdpClient(local);
                try
                {
                    while (_sending)
                        uc.Send(datas, datas.Length, remote);
                }
                finally
                {
                    uc.Close();
                    _sending = false;
                    OnEndSend?.Invoke();
                }
            });

        }
        public void EndLoopSend()
        {
            _sending = false;
        }
        public void StartBroadcast(byte[] datas, int interval, int port)
        {
            if (_broadcasting)
                return;
            _broadcasting = true;
            Broadcastdata = datas;
            var target = new IPEndPoint(IPAddress.Broadcast, port);
            Task.Run(() =>
            {
                var uc = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
                while (_broadcasting)
                {
                    lock (_dataLocker)
                    {
                        uc.Send(Broadcastdata, Broadcastdata.Length, target);
                    }
                    Thread.Sleep(interval);
                }
            });
        }
        public void StartBroadcast(int interval, int port)
        {
            if (_broadcasting)
                return;
            _broadcasting = true;
            var target = new IPEndPoint(IPAddress.Broadcast, port);
            Task.Run(() =>
            {
                var uc = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
                while (_broadcasting)
                {
                    lock (_dataLocker)
                    {
                        uc.Send(Broadcastdata, Broadcastdata.Length, target);
                    }
                    Thread.Sleep(interval);
                }
            });
        }
        public void EndBroadcast()
        {
            _broadcasting = false;
        }
        ~UdpGroup()
        {
            _sender.Close();
            _sending = false;
            _listening = false;
        }
    }
}
