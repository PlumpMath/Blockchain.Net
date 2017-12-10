namespace BlockchainNet.Test.Protobuf
{
    using System.IO;

    using ProtoBuf;

    using BlockchainNet.Core;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TransactionTest
    {
        [TestMethod]
        public void Transaction_SerializeDeserializeTest()
        {
            var transaction = new Transaction("UserA", "UserB", 999);

            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, transaction);

                stream.Flush();
                stream.Position = 0;

                var newTransaction = Serializer.Deserialize<Transaction>(stream);
                
                Assert.IsTrue(transaction.Sender == newTransaction.Sender, "Transactions sender is not equal");
                Assert.IsTrue(transaction.Recipient == newTransaction.Recipient, "Transactions recipient is not equal");
                Assert.IsTrue(transaction.Amount == newTransaction.Amount, "Transactions amount is not equal");
            }
        }
    }
}
