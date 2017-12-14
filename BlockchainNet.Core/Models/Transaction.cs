namespace BlockchainNet.Core.Models
{
    using System;

    using ProtoBuf;

    [ProtoContract]
    public class Transaction
    {
        [ProtoMember(1)]
        public string Sender { get; }

        [ProtoMember(2)]
        public string Recipient { get; }

        [ProtoMember(3)]
        public double Amount { get; }

        [ProtoMember(4)]
        public DateTime Date { get; }

        /// <summary>
        /// Приватный конструктор для сериализации
        /// </summary>
        private Transaction()
        {

        }

        /// <summary>
        /// Конструктор транзацкии
        /// </summary>
        /// <param name="sender">Отправитель</param>
        /// <param name="recipient">Получатель</param>
        /// <param name="amout">Сумма</param>
        /// <param name="date">Дата создания</param>
        public Transaction(string sender, string recipient, double amout, DateTime date)
        {
            Sender = sender;
            Recipient = recipient;
            Amount = amout;
            Date = date;
        }

        public override string ToString()
        {
            return $"Transaction: {Amount} from {(string.IsNullOrEmpty(Sender) ? "mining" : Sender)} to {Recipient}";
        }
    }
}