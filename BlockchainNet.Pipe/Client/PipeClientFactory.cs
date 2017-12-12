namespace BlockchainNet.Pipe.Client
{
    using BlockchainNet.Core.Communication;

    public class PipeClientFactory<T> : ICommunicationClientFactory<T>
    {
        public ICommunicationClient<T> CreateNew(string serverId)
        {
            return new PipeClient<T>(serverId);
        }
    }
}
