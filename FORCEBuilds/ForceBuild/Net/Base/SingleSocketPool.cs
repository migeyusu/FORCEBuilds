using System;
using System.Collections.Concurrent;
using System.Net;

namespace FORCEBuild.Net.Base
{
    public class SingleSocketPool : IDisposable
    {
        internal bool IsClosed { get; set; }

        public IPEndPoint EndPoint {
            get { return _endPoint; }
            set {
                if (_endPoint!=null&&!IsClosed) {
                    throw new Exception("");
                }
                _endPoint = value; 
            }
        }

        private readonly ConcurrentStack<Connection> _connections;

        private IPEndPoint _endPoint;

        public SingleSocketPool()
        {
            _connections = new ConcurrentStack<Connection>();
        }

        internal void Add(Connection connection)
        {
            _connections.Push(connection);
        }

        public Connection Open()
        {
            if (_connections.TryPop(out Connection result)) {
                if (result.Socket.Connected)
                    return result;
                result.Socket.Dispose();
            }
            var connection = Connection.Create(_endPoint);
            connection.Pool = this;
            return connection;
        }

        public void Dispose()
        {
            IsClosed = true;
            foreach (var connection in _connections)
                connection.Socket.Dispose();
        }
    }
}