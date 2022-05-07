using System;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using FORCEBuild.Net.Abstraction;
using FORCEBuild.Net.Base;
using Microsoft.Extensions.Logging;

namespace FORCEBuild.Net.NamedPipe
{
    public class NamedPipeMessageServer : INamedPipeMessageServer, IMessageProcessRoutine
    {
        public ILogger<NamedPipeMessageServer> Logger { get; set; }

        public IFormatter Formatter { get; set; }

        public IMessageProcessRoutine Routine => this;

        public MessagePipe<IMessage, IMessage> ProducePipe { get; set; }

        public string PipeName { get; set; }

        public int MaxConnections
        {
            get => _maxConnections;
            set
            {
                if (value < 1)
                {
                    throw new NotSupportedException();
                }

                _maxConnections = value;
            }
        }

        public Guid ServiceGuid { get; } = Guid.NewGuid();

        public bool IsRunning { get; private set; }

        private bool _disposed = false;

        private volatile bool _isListening;

        private readonly ConcurrentDictionary<Guid, NamedPipeServerEntry> _subscribers =
            new ConcurrentDictionary<Guid, NamedPipeServerEntry>();

        private readonly ConcurrentDictionary<Guid, NamedPipeServerEntry>
            _listeners = new ConcurrentDictionary<Guid, NamedPipeServerEntry>();

        private int _maxConnections = 10;

        public NamedPipeMessageServer(string pipeName, IFormatter formatter, MessagePipe<IMessage, IMessage> pipe,
            int maxConnections) : this(pipeName, formatter, maxConnections)
        {
            this.ProducePipe = pipe;
        }

        public NamedPipeMessageServer(string pipeName, IFormatter formatter, int maxConnections)
        {
            PipeName = pipeName;
            Formatter = formatter;
            if (maxConnections < 2)
            {
                throw new ArgumentNullException(nameof(maxConnections), "Can't less than 2.");
            }

            this.MaxConnections = maxConnections;
            // this.ProducePipe = new EmptyPipe<IMessage, IMessage>();
        }

        public void Start()
        {
            _isListening = true;
            for (int i = 0; i < _maxConnections; i++)
            {
                CreateNewEntry();
            }
        }

        public void Stop()
        {
            _isListening = false;
            foreach (var namedPipeEntry in _listeners.Values)
            {
                namedPipeEntry.Dispose();
            }

            foreach (var subscriber in _subscribers.Values)
            {
                subscriber.Dispose();
            }

            _disposed = true;
        }

        private void CreateNewEntry()
        {
            if (!_isListening)
            {
                return;
            }

            var namedPipeServerEntry =
                new NamedPipeServerEntry(PipeName, Formatter, Routine, this._maxConnections);
            namedPipeServerEntry.Connected += NamedPipeServerEntryOnConnected;
            namedPipeServerEntry.Disconnected += NamedPipeServerEntryOnDisconnected;
            namedPipeServerEntry.CancelConnected += NamedPipeServerEntryOnCancelConnected;
            var id = namedPipeServerEntry.Id;
            _listeners.TryAdd(id, namedPipeServerEntry);
            Logger?.LogInformation(
                $"New entry {id} created, current listeners count {_listeners.Count}.");
            namedPipeServerEntry.Start();
        }

        private void NamedPipeServerEntryOnCancelConnected(object sender, EventArgs e)
        {
            var id = ((NamedPipeServerEntry)sender).Id;
            _listeners.TryRemove(id, out var value);
            Logger.LogWarning($"Entry {id} cancel connected. Current subscribers count {_subscribers.Count}.");
        }

        private void NamedPipeServerEntryOnDisconnected(object sender, Exception arg2)
        {
            var id = ((NamedPipeServerEntry)sender).Id;
            _subscribers.TryRemove(id, out var value);
            if (arg2 != null)
            {
                Logger?.LogError(arg2, $"Entry {id} disconnected with exception {arg2}.");
            }
            else
            {
                Logger.LogWarning($"Entry {id} disconnected");
            }

            Logger?.LogInformation($"Current subscribers count {_subscribers.Count}.");
        }

        private void NamedPipeServerEntryOnConnected(object sender, EventArgs eventArgs)
        {
            var connectedEntry = sender as NamedPipeServerEntry;
            var entryId = connectedEntry.Id;
            _listeners.TryRemove(entryId, out var entry);
            _subscribers.TryAdd(entryId, connectedEntry);
            Logger?.LogInformation(
                $"Entry {entryId} connected, current listeners count {_listeners.Count}, current subscribers count {_subscribers.Count}.");
            CreateNewEntry();
        }


        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            this.Stop();
        }
    }
}