namespace BlockchainNet.Console
{
    using BlockchainNet.Core;
    using BlockchainNet.Core.Communication;
    using BlockchainNet.Core.Models;
    using BlockchainNet.Pipe;
    using BlockchainNet.Pipe.Client;
    using BlockchainNet.Pipe.Server;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    static class Program
    {
        private static Communicator communicator;
        private static Blockchain blockchain => communicator.Blockchain;
        
        private static string account;

        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        private static void Main(string[] args)
        {
            SetConsoleCtrlHandler(arg =>
                {
                    if (arg == 2)
                        communicator.Close();
                    return false;
                }, true);
            
            var currentPipeId = ProcessPipeHelper.GetCurrentPipeId();
            Console.Title = currentPipeId;

            communicator = new Communicator(
                new PipeServer<List<Block>>(currentPipeId),
                new PipeClientFactory<List<Block>>())
            {
                Blockchain = Blockchain.CreateNew()
            };
            
            communicator.ConnectTo(ProcessPipeHelper.GetNeighborPipesIds());
            
            AskAndSetAccount();

            var active = true;
            while (active)
            {
                var parts = Console.ReadLine().Split(' ');
                if (parts.Length == 0)
                    continue;

                switch (parts[0])
                {
                    case "exit":
                    case "e":
                        active = false;
                        break;
                    case "blocks":
                    case "b":
                        PrintBlockchain();
                        break;
                    case "transactions":
                    case "t":
                        PrintCurrentTransactions();
                        break;
                    case "mine":
                    case "m":
                        _ = MineAndPrintNewBlock();
                        break;
                    case "add":
                    case "a":
                        ReadAndAddNewTransaction(parts.Skip(1).ToArray());
                        break;
                    case "sync":
                    case "s":
                        SyncBlockchain();
                        break;
                    case "help":
                    case "h":
                        PrintHelp();
                        break;
                    case "amount":
                    case "am":
                        PrintAccountAmount(parts.Skip(1).FirstOrDefault());
                        break;
                    case "save":
                    case "sv":
                        SaveBlockChain(parts.Skip(1).FirstOrDefault());
                        break;
                    case "load":
                    case "ld":
                        LoadBlockChain(parts.Skip(1).FirstOrDefault());
                        break;
                    case "switch":
                    case "sw":
                        AskAndSetAccount(parts.Skip(1).FirstOrDefault());
                        break;
                }
            }

            communicator.Close();
        }
        
        private static void AskAndSetAccount(string newAccount = null)
        {
            if (string.IsNullOrWhiteSpace(newAccount))
            {
                Console.Write("Input account: ");
                newAccount = Console.ReadLine();
            }
            account = newAccount;
        }

        private static void PrintAccountAmount(string account)
        {
            while (string.IsNullOrWhiteSpace(account))
            {
                Console.Write("Account: ");
                account = Console.ReadLine();
            }
            var amount = blockchain.GetAccountAmount(account);
            Console.WriteLine($"{account} has: {amount}");
        }

        private static void PrintHelp()
        {
            Console.WriteLine("e, exit - close the node");
            Console.WriteLine("a, add [recipient] [amount] - add new transaction");
            Console.WriteLine("t, transactions - print current transactions");
            Console.WriteLine("b, blocks - print blockschain");
            Console.WriteLine("m, mine - mine new block");
            Console.WriteLine("s, sync - sync blockchain with neighbor");
            Console.WriteLine("sv, save [filename] - save blockchain");
            Console.WriteLine("ld, load [filename] - load blockchain");
            Console.WriteLine("am, amount [account] - print account amount");
            Console.WriteLine("sw, switch [account] - change account");
        }

        private static void ReadAndAddNewTransaction(string[] parts)
        {
            var recipient = parts.FirstOrDefault();
            while (string.IsNullOrEmpty(recipient))
            {
                Console.Write("Recipient: ");
                recipient = Console.ReadLine();
            }
            var amount = double.TryParse(parts.Skip(1).FirstOrDefault(), out double temp) ? temp : 0;
            
            while (amount <= 0)
            {
                Console.Write("Amount: ");
                amount = double.TryParse(Console.ReadLine(), out temp) ? temp : 0;
            }
            
            if (blockchain.GetAccountAmount(account) < amount)
            {
                Console.WriteLine("Account amount is not enough");
                return;
            }

            var index = blockchain.NewTransaction(account, recipient, amount);
            Console.WriteLine($"Transaction added to block #{index}");
        }

        private static Task MineAndPrintNewBlock()
        {
            return Task.Run(() =>
            {
                var block = blockchain.Mine(account);
                Console.WriteLine(block.ToString());
            });
        }

        private static void PrintBlockchain()
        {
            Console.WriteLine($"Blocks count = {blockchain.Chain.Count}");
            foreach (var block in blockchain.Chain)
            {
                Console.WriteLine(block.ToString());
                if (block.Transactions.Count > 0)
                    Console.WriteLine(string.Join("\r\n", block.Transactions));
            }
            Console.WriteLine();
        }

        private static void PrintCurrentTransactions()
        {
            Console.WriteLine($"Current transactions {blockchain.CurrentTransactions.Count}:");
            Console.WriteLine(string.Join("\r\n", blockchain.CurrentTransactions));
        }

        private static async void SyncBlockchain()
        {
            await communicator.SyncAsync();
            Console.WriteLine("Sync messages sended");
        }

        private static void SaveBlockChain(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "chain.protobuf";

            try
            {
                blockchain.SaveFile(fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Blockchain saved to " + fileName);
        }
        
        private static void LoadBlockChain(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "chain.protobuf";

            if (!File.Exists(fileName))
            {
                Console.WriteLine("No file");
                return;
            }

            try
            {
                communicator.Blockchain = Blockchain.FromFile(fileName);
                Console.WriteLine("Blockchain loaded from " + fileName);
                PrintBlockchain();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        private static string GetPipeIdFromProcessId(int processId)
        {
            return $"Pipe-{processId}";
        }
    }
}
