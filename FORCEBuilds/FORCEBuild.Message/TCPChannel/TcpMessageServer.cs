using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Crosscutting.Log;
using FORCEBuild.Net.Abstraction;
using FORCEBuild.Net.Base;
using FORCEBuild.Net.MessageBus;
using FORCEBuild.Serialization;

namespace FORCEBuild.Net.TCPChannel
{
    //todo:test
    /// <summary>
    /// 基于TCP的消息通道
    /// </summary>
    public class TcpMessageServer : ITcpBasedMessageServer
    {
        public IPEndPoint ServiceEndPoint { get; set; }

        public IFormatter Formatter { get; set; }

        public ILog Log { get; set; }

        public IMessageProcessRoutine Routine { get; set; }

        public Guid ServiceGuid { get; set; }

        public bool IsRunning { get; private set; }


        private bool _work = true;

        public TcpMessageServer()
        {
            Formatter = new BinaryFormatter();
        }

        public void Start()
        {
            if (IsRunning)
            {
                return;
            }

            if (ServiceEndPoint == null)
            {
                throw new Exception($"null reference to {ServiceEndPoint}");
            }

            IsRunning = true;
            _work = true;
            var listener = new TcpListener(ServiceEndPoint);
            try
            {
                listener.Start();
            }
            catch (Exception)
            {
                listener.Stop();
                throw;
            }

            Task.Run(() =>
            {
                try
                {
                    while (_work)
                    {
                        if (listener.Pending())
                        {
                            var acceptSocket = listener.AcceptSocket();
                            Task.Run(() => Produce(acceptSocket));
                        }
                        else
                        {
                            Thread.Sleep(50);
                        }
                    }
                }
                finally
                {
                    listener.Stop();
                    IsRunning = false;
                }
            });
        }

        public void Stop()
        {
            if (IsRunning)
            {
                _work = false;
            }
        }

        private void Produce(Socket socket)
        {
            try
            {
                while (socket.Connected && _work)
                {
                    if (socket.Available > 0)
                    {
                        var requestHead = socket.ReadStruct<StreamMessageHeader>();
                        // DebugExtension.WriteLine($"receive request, leave len{requestHead.LeaveLength}");
                        if (!requestHead.Verify())
                        {
                            //  DebugExtension.WriteLine($"receive error,clear buffering........");
                            //清空缓冲区
                            while (socket.Available > 0)
                            {
                                socket.Receive(new byte[socket.Available]);
                            }

                            SendResponse(new NullMessage { ErrorCode = NetErrorCode.ErrorHead }, socket);
                            // DebugExtension.WriteLine($"send error yet");
                        }
                        else
                        {
                            var desalinize =
                                Formatter.Deserialize(socket.GetSpecificLenStream(requestHead.Length));
                            var message = Routine.ProducePipe.Process(desalinize as IMessage);
                            SendResponse(message, socket);
                        }
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }
            }
            catch (Exception exception)
            {
                Log?.Write(exception);
            }
            finally
            {
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
            var response = new StreamMessageHeader(dataBytes.Length);
            socket.Send(response.GetBytes());
            socket.Send(dataBytes);
        }

        public void Dispose()
        {
            _work = false;
        }
    }
}