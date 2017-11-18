namespace BlockchainNet.Core
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class Block
    {
        public int Index { get; }
        public DateTime Date { get; }
        public IReadOnlyList<Transaction> Transactions { get; }
        public long Proof { get; }
        public string PreviousHash { get; }

        public Block(int index, DateTime date, IEnumerable<Transaction> transactions, long proof, string previousHash)
        {
            Index = index;
            Date = date;
            Transactions = transactions.ToList().AsReadOnly();
            Proof = proof;
            PreviousHash = previousHash;
        }
    }
}
