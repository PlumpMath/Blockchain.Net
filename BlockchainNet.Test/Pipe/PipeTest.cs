using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlockchainNet.Pipe.Client;
using BlockchainNet.Pipe.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlockchainNet.Test.Pipe
{
    [TestClass]
    public class PipeTest
    {
        [TestMethod, Timeout(5000)]
        public void Pipe_Instantiate_PipeServer_IdTest()
        {
            var server = new PipeServer<string>();

            Assert.IsFalse(string.IsNullOrWhiteSpace(server.ServerId), "Server id is null or empty");
        }

        [TestMethod, Timeout(5000)]
        public async Task Pipe_Client_ConnectTest()
        {
            var tcs = new TaskCompletionSource<bool>();

            var isConnected = false;

            var server = new PipeServer<string>();
            var client = new PipeClient<string>(server.ServerId);

            server.ClientConnectedEvent += (sender, args) =>
            {
                isConnected = true;
                tcs.SetResult(true);
            };

            server.Start();
            client.Start();

            await tcs.Task;

            Assert.IsTrue(isConnected, "Client is not connected");
        }

        [TestMethod, Timeout(5000)]
        public async Task Pipe_Client_Connect_ResponceServerIdTest()
        {
            const string ResponceServerId = "responceServerId";

            var tcs = new TaskCompletionSource<bool>();
            var id = string.Empty;

            var server = new PipeServer<string>();
            var client = new PipeClient<string>(server.ServerId)
            {
                ResponceServerId = ResponceServerId
            };

            server.ClientConnectedEvent += (sender, args) =>
            {
                id = args.ClientId;
                tcs.SetResult(true);
            };

            server.Start();
            client.Start();

            await tcs.Task;

            Assert.AreEqual(ResponceServerId, id, "Responce server id does not match the correct");
        }

        [TestMethod, Timeout(5000)]
        public async Task Pipe_Client_Connect_ResponceServerId_EmptyTest()
        {
            const string ResponceServerId = null;

            var tcs = new TaskCompletionSource<bool>();
            var id = string.Empty;

            var server = new PipeServer<string>();
            var client = new PipeClient<string>(server.ServerId);

            server.ClientConnectedEvent += (sender, args) =>
            {
                id = args.ClientId;
                tcs.SetResult(true);
            };

            server.Start();
            client.Start();

            await tcs.Task;

            Assert.AreEqual(ResponceServerId, id, "Responce server id does not match the correct");
        }

        [TestMethod, Timeout(5000)]
        public async Task Pipe_Client_DisconnectTest()
        {
            var tcs = new TaskCompletionSource<bool>();

            var isDisconnected = false;

            var server = new PipeServer<string>();
            var client = new PipeClient<string>(server.ServerId);

            server.ClientDisconnectedEvent += (sender, args) =>
            {
                isDisconnected = true;
                tcs.SetResult(true);
            };

            server.Start();
            client.Start();

            Assert.IsFalse(isDisconnected, "Client is disconected");

            client.Stop();
            client = null;

            await tcs.Task;

            Assert.IsTrue(isDisconnected, "Client is still connected");
        }

        [TestMethod, Timeout(5000)]
        public async Task Pipe_Client_SendMessageTest()
        {
            const string Message = "Client's message";

            var tcs = new TaskCompletionSource<string>();

            var server = new PipeServer<string>();
            var client = new PipeClient<string>(server.ServerId);

            server.MessageReceivedEvent += (sender, args) =>
            {
                tcs.SetResult(args.Message);
            };

            server.Start();
            client.Start();

            var success = await client.SendMessageAsync(Message);
            Assert.IsTrue(success, "Send message failed");

            var message = await tcs.Task;

            Assert.AreEqual(Message, message, "Message are not equal");
        }

        [TestMethod, Timeout(5000)]
        public async Task Pipe_Client_SendMessage_LongMessageTest()
        {
            const int BufferSize = 2048;

            var tcs = new TaskCompletionSource<string>();

            var server = new PipeServer<string>();
            var client = new PipeClient<string>(server.ServerId);

            server.MessageReceivedEvent += (sender, args) =>
            {
                tcs.SetResult(args.Message);
            };

            server.Start();
            client.Start();

            var longString = $"[{new string('*', 10 * 2048)}]";

            Assert.IsTrue(Encoding.UTF8.GetByteCount(longString) > BufferSize, "Test string is smaller than needed buffer length");

            var success = await client.SendMessageAsync(longString);
            Assert.IsTrue(success, "Send message failed");

            var message = await tcs.Task;

            Assert.AreEqual(longString, message, "Messages are not equal");
        }
        
        [TestMethod, Timeout(5000)]
        public async Task Pipe_Client_SendMessage_MultiMessageTest()
        {
            const string FirstTestMessage = "Hi from first";
            const string SecondTestMessage = "Hi from second";

            var autoEvent = new AutoResetEvent(false);
            var message = string.Empty;

            var server = new PipeServer<string>();
            var client = new PipeClient<string>(server.ServerId);

            server.MessageReceivedEvent += (sender, args) =>
            {
                message = args.Message;
                autoEvent.Set();
            };

            server.Start();
            client.Start();

            var success = await client.SendMessageAsync(FirstTestMessage);
            Assert.IsTrue(success, "Send #1 message failed");

            autoEvent.WaitOne();
            Assert.AreEqual(FirstTestMessage, message, "#1 messages are not equal");

            success = await client.SendMessageAsync(SecondTestMessage);
            Assert.IsTrue(success, "Send #2 message failed");

            autoEvent.WaitOne();
            Assert.AreEqual(SecondTestMessage, message, "#2 messages are not equal");
        }
        
        [TestMethod, Timeout(10000)]
        public async Task Pipe_Client_SendMessage_MessageSpamTest()
        {
            const int MessageCount = PipeServer<string>.MaxNumberOfServerInstances / 2;

            var sendMessages = Enumerable.Range(0, MessageCount).Select(i => i.ToString()).ToList();
            var recievedMessages = new List<string>();

            var autoEvent = new AutoResetEvent(false);
            var message = string.Empty;

            var server = new PipeServer<string>();
            var client = new PipeClient<string>(server.ServerId);

            server.MessageReceivedEvent += (sender, args) =>
            {
                recievedMessages.Add(args.Message);

                if (recievedMessages.Count >= MessageCount)
                    autoEvent.Set();
            };

            server.Start();
            client.Start();

            await Task.WhenAll(sendMessages.Select(async i =>
            {
                var success = await client.SendMessageAsync(i);
                Assert.IsTrue(success, $"Send #{i} message failed");
            }));
            
            autoEvent.WaitOne();

            Assert.AreEqual(MessageCount, recievedMessages.Count, "Not all messages recieved");

            CollectionAssert.AreEquivalent(sendMessages, recievedMessages, "Send and recieved messages are not equal");
        }
        
        [TestMethod, Timeout(10000)]
        public async Task Pipe_Client_SendMessage_MultiClientSpamTest()
        {
            const int MessageCount = PipeServer<string>.MaxNumberOfServerInstances / 2;

            var sendMessages = Enumerable.Range(0, MessageCount).Select(i => i.ToString()).ToList();
            var recievedMessages = new List<string>();

            var autoEvent = new AutoResetEvent(false);
            var messages = new List<string>();

            var server = new PipeServer<string>();
            server.MessageReceivedEvent += (sender, args) =>
            {
                recievedMessages.Add(args.Message);

                if (recievedMessages.Count >= MessageCount)
                    autoEvent.Set();
            };

            server.Start();

            await Task.WhenAll(sendMessages.Select(async i =>
            {
                var client = new PipeClient<string>(server.ServerId);
                client.Start();

                var success = await client.SendMessageAsync(i);
                Assert.IsTrue(success, $"Send #{i} message failed");

                client.Stop();
            }));

            autoEvent.WaitOne();

            Assert.AreEqual(MessageCount, recievedMessages.Count, "Not all messages recieved");

            CollectionAssert.AreEquivalent(sendMessages, recievedMessages, "Send and recieved messages are not equal");
        }

        [TestMethod, Timeout(10000)]
        public async Task Pipe_Client_SendMessage_RestartedTest()
        {
            const string FirstMessage = "First";
            const string SecondMessage = "Second";

            var message = string.Empty;
            var autoEvent = new AutoResetEvent(false);

            var server = new PipeServer<string>();
            server.MessageReceivedEvent += (sender, args) =>
            {
                message = args.Message;

                autoEvent.Set();
            };

            server.Start();

            var client = new PipeClient<string>(server.ServerId);
            client.Start();

            await client.SendMessageAsync(FirstMessage);
            autoEvent.WaitOne();
            Assert.AreEqual(FirstMessage, message, "Message are not equal");

            client.Stop();
            client.Start();

            await client.SendMessageAsync(SecondMessage);
            autoEvent.WaitOne();
            Assert.AreEqual(SecondMessage, message, "Message are not equal");
        }
    }
}
