namespace BlockchainNet.Core.Communication
{
    public interface ICommunication
    {
        /// <summary>
        /// Id сервера
        /// </summary>
        string ServerId { get; }

        /// <summary>
        /// Запускает коммуникацию
        /// </summary>
        void Start();

        /// <summary>
        /// Остагавливает коммуникацию
        /// </summary>
        void Stop();
    }
}
