namespace BlockchainNet.Core.Communication
{
    public interface ICommunicationClientFactory<T>
    {
        ICommunicationClient<T> CreateNew(string serverId);
    }
}
