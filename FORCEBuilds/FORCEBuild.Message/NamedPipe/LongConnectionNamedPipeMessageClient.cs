using System;
using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Net.Base;
using Microsoft.Extensions.Logging;

namespace FORCEBuild.Net.NamedPipe
{
    /// <summary>
    /// 支持长连接的命名管道消息客户端
    /// </summary>
    public class LongConnectionNamedPipeMessageClient : INamedPipeMessageClient, IDisposable
    {
        public IFormatter Formatter { get; set; }

        public bool CanRequest { get; } = true;

        public string PipeName { get; set; }

        private readonly int _timeOut;

        private ILogger<LongConnectionNamedPipeMessageClient> _logger;

        private readonly NamedPipeClientStream _namedPipeClientStream;

        private readonly NamedPipeMessageFormatterAccessor _accessor;

        public LongConnectionNamedPipeMessageClient(string pipeName, TimeSpan timeOut, IFormatter formatter,
            ILogger<LongConnectionNamedPipeMessageClient> logger)
        {
            PipeName = pipeName;
            Formatter = formatter;
            this._logger = logger;
            this._timeOut = (int)timeOut.TotalMilliseconds;
            this._namedPipeClientStream = new NamedPipeClientStream(pipeName);
            this._accessor = new NamedPipeMessageFormatterAccessor(formatter, _namedPipeClientStream);
        }

        public TK GetResponse<T, TK>(T message) where T : IMessage where TK : IMessage
        {
            using (_logger?.BeginScope(Guid.NewGuid()))
            {
                if (!_namedPipeClientStream.IsConnected)
                {
                    _logger?.LogInformation("Begin to connect.");
                    _namedPipeClientStream.Connect(_timeOut);
                    _logger?.LogInformation("Connected.");
                }

                _accessor.WriteMessage(message);
                _logger?.LogInformation("Message sent.");
                var readMessage = _accessor.ReadMessage();
                _logger?.LogInformation("Message received.");
                return (TK)readMessage;
            }
        }

        public async Task<TK> GetResponseAsync<T, TK>(T message, CancellationToken token)
            where T : IMessage where TK : IMessage
        {
            using (_logger?.BeginScope(Guid.NewGuid()))
            {
                if (!_namedPipeClientStream.IsConnected)
                {
                    _logger?.LogInformation("Begin to connect.");
                    await _namedPipeClientStream.ConnectAsync(_timeOut, token);
                    _logger?.LogInformation($"Pipe connected.");
                }

                await _accessor.WriteMessageAsync(message, token);
                _logger?.LogInformation($"Message sent.");
                var readMessageAsync = await _accessor.ReadMessageAsync(token);
                _logger?.LogInformation("Message received.");
                return (TK)readMessageAsync;
            }
        }

        public void Dispose()
        {
            _namedPipeClientStream?.Dispose();
        }
    }
}