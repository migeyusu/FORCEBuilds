using System;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Net.Abstraction;
using FORCEBuild.Net.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FORCEBuild.Net.NamedPipe
{
    public class NamedPipeServerEntry : IDisposable
    {
        public ILogger Logger { get; set; }

        public Guid Id { get; }

        public event EventHandler Connected;

        public event EventHandler<Exception> Disconnected;

        public event EventHandler CancelConnected;

        private readonly string _pipeName;

        private bool _connected = false;

        private bool _disposed = false;

        private readonly CancellationTokenSource _waitTokenSource;

        private readonly IFormatter _formatter;

        private readonly IMessageProcessRoutine _routine;

        private readonly int _maxCount;

        public NamedPipeServerEntry(string pipeName, IFormatter formatter,
            IMessageProcessRoutine routine, int maxCount)
        {
            _pipeName = pipeName;
            _formatter = formatter;
            this._routine = routine;
            this._maxCount = maxCount;
            Id = Guid.NewGuid();
            _waitTokenSource = new CancellationTokenSource();
        }

        public Task Start()
        {
            if (_connected)
            {
                return Task.CompletedTask;
            }

            var token = _waitTokenSource.Token;
            return Task.Run((async () =>
            {
                using (var namedPipeServerStream = new NamedPipeServerStream(_pipeName, PipeDirection.InOut,
                           _maxCount, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                {
                    try
                    {
                        Logger?.LogInformation($"Entry {Id} started.");
                        await namedPipeServerStream.WaitForConnectionAsync(token);
                    }
                    catch (OperationCanceledException)
                    {
                        OnCancelConnected();
                        return;
                    }
                    catch (Exception exception)
                    {
                        OnDisconnected(exception);
                    }

                    try
                    {
                        _connected = true;
                        OnConnected();
                        using (var messageReadWriter =
                               new NamedPipeMessageFormatterAccessor(_formatter, namedPipeServerStream, false))
                        {
                            var message = await messageReadWriter.ReadMessageAsync(token);
                            var processedMessage = this._routine.ProducePipe.Process(message);
                            await messageReadWriter.WriteMessageAsync(processedMessage, token);
                        }
                    }
                    catch (Exception exception)
                    {
                        OnDisconnected(exception);
                    }
                    finally
                    {
                        _connected = false;
                        if (namedPipeServerStream.IsConnected)
                        {
                            namedPipeServerStream.Disconnect();
                        }

                        OnDisconnected(null);
                        namedPipeServerStream.Close();
                    }
                }
            }), token);
        }

        public void Close()
        {
            _waitTokenSource.Cancel();
            _waitTokenSource.Dispose();
            _disposed = true;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Close();
        }

        protected virtual void OnConnected()
        {
            Connected?.Invoke(this, null);
        }

        protected virtual void OnDisconnected(Exception exception)
        {
            Disconnected?.Invoke(this, exception);
        }

        protected virtual void OnCancelConnected()
        {
            CancelConnected?.Invoke(this, null);
        }
    }
}