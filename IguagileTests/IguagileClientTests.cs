using Iguagile;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
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
        public void Connect_Tcp_WithValidAddress()
        {
            using (var client = new IguagileClient())
            {
                client.Open += () => client.Disconnect();
                var pool = new Semaphore(0, 1);
                client.Close += () => pool.Release(1);
                client.OnError += e => Assert.Fail(e.Message);
                _ = client.StartAsync(ServerAddress, PortTcp, Protocol.Tcp);
                pool.WaitOne();
            }
        }

        private readonly int ClientsNum = 3;

        [TestMethod]
        [Timeout(2000)]
        public async Task Rpc_OtherClients()
        {
            var receivers = new RpcReceiver[ClientsNum];
            var clients = new IguagileClient[ClientsNum];
            var poolOpen = new Semaphore(0, ClientsNum);
            var poolClose = new Semaphore(0, ClientsNum);

            for (var i = 0; i < ClientsNum; i++)
            {
                var client = new IguagileClient();
                client.Open += () => poolOpen.Release(1);
                client.Close += () => poolClose.Release(1);
                client.OnError += e => Assert.Fail(e.Message);
                var receiver = new RpcReceiver(client, ClientsNum - 1);
                client.AddRpc(nameof(RpcReceiver.RpcMethod), receiver);
                _ = client.StartAsync(ServerAddress, PortTcp, Protocol.Tcp);
                clients[i] = client;
                receivers[i] = receiver;
            }

            for (var i = 0; i < ClientsNum; i++)
            {
                poolOpen.WaitOne();
            }

            for (var i = 0; i < ClientsNum; i++)
            {
                await clients[i].Rpc(nameof(RpcReceiver.RpcMethod), RpcTargets.OtherClients, clients[i].UserId);
            }

            for (var i = 0; i < ClientsNum; i++)
            {
                poolClose.WaitOne();
            }
        }

        class RpcReceiver
        {
            private readonly IguagileClient _client;
            private readonly int _otherClientsNum;
            private int _count;

            public RpcReceiver(IguagileClient client, int otherClientsNum)
            {
                _client = client;
                _otherClientsNum = otherClientsNum;
            }

            public void RpcMethod(int senderId)
            {
                Assert.AreNotEqual(senderId, _client.UserId);
                _count++;
                if (_count == _otherClientsNum)
                {
                    _client.Disconnect();
                }
            }
        }
    }
}
