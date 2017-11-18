namespace BlockchainNet.API.Controllers
{
    using System.Collections.Generic;

    using Microsoft.AspNetCore.Mvc;

    using BlockchainNet.Core;

    [Route("api/[controller]")]
    public class TransactionController : Controller
    {
        private Blockchain blockchain;

        public TransactionController(Blockchain chain)
        {
            blockchain = chain;
        }
        
        [HttpGet]
        public IEnumerable<Transaction> Get()
        {
            return blockchain.CurrentTransactions;
        }

        [HttpPut]
        public int New([FromForm]string sender, [FromForm]string recipient, [FromForm]double amount)
        {
            return blockchain.NewTransaction(sender, recipient, amount);
        }
    }
}