using Iguagile;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IguagileTests
{
    [TestClass]
    public class IguagileClientTests
    {
        private readonly string ServerAddress = "localhost";
        private readonly int PortTcp = 4000;

        [TestMethod]
        [Timeout(2000)]
        public async Task Connect_Tcp_WithValidAddress()
        {
            using var client = new IguagileClient();
            client.OnConnected += () => client.Disconnect();
            Exception exception = null;
            client.OnError += e => exception = e;
            await client.StartAsync(ServerAddress, PortTcp, Protocol.Tcp);
            if (exception != null)
            {
                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [Timeout(2000)]
        public async Task Binary()
        {
            var testData = System.Text.Encoding.UTF8.GetBytes("iguagile-dotnet");
            using var client = new IguagileClient();
            client.OnConnected += () => _ = client.SendBinaryAsync(testData);
            Exception exception = null;
            client.OnBinaryReceived += (id, data) =>
            {
                if (id != client.UserId)
                {
                    exception = new Exception("id is not match");
                }

                if (!data.SequenceEqual(testData))
                {
                    var correctData = string.Join(", ", testData);
                    var incorrectData = string.Join(", ", data);
                    exception = new Exception($"data is not match \n({correctData})\n({incorrectData})");
                }

                client.Disconnect();
            };
            client.OnError += e => exception = e;
            await client.StartAsync(ServerAddress, PortTcp, Protocol.Tcp);
            if (exception != null)
            {
                Assert.Fail(exception.Message);
            }
        }

        private readonly int ClientsNum = 3;

        [TestMethod]
        [Timeout(2000)]
        public void Rpc_OtherClients()
        {
            var exceptions = new Exception[ClientsNum];
            var idPairs = new IdPair[ClientsNum];
            var receivers = new RpcReceiver[ClientsNum];
            var clients = new IguagileClient[ClientsNum];
            var tasks = new Task[3];

            for (var i = 0; i < ClientsNum; i++)
            {
                var client = new IguagileClient();
                var index = i;
                client.OnError += e => exceptions[index] = e;
                var receiver = new RpcReceiver(client, ClientsNum - 1);
                receiver.OnIdEqual += (senderId, receiverId) => idPairs[index] = new IdPair(senderId, receiverId);
                client.AddRpc(nameof(RpcReceiver.RpcMethod), receiver);
                tasks[i] = client.StartAsync(ServerAddress, PortTcp, Protocol.Tcp);
                clients[i] = client;
                receivers[i] = receiver;
            }

            for (var i = 0; i < ClientsNum; i++)
            {
                _ = clients[i].Rpc(nameof(RpcReceiver.RpcMethod), RpcTargets.OtherClients, clients[i].UserId);
            }

            Task.WaitAll(tasks);

            for (var i = 0; i < ClientsNum; i++)
            {
                if (exceptions[i] != null)
                {
                    Assert.Fail(exceptions[i].Message);
                }

                if (idPairs[i] != null)
                {
                    Assert.Fail($"id is match {idPairs[i].SenderId}, {idPairs[i].ReceiverId}");
                }
            }
        }

        class IdPair
        {
            public int SenderId { get; }
            public int ReceiverId { get; }

            public IdPair(int senderId, int receiverId)
            {
                SenderId = senderId;
                ReceiverId = receiverId;
            }
        }

        class RpcReceiver
        {
            private readonly IguagileClient _client;
            private readonly int _otherClientsNum;
            private int _count;

            public event Action<int, int> OnIdEqual = delegate { };

            public RpcReceiver(IguagileClient client, int otherClientsNum)
            {
                _client = client;
                _otherClientsNum = otherClientsNum;
            }

            public void RpcMethod(int senderId)
            {
                if (senderId == _client.UserId)
                {
                    OnIdEqual(senderId, _client.UserId);
                    return;
                }

                _count++;
                if (_count == _otherClientsNum)
                {
                    _client.Disconnect();
                }
            }
        }
    }
}
