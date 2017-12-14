namespace BlockchainNet.Pipe.Client
{
    using System.IO;
    using System.IO.Pipes;
    using System.Text;
    using System.Threading.Tasks;

    using BlockchainNet.Core.Communication;

    using ProtoBuf;

    public class PipeClient<T> : ICommunicationClient<T>
    {
        private NamedPipeClientStream _pipeClient;

        public PipeClient(string serverId)
        {
            ServerId = serverId;
        }

        public string ServerId { get; }

        public string ResponceServerId { get; set; }

        public void Start()
        {
            if (_pipeClient == null)
            {
                _pipeClient = new NamedPipeClientStream(
                    ".",
                    ServerId,
                    PipeDirection.Out,
                    PipeOptions.Asynchronous);
            }

            _pipeClient.Connect();

            // Сразу отправляем Id сервера для обратной связи
            var buffer = Encoding.UTF8.GetBytes(string.IsNullOrEmpty(ResponceServerId) ? "\0" : ResponceServerId);
            _pipeClient.Write(buffer, 0, buffer.Length);
        }
        
        public void Stop()
        {
            try
            {
                _pipeClient.WaitForPipeDrain();
            }
            finally
            {
                _pipeClient.Close();
                _pipeClient.Dispose();
                _pipeClient = null;
            }
        }

        public async Task<bool> SendMessageAsync(T message)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, message);

                await _pipeClient.WriteAsync(stream.ToArray(), 0, (int)stream.Length);
                await _pipeClient.FlushAsync();
            }
            return true;
        }
    }
}
