namespace BlockchainNet.Core.Communication
{
    using System;
    
    public interface ICommunicationServer<T> : ICommunication
    {
        /// <summary>
        /// Id сервера
        /// </summary>
        string ServerId { get; }

        /// <summary>
        /// Событие, которое вызывается при получении сообщения
        /// </summary>
        event EventHandler<MessageReceivedEventArgs<T>> MessageReceivedEvent;

        /// <summary>
        /// Событие, которое вызывается при подключении клиента 
        /// </summary>
        event EventHandler<ClientConnectedEventArgs> ClientConnectedEvent;

        /// <summary>
        /// Событие, которое вызывается при отключении клиента 
        /// </summary>
        event EventHandler<ClientDisconnectedEventArgs> ClientDisconnectedEvent;
    }

    public class ClientConnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Id клиента
        /// </summary>
        public string ClientId { get; set; }
    }

    public class ClientDisconnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Id клиента
        /// </summary>
        public string ClientId { get; set; }
    }

    public class MessageReceivedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Полученое сообщение
        /// </summary>
        public T Message { get; set; }
    }
}
