namespace BlockchainNet.Core
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;

    using ProtoBuf;

    using BlockchainNet.Core.Models;

    [ProtoContract]
    public class Blockchain
    {
        [ProtoMember(1)]
        private List<Block> chain;

        [ProtoMember(2)]
        private List<Transaction> currentTransactions;

        /// <summary>
        /// Цепочка блоков, блокчейн
        /// </summary>
        public IReadOnlyList<Block> Chain => chain ?? new List<Block>();

        /// <summary>
        /// Текущие транзакции
        /// </summary>
        public IReadOnlyList<Transaction> CurrentTransactions => currentTransactions ?? new List<Transaction>();

        public event EventHandler<BlockAddedEventArgs> BlockAdded;

        private Blockchain()
        {
            chain = new List<Block>();
            currentTransactions = new List<Transaction>();
        }

        /// <summary>
        /// Создает блокчейн с одним генезис блоком
        /// </summary>
        /// <returns>Созданный блокчейн</returns>
        public static Blockchain CreateNew()
        {
            var blockchain = new Blockchain();
            blockchain.NewBlock(100, "1");
            return blockchain;
        }

        /// <summary>
        /// Читает и создает блокчейн из файла
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <returns>Прочитанный блокчейн</returns>
        public static Blockchain FromFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));

            using (var file = new FileStream(fileName, FileMode.OpenOrCreate))
                return Serializer.Deserialize<Blockchain>(file);
        }

        /// <summary>
        /// Сохраняет текущий блокчейн в файл
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        public void SaveFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));

            using (var file = new FileStream(fileName, FileMode.OpenOrCreate))
                Serializer.Serialize(file, this);
        }

        /// <summary>
        /// Проверка блокчейна
        /// </summary>
        /// <param name="chain">Блокчейн</param>
        /// <returns>True если прошел проверку, иначе False</returns>
        public bool IsValidChain(IEnumerable<Block> chain)
        {
            var prevBlock = chain.FirstOrDefault();
            if (prevBlock == null)
                throw new ArgumentException("Blocks chain cannot be empty", nameof(chain));

            var amountDictionary = new Dictionary<string, double>();

            foreach (var block in chain.Skip(1))
            {
                if (block.PreviousHash != Crypto.HashBlockInBase64(prevBlock)
                    || !IsValidProof(prevBlock.Proof, block.Proof))
                    return false;

                foreach (var transaction in block.Transactions)
                {
                    if (!string.IsNullOrEmpty(transaction.Sender))
                    {
                        if (amountDictionary.ContainsKey(transaction.Sender))
                        {
                            amountDictionary[transaction.Sender] -= transaction.Amount;
                            if (amountDictionary[transaction.Sender] < 0)
                                return false;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    if (amountDictionary.ContainsKey(transaction.Recipient))
                    {
                        amountDictionary[transaction.Recipient] += transaction.Amount;
                    }
                    else
                    {
                        amountDictionary.Add(transaction.Recipient, transaction.Amount);
                    }
                }

                prevBlock = block;
            }
            return true;
        }
        
        /// <summary>
        /// Добавляет новую транзакцию к списку транзакций
        /// </summary>
        /// <param name="sender">Адресс отправителя</param>
        /// <param name="recipient">Адресс получателя</param>
        /// <param name="amount">Сумма</param>
        /// <returns>Индекс блока, хранящего добаленную транзакцию</returns>
        public int NewTransaction(string sender, string recipient, double amount)
        {
            if (GetAccountAmount(sender) < amount)
                throw new InvalidOperationException("Sender amount is not enough");

            var transaction = new Transaction(sender, recipient, amount);
            currentTransactions.Add(transaction);
            return LastBlock().Index + 1;
        }

        /// <returns>Последний блок в цепочке</returns>
        public Block LastBlock()
        {
            return chain.Last();
        }

        /// <summary>
        /// Запускает процесс майнинга нового блока
        /// </summary>
        /// <returns>Новый блок</returns>
        public Block Mine()
        {
            var lastBlock = LastBlock();
            var lastProof = lastBlock.Proof;
            var lastHash = Crypto.HashBlockInBase64(lastBlock);

            var proof = ProofOfWork(lastProof);

            return NewBlock(proof, lastHash);
        }

        /// <summary>
        /// Пытается заменить блокчейн
        /// </summary>
        /// <param name="recievedChain">Новый блокчейн</param>
        /// <returns>True, если новый блок валидный и замена успешна, иначе - False</returns>
        public bool TryAddChainIfValid(ICollection<Block> recievedChain)
        {
            if (recievedChain.Count >= chain.Count
                && IsValidChain(recievedChain))
            {
                chain = recievedChain.ToList();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Возвращает сумму на счету аккаунта
        /// </summary>
        /// <param name="account">Аккаунт</param>
        /// <returns>Сумма на счету</returns>
        public double GetAccountAmount(string account)
        {
            if (string.IsNullOrEmpty(account))
                throw new ArgumentException("Account cannot be null or empty", nameof(account));

            var amount = 0d;
            foreach (var block in chain)
            {
                foreach (var transaction in block.Transactions)
                {
                    if (transaction.Recipient == account)
                        amount += transaction.Amount;
                    if (transaction.Sender == account)
                        amount -= transaction.Amount;
                }
            }
            if (amount < 0)
                throw new InvalidDataException("Chain is invalid");

            return amount;
        }

        private Block NewBlock(long proof, string previousHash = null)
        {
            var block = new Block(
                chain.Count,
                DateTime.Now,
                currentTransactions,
                proof,
                previousHash);
            chain.Add(block);

            currentTransactions.Clear();

            BlockAdded?.Invoke(this, new BlockAddedEventArgs(block, chain));

            return block;
        }

        private long ProofOfWork(long lastProof)
        {
            var proof = 0L;

            while (!IsValidProof(lastProof, proof))
                proof++;

            return proof;
        }

        private static bool IsValidProof(long lastProof, long proof)
        {
            return Crypto.HashString($"{lastProof}{proof}").EndsWith("00");
        }
    }
}