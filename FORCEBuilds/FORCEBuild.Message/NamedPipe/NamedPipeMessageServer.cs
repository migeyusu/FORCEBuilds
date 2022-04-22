using System;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using FORCEBuild.Net.Abstraction;

namespace FORCEBuild.Net.NamedPipe
{
    public interface INamedPipeMessageServer : IMessageServer
    {
        string PipeName { get; set; }

        int MaxConnections { get; set; }
    }

    public class NamedPipeMessageServer : INamedPipeMessageServer
    {
        public IFormatter Formatter { get; set; }

        public IMessageProcessRoutine Routine { get; set; }

        public string PipeName { get; set; } = nameof(NamedPipeMessageServer);

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

        private readonly IServiceProvider _provider;

        private readonly ConcurrentDictionary<Guid, NamedPipeServerEntry> _subscribers =
            new ConcurrentDictionary<Guid, NamedPipeServerEntry>();

        private readonly ConcurrentDictionary<Guid, NamedPipeServerEntry>
            _listeners = new ConcurrentDictionary<Guid, NamedPipeServerEntry>();

        private int _maxConnections = 10;

        public NamedPipeMessageServer(IServiceProvider provider)
        {
            this._provider = provider;
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
                new NamedPipeServerEntry(PipeName, _provider, Formatter, Routine, this._maxConnections);
            namedPipeServerEntry.Connected += NamedPipeServerEntryOnConnected;
            namedPipeServerEntry.Disconnected += NamedPipeServerEntryOnDisconnected;
            namedPipeServerEntry.CancelConnected += NamedPipeServerEntryOnCancelConnected;
            _listeners.TryAdd(namedPipeServerEntry.Id, namedPipeServerEntry);
            namedPipeServerEntry.Start();
        }

        private void NamedPipeServerEntryOnCancelConnected(object sender, EventArgs e)
        {
            _subscribers.TryRemove(((NamedPipeServerEntry)sender).Id, out var value);
        }

        private void NamedPipeServerEntryOnDisconnected(object sender, Exception arg2)
        {
            _subscribers.TryRemove(((NamedPipeServerEntry)sender).Id, out var value);
        }

        private void NamedPipeServerEntryOnConnected(object sender, EventArgs eventArgs)
        {
            var connectedEntry = sender as NamedPipeServerEntry;
            _listeners.TryRemove(connectedEntry.Id, out var entry);
            CreateNewEntry();
            _subscribers.TryAdd(connectedEntry.Id, connectedEntry);
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