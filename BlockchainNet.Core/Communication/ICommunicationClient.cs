namespace BlockchainNet.Core.Communication
{
    using System.Threading.Tasks;

    public interface ICommunicationClient<T> : ICommunication
    {
        /// <summary>
        /// Асинхронный метод для отправки сообщения
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns>True если отправка успещна, иначе False</returns>
        Task<bool> SendMessageAsync(T message);
    }
}
