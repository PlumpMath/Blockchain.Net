namespace BlockchainNet.Core
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using BlockchainNet.Core.Models;

    public class BlockAddedEventArgs : EventArgs
    {
        public Block AddedBlock { get; }
        public IReadOnlyList<Block> Chain { get; }

        public BlockAddedEventArgs(Block addedBlock, IEnumerable<Block> chain = null)
        {
            AddedBlock = addedBlock;
            Chain = (chain?.ToList() ?? new List<Block> { addedBlock }).AsReadOnly();
        }
    }
}
