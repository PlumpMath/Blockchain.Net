namespace BlockchainNet.Core
{
    using System;
    using System.IO;
    using System.Text;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Security.Cryptography;

    using ProtoBuf;

    public class Blockchain
    {
        private List<Block> chain;
        private List<Transaction> currentTransactions;
        private List<Uri> nodes;

        /// <summary>
        /// Цепочка блоков, блокчейн
        /// </summary>
        public IReadOnlyList<Block> Chain => chain;

        /// <summary>
        /// Текущие транзакции
        /// </summary>
        public IReadOnlyList<Transaction> CurrentTransactions => currentTransactions;

        /// <summary>
        /// Зарегестрированные соседние узлы
        /// </summary>
        public IReadOnlyList<Uri> Nodes => nodes;

        public Blockchain()
        {
            chain = new List<Block>();
            currentTransactions = new List<Transaction>();
            nodes = new List<Uri>();

            NewBlock(100, "1");
        }

        /// <summary>
        /// Регистирует адрес узла
        /// </summary>
        /// <param name="node">Адрес узла</param>
        public void RegisterNode(Uri node)
        {
            if (!nodes.Contains(node))
                nodes.Add(node);
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

            foreach (var block in chain.Skip(1))
            {
                if (block.PreviousHash != Hash(prevBlock)
                    || !IsValidProof(prevBlock.Proof, block.Proof))
                    return false;

                prevBlock = block;
            }
            return true;
        }

        /// <summary>
        /// Решение конфликтов с другими узлами
        /// </summary>
        /// <returns>True если наша цепочка была заменена, False если это нет</returns>
        public async Task<bool> ResolveConflictsAsync()
        {
            var currentLength = chain.Count;

            var httpClient = new HttpClient();
            var newChain = (await Task
                .WhenAll(nodes
                .Select(async node =>
                {
                    try
                    {
                        var chainResponce = await httpClient.GetStreamAsync(new Uri(node, "chain")).ConfigureAwait(false);
                        var chain = Serializer.Deserialize<List<Block>>(chainResponce);

                        if (chain.Count < currentLength
                            || !IsValidChain(chain))
                            return null;

                        return chain;
                    }
                    catch
                    {
                        return null;
                    }
                }))
                .ConfigureAwait(false))
                .Where(chain => chain != null)
                .OrderBy(chain => chain.Count)
                .FirstOrDefault();
            
            if (newChain != null)
            {
                chain = newChain;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Создает новый Блок и добавляет его к цепочке
        /// </summary>
        /// <param name="proof">Доказательство работы</param>
        /// <param name="previousHash">Хэш предыдущего блока, может быть null</param>
        /// <returns>Созданный блок</returns>
        public Block NewBlock(long proof, string previousHash = null)
        {
            var block = new Block(
                chain.Count,
                DateTime.Now,
                currentTransactions,
                proof,
                previousHash);
            chain.Add(block);

            currentTransactions.Clear();

            return block;
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
            var transaction = new Transaction(sender, recipient, amount);
            currentTransactions.Add(transaction);
            return LastBlock().Index + 1;
        }
        
        /// <param name="block">Хешируемый блок</param>
        /// <returns>SHA-256 хэш блока</returns>
        private static string Hash(Block block)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, block);
                var blockBytes = stream.ToArray();
                var sha = SHA256.Create();
                return Encoding.UTF8.GetString(sha.ComputeHash(blockBytes));
            }
        }
        
        /// <returns>Последний блок в цепочке</returns>
        public Block LastBlock()
        {
            return chain.Last();
        }

        public long ProofOfWork(long lastProof)
        {
            var proof = 0L;

            while (!IsValidProof(lastProof, proof))
                proof++;

            return proof;
        }

        private static bool IsValidProof(long lastProof, long proof)
        {
            var blockBytes = Encoding.UTF8.GetBytes($"{lastProof}{proof}");
            var sha = SHA256.Create();
            return Encoding.UTF8.GetString(sha.ComputeHash(blockBytes)).EndsWith("00");
        }
    }
}