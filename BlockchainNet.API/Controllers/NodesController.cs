namespace BlockchainNet.API.Controllers
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    
    using Microsoft.AspNetCore.Mvc;

    using BlockchainNet.Core;

    [Route("api/[controller]")]
    public class NodesController : Controller
    {
        private Blockchain blockchain;

        public NodesController(Blockchain chain)
        {
            blockchain = chain;
        }
        
        [HttpGet]
        public IEnumerable<Uri> Get()
        {
            return blockchain.Nodes;
        }

        [HttpPut]
        public void Register([FromForm]List<Uri> nodes)
        {
            foreach (var node in nodes)
                blockchain.RegisterNode(node);
        }

        [HttpGet("resolve")]
        public Task ResolveAsync()
        {
            return blockchain.ResolveConflictsAsync();
        }
    }
}
