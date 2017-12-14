namespace BlockchainNet.Core.Communication
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    using BlockchainNet.Core.Models;
    using System;

    public class Communicator
    {
        private readonly ICommunicationServer<List<Block>> server;
        private readonly ICommunicationClientFactory<List<Block>> clientFactory;

        private readonly List<ICommunicationClient<List<Block>>> nodes;

        public Blockchain Blockchain { get; set; }

        /// <summary>
        /// Конструктор коммуникатора
        /// </summary>
        /// <param name="server">Сервер</param>
        /// <param name="clientFactory">Фаблика клиентов</param>
        public Communicator(
            ICommunicationServer<List<Block>> server,
            ICommunicationClientFactory<List<Block>> clientFactory)
        {
            this.server = server;
            this.clientFactory = clientFactory;

            nodes = new List<ICommunicationClient<List<Block>>>();

            server.MessageReceivedEvent += Server_MessageReceivedEvent;
            server.ClientConnectedEvent += Server_ClientConnectedEvent;
            server.ClientDisconnectedEvent += Server_ClientDisconnectedEvent;
            server.Start();
        }
        
        public Task SyncAsync(bool onlyGet = false)
        {
            if (Blockchain == null)
                throw new InvalidOperationException("Blockchain must be setted");

            var sendedList = onlyGet
                ? new List<Block>()
                : Blockchain.Chain.ToList();
            return Task
                .WhenAll(nodes
                .Select(node => node.SendMessageAsync(sendedList)));
        }

        public void ConnectTo(IEnumerable<string> serversId)
        {
            foreach (var serverId in serversId)
            {
                var nodeClient = clientFactory.CreateNew(serverId);
                nodeClient.ResponceServerId = server.ServerId;
                nodeClient.Start();
                nodes.Add(nodeClient);
            }
        }

        public void Close()
        {
            server.Stop();

            foreach (var nodeClient in nodes)
                nodeClient.Stop();
        }

        private void Server_ClientConnectedEvent(object sender, ClientConnectedEventArgs e)
        {
            if (nodes.All(c => c.ServerId != e.ClientId))
            {
                var nodeClient = clientFactory.CreateNew(e.ClientId);
                nodeClient.ResponceServerId = server.ServerId;
                nodeClient.Start();
                nodes.Add(nodeClient);
            }
        }

        private void Server_ClientDisconnectedEvent(object sender, ClientDisconnectedEventArgs e)
        {
            var node = nodes.FirstOrDefault(n => n.ServerId == e.ClientId);
            if (node != null)
            {
                node.Stop();
                nodes.Remove(node);
            }
        }

        private async void Server_MessageReceivedEvent(object sender, MessageReceivedEventArgs<List<Block>> e)
        {
            if (Blockchain == null)
                throw new InvalidOperationException("Blockchain must be setted");

            var replaced = Blockchain.TryAddChainIfValid(e.Message);
            // Если не заменено, то входящяя цепочка или невалидна, или меньше существующей
            // Есть смысл отправить отправителю свою цепочку для замены
            if (!replaced)
            {
                var nodeClient = nodes.FirstOrDefault(c => c.ServerId == e.ClientId);
                await nodeClient.SendMessageAsync(Blockchain.Chain.ToList());
            }
        }
    }
}
