namespace BlockchainNet.Console
{
    using BlockchainNet.Core;
    using BlockchainNet.Core.Communication;
    using BlockchainNet.Pipe.Client;
    using BlockchainNet.Pipe.Server;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    static class Program
    {
        private static ICommunicationServer<List<Block>> server;
        private static List<ICommunicationClient<List<Block>>> nodes;
        private static Blockchain blockchain;

        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        private static void Main(string[] args)
        {
            SetConsoleCtrlHandler(arg =>
                {
                    if (arg == 2)
                        server?.Stop();
                    return false;
                }, true);

            nodes = new List<ICommunicationClient<List<Block>>>();
            blockchain = new Blockchain();

            var currentProcess = Process.GetCurrentProcess();
            var currentPipeId = GetPipeIdFromProcessId(currentProcess.Id);
            Console.Title = currentPipeId;

            server = new PipeServer<List<Block>>(currentPipeId);
            server.MessageReceivedEvent += CurrentServer_MessageReceivedEvent;
            server.Start();

            server.ClientConnectedEvent += Server_ClientConnectedEvent;
            server.ClientDisconnectedEvent += Server_ClientDisconnectedEvent;

            var processes = Process
                .GetProcessesByName(currentProcess.ProcessName)
                .Where(p => p.Id != currentProcess.Id)
                .ToArray();


            foreach (var process in processes)
            {
                var node = new PipeClient<List<Block>>(GetPipeIdFromProcessId(process.Id))
                {
                    ResponceServerId = server.ServerId
                };
                node.Start();
                nodes.Add(node);
            }

            if (processes.Length > 0)
                Console.WriteLine("Connected to: " + string.Join(", ", processes.Select(p => p.Id)));

            var active = true;
            while (active)
            {
                switch (Console.ReadLine())
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
                        ReadAndAddNewTransaction();
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
                }
            }

            server.Stop();

            foreach (var client in nodes)
                client.Stop();
        }

        private static void Server_ClientDisconnectedEvent(object sender, ClientDisconnectedEventArgs e)
        {
            var node = nodes.FirstOrDefault(n => n.ServerId == e.ClientId);
            if (node != null)
            {
                node.Stop();
                nodes.Remove(node);
                Console.WriteLine("Disconected: " + e.ClientId);
            }
        }

        private static void Server_ClientConnectedEvent(object sender, ClientConnectedEventArgs e)
        {
            if (nodes.All(c => c.ServerId != e.ClientId))
            {
                var client = new PipeClient<List<Block>>(e.ClientId)
                {
                    ResponceServerId = server.ServerId
                };
                client.Start();
                nodes.Add(client);
                Console.WriteLine("Connected: " + e.ClientId);
            }
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
            Console.WriteLine("a, add - add new transaction");
            Console.WriteLine("t, transactions - print current transactions");
            Console.WriteLine("b, blocks - print blockschain");
            Console.WriteLine("m, mine - mine new block");
            Console.WriteLine("s, sync - sync blockchain with neighbor");
        }

        private static void ReadAndAddNewTransaction()
        {
            Console.Write("Recipient: ");
            var recipient = Console.ReadLine();

            Console.Write("Amount: ");
            var amount = double.TryParse(Console.ReadLine(), out double temp) ? temp : 0;

            var index = blockchain.NewTransaction(server.ServerId, recipient, amount);
            Console.WriteLine($"Transaction added to block #{index}");
        }

        private static Task MineAndPrintNewBlock()
        {
            return Task.Run(() =>
            {
                var block = blockchain.Mine();
                Console.WriteLine(block.ToString());
            });
        }

        private static void PrintBlockchain()
        {
            Console.WriteLine($"Blocks count = {blockchain.Chain.Count}");
            foreach (var block in blockchain.Chain)
            {
                Console.WriteLine(block.ToString());
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
            await Task
                .WhenAll(nodes
                .Select(client => client.SendMessageAsync(blockchain.Chain.ToList())));
            Console.WriteLine("Sync messages sended");
        }

        private static async void CurrentServer_MessageReceivedEvent(object sender, MessageReceivedEventArgs<List<Block>> e)
        {
            var replaced = blockchain.TryAddChainIfValid(e.Message);
            if (replaced)
                Console.WriteLine("Blockchain replaced");
            else
            {
                var client = nodes.FirstOrDefault(c => c.ServerId == e.ClientId);
                await client.SendMessageAsync(blockchain.Chain.ToList());
            }
        }

        private static string GetPipeIdFromProcessId(int processId)
        {
            return $"Pipe-{processId}";
        }
    }
}
