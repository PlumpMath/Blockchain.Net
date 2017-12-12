namespace BlockchainNet.Test
{
    using BlockchainNet.Core;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BlockchainTest
    {
        [TestMethod]
        public void Blockchain_MineTest()
        {
            var blockchain = Blockchain.CreateNew();
            blockchain.NewTransaction("Alice", "Bob", 100);
            var block = blockchain.Mine();
            var isValid = blockchain.IsValidChain(blockchain.Chain);

            Assert.IsTrue(isValid, "Blockchain is invalid");
        }
    }
}
