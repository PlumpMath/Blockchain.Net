namespace BlockchainNet.Pipe.Server
{
    using System;
    using System.Threading;
    using System.ComponentModel;
    using System.Collections.Concurrent;

    using BlockchainNet.Core.Communication;

    public class PipeServer<T> : ICommunicationServer<T>
    {
        public const int BufferSize = 2048;
        public const int MaxNumberOfServerInstances = 100;
        
        private readonly SynchronizationContext _synchronizationContext;
        private readonly ConcurrentDictionary<string, ICommunicationServer<T>> _servers;
        
        public PipeServer(string id)
        {
            ServerId = id;
            _synchronizationContext = AsyncOperationManager.SynchronizationContext;
            _servers = new ConcurrentDictionary<string, ICommunicationServer<T>>();
        }

        public PipeServer() : this(Guid.NewGuid().ToString())
        {
        }
        
        public event EventHandler<MessageReceivedEventArgs<T>> MessageReceivedEvent;
        public event EventHandler<ClientConnectedEventArgs> ClientConnectedEvent;
        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnectedEvent;
        
        public string ServerId { get; }

        public void Start()
        {
            StartNamedPipeServer();
        }

        public void Stop()
        {
            foreach (var server in _servers.Values)
            {
                try
                {
                    UnregisterFromServerEvents(server);
                    server.Stop();
                }
                catch (Exception)
                {
                }
            }

            _servers.Clear();
        }
        
        private void StartNamedPipeServer()
        {
            // Есть смысл создавать свой InternalPipeServer для каждого клиента, но при этом основываясь на одном его имени
            // Это позволяет независимо читать данные с множества клиентов
            var server = new InternalPipeServer<T>(ServerId, MaxNumberOfServerInstances);

            server.ClientConnectedEvent += ClientConnectedHandler;
            server.ClientDisconnectedEvent += ClientDisconnectedHandler;
            server.MessageReceivedEvent += MessageReceivedHandler;

            _servers[server.ServerId] = server;

            server.Start();
        }
        
        private void StopNamedPipeServer(string id)
        {
            if (_servers.TryRemove(id, out ICommunicationServer<T> removed))
            {
                UnregisterFromServerEvents(removed);
                removed.Stop();
            }
        }
        
        private void UnregisterFromServerEvents(ICommunicationServer<T> server)
        {
            server.ClientConnectedEvent -= ClientConnectedHandler;
            server.ClientDisconnectedEvent -= ClientDisconnectedHandler;
            server.MessageReceivedEvent -= MessageReceivedHandler;
        }
        
        private void ClientConnectedHandler(object sender, ClientConnectedEventArgs eventArgs)
        {
            _synchronizationContext.Post(
                e => ClientConnectedEvent?.Invoke(this, (ClientConnectedEventArgs)e),
                eventArgs);

            StartNamedPipeServer();
        }
        
        private void ClientDisconnectedHandler(object sender, ClientDisconnectedEventArgs eventArgs)
        {
            _synchronizationContext.Post(
                e => ClientDisconnectedEvent?.Invoke(this, (ClientDisconnectedEventArgs)e), 
                eventArgs);

            StopNamedPipeServer(eventArgs.ClientId);
        }
        
        private void MessageReceivedHandler(object sender, MessageReceivedEventArgs<T> eventArgs)
        {
            _synchronizationContext.Post(
                e => MessageReceivedEvent?.Invoke(this, (MessageReceivedEventArgs<T>)e), 
                eventArgs);
        }
    }
}
