namespace BlockchainNet.Core.Communication
{
    public interface ICommunication
    {
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
