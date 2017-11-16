#define factorymethod

using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FORCEBuild.Crosscutting.Log;
using FORCEBuild.Message.Remote;
using FORCEBuild.Message.Remote.DataTransferObject;
using FORCEBuild.Net.Base;
using FORCEBuild.Net.Service;
using FORCEBuild.Persistence.Serialization;

namespace FORCEBuild.Message.Base
{
    public class TcpMessageRequester:IMessageRequester,ITcpServiceCustomer
    {
        public IFormatter Formatter { get; set; }

        /// <summary>
        /// 在外部释放connection.socket
        /// </summary>
        private readonly SocketPool _socketPool;

        /// <summary>   
        /// 重试次数
        /// </summary>
        public int TryTimes { get; set; } = 3;

        internal TcpMessageRequester(SocketPool socketPool, IPEndPoint endPoint)
        {
            this._socketPool = socketPool;
            this.RemoteChannel = endPoint;
            Formatter = new BinaryFormatter();
        } 
         
        /// <exception cref="Exception">消息不能为空</exception>
        public IMessage GetResponse(IMessage x)
        {
            if (!CanRequest) {
                throw new Exception("服务不可用");
            }
            if (x == null) {
                throw new Exception("消息不能为空");
            }
            IMessage message = null;
            var memoryStream = new MemoryStream();
            Formatter.Serialize(memoryStream, x);
            var dataBytes = memoryStream.ToArray();
            var headBytes = new RequestHead(dataBytes.Length).ToBytes();
            for (var i = 0; i < TryTimes; i++)
            {
                using (var connection = _socketPool.Open(RemoteChannel)) {
                    var socket = connection.Socket; 
                    socket.Send(headBytes);
                    socket.Send(dataBytes);
                    //DebugExtension.WriteLine("send request yet");
                    ResponseHead responseHead;
                    if ((responseHead = socket.GetStruct<ResponseHead>()).IsCorrect)
                    {
                        //  DebugExtension.WriteLine("get response correct");
                        var receiveStream = socket.GetSpecificLenStream(responseHead.LeaveLength);
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
            if (nullMessage != null) {
                Log.Write($"Response Error,Error Code:{nullMessage.ErrorCode}");
                throw new MessageTransferException {NetErrorCode = nullMessage.ErrorCode};
            }
            return message;
        }

        public bool CanRequest => RemoteChannel != null;

        public IPEndPoint RemoteChannel { get; set; }

        public ILog Log { get; set; }
    }
}
