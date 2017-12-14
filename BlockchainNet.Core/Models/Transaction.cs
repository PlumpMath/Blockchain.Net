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

        private Transaction()
        {

        }

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