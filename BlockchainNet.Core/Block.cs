﻿namespace BlockchainNet.Core
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using ProtoBuf;

    [ProtoContract]
    public class Block
    {
        [ProtoMember(1)]
        public int Index { get; }

        [ProtoMember(2)]
        public DateTime Date { get; }

        [ProtoMember(3)]
        private List<Transaction> transaction;

        [ProtoIgnore]
        public IReadOnlyList<Transaction> Transactions => transaction.AsReadOnly();

        [ProtoMember(4)]
        public long Proof { get; }

        [ProtoMember(5)]
        public string PreviousHash { get; }

        private Block()
        {

        }

        public Block(int index, DateTime date, IEnumerable<Transaction> transactions, long proof, string previousHash)
        {
            Index = index;
            Date = date;
            transaction = transactions.ToList();
            Proof = proof;
            PreviousHash = previousHash;
        }
    }
}