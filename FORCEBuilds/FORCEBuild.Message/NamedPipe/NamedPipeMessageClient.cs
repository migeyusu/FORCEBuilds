using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Net.Base;
using Microsoft.Extensions.Logging;

namespace FORCEBuild.Net.NamedPipe
{
    // not implement dispose
    public class NamedPipeMessageClient : INamedPipeMessageClient
    {
        public IFormatter Formatter { get; set; }

        public bool CanRequest { get; } = true;

        public string PipeName { get; set; }

        private readonly ILogger<NamedPipeMessageClient> _logger;

        private readonly int _timeout;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pipeName"></param>
        /// <param name="timeOut"></param>
        /// <param name="formatter"></param>
        /// <param name="logger"></param>
        public NamedPipeMessageClient(string pipeName, TimeSpan timeOut, IFormatter formatter,
            ILogger<NamedPipeMessageClient> logger = null)
        {
            this.PipeName = pipeName;
            this.Formatter = formatter;
            this._logger = logger;
            this._timeout = (int)timeOut.TotalMilliseconds;
        }

        public TK GetResponse<T, TK>(T message)
            where T : IMessage
            where TK : IMessage
        {
            using (_logger?.BeginScope(Guid.NewGuid()))
            {
                using (var clientStream = new NamedPipeClientStream(PipeName))
                {
                    clientStream.Connect(_timeout);
                    _logger?.LogInformation($"Pipe connected.");
                    var readWriter = new NamedPipeMessageFormatterAccessor(Formatter, clientStream);
                    readWriter.WriteMessage(message);
                    _logger?.LogInformation($"Message sent.");
                    var readMessage = readWriter.ReadMessage();
                    _logger?.LogInformation("Message received.");
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
            using (_logger?.BeginScope(Guid.NewGuid()))
            {
                using (var clientStream = new NamedPipeClientStream(PipeName))
                {
                    await clientStream.ConnectAsync(_timeout, token);
                    _logger?.LogInformation($"Pipe connected.");
                    var readWriter = new NamedPipeMessageFormatterAccessor(Formatter, clientStream);
                    await readWriter.WriteMessageAsync(message, token);
                    _logger?.LogInformation($"Message sent.");
                    var readMessageAsync = await readWriter.ReadMessageAsync(token);
                    _logger?.LogInformation("Message received.");
                    return (TK)readMessageAsync;
                }
            }
        }
    }
}