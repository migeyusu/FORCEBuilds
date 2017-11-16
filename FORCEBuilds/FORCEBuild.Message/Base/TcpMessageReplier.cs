using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Crosscutting.Log;
using FORCEBuild.Message.Remote;
using FORCEBuild.Net.Base;
using FORCEBuild.Persistence.Serialization;

namespace FORCEBuild.Message.Base
{
    public class TcpMessageReplier:IDisposable,IMessageReplier
    {
        public IPEndPoint EndPoint { get; set; }

        public IFormatter Formatter { get; set; }

        public ILog Log { get; set; }

        public bool Working { get; private set; }

        public MessagePipe<IMessage,IMessage> ProducePipe { get; set; }
            
        private bool _work = true;  

        public TcpMessageReplier()
        {
            Formatter = new BinaryFormatter();
        }

        public void Start() 
        {
            if (Working) {
                return;
            }
            if (EndPoint == null) {
                throw new Exception($"null reference to {EndPoint}");
            }
            Working = true;
            _work = true;
            var listener = new TcpListener(EndPoint);
            try {
                listener.Start();
            }
            catch (Exception) {
                listener.Stop();
                throw;
            }
            Task.Run(() => {
                try {
                    while (_work) {
                        if (listener.Pending()) {
                            var acceptSocket = listener.AcceptSocket();
                            Task.Run(() => Produce(acceptSocket));
                        }
                        else {
                            Thread.Sleep(50);
                        }
                    }
                }
                finally {
                    listener.Stop();
                    Working = false;
                }
            });
        }

        public void End()
        {
            if (Working) {
                _work = false;
            }
        }

        private void Produce(Socket socket)
        {
            try {
                while (socket.Connected && _work) {
                    if (socket.Available > 0) {
                        var requestHead = socket.GetStruct<RequestHead>();
                        //DebugExtension.WriteLine($"receive request, leave len{requestHead.LeaveLength}");
                        if (!requestHead.IsCorrect) {
                          //  DebugExtension.WriteLine($"receive error,clear buffering........");
                            //清空缓冲区
                            while (socket.Available > 0) {
                                socket.Receive(new byte[socket.Available]);
                            }
                            SendResponse(new NullMessage {ErrorCode = NetErrorCode.ErrorHead}, socket);
                           // DebugExtension.WriteLine($"send error yet");
                        }
                        else {
                            var desalinize =
                                Formatter.Deserialize(socket.GetSpecificLenStream(requestHead.LeaveLength));
                            var message = ProducePipe.Process(desalinize as IMessage);
                            SendResponse(message, socket);
                        }
                    }
                    else {
                        Thread.Sleep(50);
                    }
                }
            }
            catch (Exception exception) {
                Log?.Write(exception);
            }
            finally {
               // DebugExtension.WriteLine("socket exit");
                socket.Close();
                socket.Dispose();
            }
            
        }
        
        private void SendResponse(object dto, Socket socket)
        {
            var memoryStream = new MemoryStream();
            Formatter.Serialize(memoryStream, dto);
            var dataBytes = memoryStream.ToArray();
            var response = new ResponseHead(dataBytes.Length);
            socket.Send(response.ToBytes());
            socket.Send(dataBytes);
        }

        public void Dispose()
        {
            _work = false;
            ProducePipe?.Dispose();
        }
    }   
}