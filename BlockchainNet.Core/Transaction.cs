namespace BlockchainNet.Core
{
    public class Transaction
    {
        public string Sender { get; }
        public string Recipient { get; }
        public double Amount { get; }

        public Transaction(string sender, string recipient, double amout)
        {
            Sender = sender;
            Recipient = recipient;
            Amount = amout;
        }
    }
}