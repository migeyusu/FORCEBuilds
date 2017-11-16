using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace FORCEBuild.Net.Base
{
    /* 2017.10.28：tcpmessagerequest不再从connection中生成，而是从socketpool生成，内部调用connection，
     * 这样做减少了调用的代码量，也允许tcpmessagerequest重复请求connection以降低取得的socket异常的可能性
     */


    /// <summary>
    /// 多终结点
    /// </summary>
    public class SocketPool : IDisposable
    {
        public bool IsClosed => _isClosed;

        private readonly ConcurrentDictionary<IPEndPoint, ConcurrentStack<Connection>> _connectionsDic;

        private bool _isClosed;

        public SocketPool()
        {
            _connectionsDic = new ConcurrentDictionary<IPEndPoint, ConcurrentStack<Connection>>();
        }

        internal void Add(Connection connection)
        {
            if (_connectionsDic.TryGetValue(connection.BindEndPoint, out ConcurrentStack<Connection> value))
            {
                value.Push(connection);
            }
            else
            {
                var connections = new ConcurrentStack<Connection>();
                connections.Push(connection);
                _connectionsDic.TryAdd(connection.BindEndPoint, connections);
            }
        }

        public Connection Open(IPEndPoint endPoint)
        {
            if (_connectionsDic.TryGetValue(endPoint, out ConcurrentStack<Connection> value))
            {
                if (value.TryPop(out Connection result)) {
                    if (result.Socket.Connected)
                    {
                        return result;
                    }
                    result.Socket.Dispose();
                }
            }
            return CreateConnection(endPoint);
        }
        
        private Connection CreateConnection(IPEndPoint endPoint)
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect(endPoint);
            return new Connection(this) {
                BindEndPoint = endPoint,
                Socket = tcpClient.Client,
            };
        }


        public void Dispose()
        {
            _isClosed = true;
            foreach (var value in _connectionsDic.Values)
            {
                foreach (var connection in value)
                {
                    connection.Socket.Dispose();
                }
            }
        }
    }

    public class Connection : IDisposable
    {
        private readonly SocketPool _pool;

        public IPEndPoint BindEndPoint { get; set; }

        public Socket Socket { get; set; }

        internal Connection(SocketPool pool)
        {
            _pool = pool;
        }

        public void Dispose()
        {
            if (_pool == null || _pool.IsClosed) {
                Socket.Dispose();
                return;
            }
            _pool.Add(this);
        }
    }
    
   
    
}