namespace BlockchainNet.Pipe.Client
{
    using System.IO;
    using System.IO.Pipes;
    using System.Threading.Tasks;

    using BlockchainNet.Core.Communication;

    using ProtoBuf;

    public class PipeClient<T> : ICommunicationClient<T>
    {
        private readonly NamedPipeClientStream _pipeClient;
        
        public PipeClient(string serverId)
        {
            _pipeClient = new NamedPipeClientStream(
                ".", 
                serverId, 
                PipeDirection.Out, 
                PipeOptions.Asynchronous);
        }
        
        public void Start()
        {
            _pipeClient.Connect();
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
