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

        public IMessage GetResponse(IMessage message)
        {
            Task<IMessage> task = Task.Run((async () => await this.GetResponseAsync(message, CancellationToken.None)));
            return task.Result;
        }

        public bool CanRequest { get; } = true;

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

        public string PipeName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IMessage> GetResponseAsync(IMessage message, CancellationToken token)
        {
            using (var clientStream = new NamedPipeClientStream(PipeName))
            {
                var timeout = (int)_timeOut.TotalMilliseconds;
                await clientStream.ConnectAsync(timeout, token);
                using (var readWriter = new NamedPipeMessageFormatterReadWriter(Formatter, clientStream))
                {
                    await readWriter.WriteMessageAsync(message, token);
                    var readMessageAsync = await readWriter.ReadMessageAsync(token);
                    return readMessageAsync;
                }
            }
        }
    }
}