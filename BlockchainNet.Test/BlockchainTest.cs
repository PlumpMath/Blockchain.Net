﻿namespace BlockchainNet.Test
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
            blockchain.NewTransaction("Alice", "Bob", 0);
            var block = blockchain.Mine("Alice");
            var isValid = blockchain.IsValidChain(blockchain.Chain);

            Assert.IsTrue(isValid, "Blockchain is invalid");
        }
    }
}
