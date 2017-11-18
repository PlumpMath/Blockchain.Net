namespace BlockchainNet.API.Controllers
{
    using System.Collections.Generic;

    using Microsoft.AspNetCore.Mvc;

    using BlockchainNet.Core;
    
    [Route("api/[controller]")]
    public class BlockchainController : Controller
    {
        private Blockchain blockchain;

        public BlockchainController(Blockchain chain)
        {
            blockchain = chain;
        }
        
        [HttpGet]
        public IEnumerable<Block> Get()
        {
            return blockchain.Chain;
        }

        [HttpGet("mine")]
        public Block Mine()
        {
            var lastBlock = blockchain.LastBlock();
            var lastProof = lastBlock.Proof;

            var proof = blockchain.ProofOfWork(lastProof);

            blockchain.NewTransaction("0", "1234", 1);

            return blockchain.NewBlock(proof);
        }
    }
}
