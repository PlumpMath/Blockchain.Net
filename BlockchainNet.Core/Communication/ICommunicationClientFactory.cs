namespace BlockchainNet.Core.Communication
{
    public interface ICommunicationClientFactory<T>
    {
        /// <summary>
        /// Создает новый клиент, подключаемый к серверу по его Id
        /// </summary>
        /// <param name="serverId">Id сервера, к которому подключается клиент</param>
        /// <returns>Экземпляр клиента</returns>
        ICommunicationClient<T> CreateNew(string serverId);
    }
}
