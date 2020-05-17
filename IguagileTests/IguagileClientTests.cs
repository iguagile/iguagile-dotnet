using Iguagile;
using Iguagile.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IguagileTests
{
    [TestClass]
    public class IguagileClientTests
    {
        private readonly string BaseUri = "http://localhost/api/v1";
        private readonly string ApplicationName = "ApplicationName";
        private readonly string Version = "0.0.0";
        private readonly string Password = "******";
        private readonly int MaxUser = 20;
        private readonly byte[] TestData = Encoding.UTF8.GetBytes("test data");


        // iguaigle-engine server uses an implementation of RelayService.
        [TestMethod]
        [Timeout(10000)]
        public async Task SendBinary()
        {
            var room = await CreateRoom();
            Exception exception = null;
            using (var client = new IguagileClient())
            {
                client.OnError += e =>
                {
                    exception = e;
                };

                client.OnConnected += () => _ = client.SendAsync(TestData);

                client.OnReceived += x =>
                {
                    if (!TestData.SequenceEqual(x))
                    {
                        var correctData = string.Join(", ", TestData);
                        var incorrectData = string.Join(", ", x);
                        exception = new Exception($"data is not match \n({correctData})\n({incorrectData})");
                    }

                    client.Disconnect();
                };

                await client.StartAsync(room);
            }

            Assert.IsNull(exception);
        }

        private async Task<Room> CreateRoom()
        {
            using (var api = new RoomApiClient(BaseUri))
            {
                var req = new CreateRoomRequest
                {
                    ApplicationName = ApplicationName,
                    Version = Version,
                    Password = Password,
                    MaxUser = MaxUser,
                };

                return await api.CreateRoomAsync(req);
            }
        }
    }
}
