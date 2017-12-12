namespace BlockchainNet.Core
{
    using System;
    using System.IO;
    using System.Text;
    using System.Security.Cryptography;

    using ProtoBuf;

    using BlockchainNet.Core.Models;

    public static class Crypto
    {
        /// <summary>
        /// Хеширует строку
        /// </summary>
        /// <param name="input">Входящая строка</param>
        /// <returns>Захешированная строка</returns>
        public static string HashString(string input)
        {
            var blockBytes = Encoding.UTF8.GetBytes(input);
            var sha = SHA256.Create();
            return Encoding.UTF8.GetString(sha.ComputeHash(blockBytes));
        }

        /// <summary>
        /// Хеширует блок
        /// </summary>
        /// <param name="block">Входящий блок</param>
        /// <returns>Блок захешированный в строку</returns>
        public static string HashBlockInBase64(Block block)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, block);
                var blockBytes = stream.ToArray();
                var sha = SHA256.Create();
                return Convert.ToBase64String(sha.ComputeHash(blockBytes));
            }
        }
    }
}
