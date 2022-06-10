#define factorymethod

using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Crosscutting.Log;
using FORCEBuild.Net.Base;
using FORCEBuild.Net.MessageBus;
using FORCEBuild.Net.MessageBus.DataTransferObject;
using FORCEBuild.Serialization;
using Microsoft.Extensions.Logging;

namespace FORCEBuild.Net.TCPChannel
{
    public class TcpMessageClient : ITcpBasedMessageClient
    {
        public IFormatter Formatter { get; set; }

        /// <summary>
        /// 在外部释放connection.socket
        /// </summary>
        private readonly ConnectionPool _socketPool;

        /// <summary>   
        /// 重试次数
        /// </summary>
        public int TryTimes { get; set; } = 3;

        internal TcpMessageClient(ConnectionPool socketPool, IPEndPoint endPoint)
        {
            this._socketPool = socketPool;
            this.RemoteChannel = endPoint;
            Formatter = new BinaryFormatter();
        }

        /// <exception cref="Exception">消息不能为空</exception>
        public IMessage GetResponse(IMessage x)
        {
            if (!CanRequest)
            {
                throw new Exception("服务不可用");
            }

            if (x == null)
            {
                throw new Exception("消息不能为空");
            }

            IMessage message = null;
            var memoryStream = new MemoryStream();
            Formatter.Serialize(memoryStream, x);
            var dataBytes = memoryStream.ToArray();
            var headBytes = new StreamMessageHeader(dataBytes.Length).GetBytes();
            for (var i = 0; i < TryTimes; i++)
            {
                using (var connection = _socketPool.Open(RemoteChannel))
                {
                    var socket = connection.Socket;
                    socket.Send(headBytes);
                    socket.Send(dataBytes);
                    //DebugExtension.WriteLine("send request yet");
                    StreamMessageHeader responseHead;
                    if ((responseHead = socket.ReadStruct<StreamMessageHeader>()).Verify())
                    {
                        //  DebugExtension.WriteLine("get response correct");
                        var receiveStream = socket.GetSpecificLenStream(responseHead.Length);
                        message = Formatter.Deserialize(receiveStream) as IMessage;
                        if (message != null && message.GetType() != typeof(NullMessage))
                            break;
                        //   DebugExtension.WriteLine($"get null-message,ERROR-CODE:{((NullMessage) message).ErrorCode}");
                    }

                    //DebugExtension.WriteLine("error,clear buffering........");
                    //清空缓冲区，准备下次重试
                    while (socket.Available > 0)
                        socket.Receive(new byte[socket.Available]);
                }
            }

            var nullMessage = message as NullMessage;
            if (nullMessage != null)
            {
                var messageTransferException = new MessageTransferException { NetErrorCode = nullMessage.ErrorCode };
                Log?.LogError(messageTransferException, $"Response Error,Error Code:{nullMessage.ErrorCode}");
                throw messageTransferException;
            }

            return message;
        }

        public Task<IMessage> GetResponseAsync(IMessage message, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public TK GetResponse<T, TK>(T message) where T : IMessage where TK : IMessage
        {
            throw new NotImplementedException();
        }

        public Task<TK> GetResponseAsync<T, TK>(T message, CancellationToken token)
            where T : IMessage where TK : IMessage
        {
            throw new NotImplementedException();
        }

        public bool CanRequest => RemoteChannel != null;

        public IPEndPoint RemoteChannel { get; set; }

        public ILogger<TcpMessageClient> Log { get; set; }
    }
}