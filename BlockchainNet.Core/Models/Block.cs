namespace BlockchainNet.Core.Models
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
        public IReadOnlyList<Transaction> Transactions => (transaction ?? new List<Transaction>()).AsReadOnly();

        [ProtoMember(4)]
        public long Proof { get; }

        [ProtoMember(5)]
        public string PreviousHash { get; }

        /// <summary>
        /// Приватные конструктор для сериализации
        /// </summary>
        private Block()
        {

        }

        /// <summary>
        /// Конструктор блока
        /// </summary>
        /// <param name="index">Индекс блока</param>
        /// <param name="date">Дата создания</param>
        /// <param name="transactions">Транзакции</param>
        /// <param name="proof">Доказательство доберия блоку</param>
        /// <param name="previousHash">Хэш предыдущего блока</param>
        public Block(int index, DateTime date, IEnumerable<Transaction> transactions, long proof, string previousHash)
        {
            Index = index;
            Date = date;
            transaction = transactions?.ToList() ?? new List<Transaction>();
            Proof = proof;
            PreviousHash = previousHash;
        }

        public override string ToString()
        {
            var str = $"Block #{Index} {Date}\r\n";
            return str = $"Proof = {Proof}; Previous hash = {PreviousHash}";
        }
    }
}
