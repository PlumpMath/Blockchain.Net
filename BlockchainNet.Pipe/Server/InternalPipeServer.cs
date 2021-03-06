﻿namespace BlockchainNet.Pipe.Server
{
    using System;
    using System.Text;
    using System.IO;
    using System.IO.Pipes;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    using BlockchainNet.Core.Communication;

    using ProtoBuf;

    internal class InternalPipeServer<T> : ICommunicationServer<T>
    {
        private readonly NamedPipeServerStream _pipeServer;
        private bool _isStopping;

        class Package
        {
            public byte[] Buffer = new byte[PipeServer<T>.BufferSize];
            public List<byte> Result = new List<byte>();
        }

        public InternalPipeServer(string pipeName, int maxNumberOfServerInstances)
        {
            _pipeServer = new NamedPipeServerStream(
                pipeName, 
                PipeDirection.In, 
                maxNumberOfServerInstances,
                PipeTransmissionMode.Message, 
                PipeOptions.Asynchronous);

            ServerId = pipeName;
        }

        public event EventHandler<ClientConnectedEventArgs> ClientConnectedEvent;
        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnectedEvent;
        public event EventHandler<MessageReceivedEventArgs<T>> MessageReceivedEvent;

        public string ServerId { get; }

        private string responceClientId;

        public void Start()
        {
            _pipeServer.WaitForConnectionAsync().ContinueWith(t =>
            {
                if (!_isStopping)
                {
                    // сразу читаем id сервера для обратной связи
                    // он пригодится для передачи в обработчики событий
                    var buffer = new byte[2048];
                    var length = _pipeServer.Read(buffer, 0, buffer.Length);
                    responceClientId = Encoding.UTF8.GetString(buffer, 0, length);

                    if (responceClientId == "\0")
                        responceClientId = null;

                    ClientConnectedEvent?.Invoke(
                        this,
                        new ClientConnectedEventArgs { ClientId = responceClientId });

                    return ReadAsync(new Package());
                }
                return Task.CompletedTask;
            });
        }

        public void Stop()
        {
            _isStopping = true;

            try
            {
                if (_pipeServer.IsConnected)
                {
                    _pipeServer.Disconnect();
                }
            }
            finally
            {
                _pipeServer.Close();
                _pipeServer.Dispose();
            }
        }

        private async Task ReadAsync(Package package)
        {
            // побуферно читаем сообщения, что позволяет не ограничиваться его размером

            var readBytes = await _pipeServer.ReadAsync(package.Buffer, 0, package.Buffer.Length);

            if (readBytes > 0)
            {
                byte[] result;
                if (_pipeServer.IsMessageComplete)
                {
                    result = new byte[readBytes];
                    Array.Copy(package.Buffer, 0, result, 0, readBytes);
                }
                else
                {
                    result = package.Buffer;
                }

                package.Result.AddRange(result);

                // если не дочитали...
                if (!_pipeServer.IsMessageComplete)
                {
                    // ...то читаем следующий буфер
                    await ReadAsync(package);
                }
                else
                {
                    using (var stream = new MemoryStream(package.Result.ToArray()))
                    {
                        var message = Serializer.Deserialize<T>(stream);

                        MessageReceivedEvent?.Invoke(this,
                            new MessageReceivedEventArgs<T>
                            {
                                ClientId = responceClientId,
                                Message = message
                            });
                    }
                    await ReadAsync(new Package());
                }
            }
            // Если прочитано 0 байт, то клиент вероятно отключился
            else
            {
                if (!_isStopping)
                {
                    Stop();
                    ClientDisconnectedEvent?.Invoke(
                        this, 
                        new ClientDisconnectedEventArgs { ClientId = responceClientId });
                }
            }
        }
    }
}
