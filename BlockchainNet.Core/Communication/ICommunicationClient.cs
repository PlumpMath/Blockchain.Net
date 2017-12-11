namespace BlockchainNet.Core.Communication
{
    using System.Threading.Tasks;

    public interface ICommunicationClient<T> : ICommunication
    {
        /// <summary>
        /// Id сервера для обратной связи
        /// </summary>
        string ResponceServerId { get; set; }

        /// <summary>
        /// Асинхронный метод для отправки сообщения
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns>True если отправка успещна, иначе False</returns>
        Task<bool> SendMessageAsync(T message);
    }
}
