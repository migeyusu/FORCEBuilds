using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.NamedPipe
{
    // not implement dispose
    public class NamedPipeMessageClient : INamedPipeMessageClient
    {
        public IFormatter Formatter { get; set; }

        private TimeSpan _timeOut;

        public bool CanRequest { get; } = true;

        public string PipeName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pipeName"></param>
        /// <param name="timeOut"></param>
        /// <param name="formatter"></param>
        public NamedPipeMessageClient(string pipeName, TimeSpan timeOut, IFormatter formatter)
        {
            this.PipeName = pipeName;
            this.Formatter = formatter;
            this._timeOut = timeOut;
        }

        public TK GetResponse<T, TK>(T message)
            where T : IMessage
            where TK : IMessage
        {
            using (var clientStream = new NamedPipeClientStream(PipeName))
            {
                var timeout = (int)_timeOut.TotalMilliseconds;
                clientStream.Connect(timeout);
                using (var readWriter = new NamedPipeMessageFormatterAccessor(Formatter, clientStream))
                {
                    readWriter.WriteMessage(message);
                    var readMessage = readWriter.ReadMessage();
                    return (TK)readMessage;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<TK> GetResponseAsync<T, TK>(T message, CancellationToken token)
            where T : IMessage
            where TK : IMessage
        {
            using (var clientStream = new NamedPipeClientStream(PipeName))
            {
                var timeout = (int)_timeOut.TotalMilliseconds;
                await clientStream.ConnectAsync(timeout, token);
                using (var readWriter = new NamedPipeMessageFormatterAccessor(Formatter, clientStream))
                {
                    await readWriter.WriteMessageAsync(message, token);
                    var readMessageAsync = await readWriter.ReadMessageAsync(token);
                    return (TK)readMessageAsync;
                }
            }
        }
    }
}