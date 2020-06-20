using Iguagile;
using Iguagile.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IguagileTests
{
    [TestClass]
    public class IguagileClientTests
    {
        private const string BaseUri = "http://localhost/api/v1";
        private const string ApplicationName = "ApplicationName";
        private const string Version = "0.0.0";
        private const string Password = "******";
        private const int MaxUser = 20;

        private readonly byte[] _testData = Encoding.UTF8.GetBytes("test data");

        // iguaigle-engine server uses an implementation of RelayService.
        [TestMethod]
        [Timeout(10000)]
        public async Task SendBinary()
        {
            var room = await CreateRoom();
            Exception exception = null;

            var client = new IguagileClient();
            client.OnError += e => { exception = e; };
            client.OnConnected += () => _ = client.SendAsync(_testData);
            client.OnReceived += x =>
            {
                if (!_testData.SequenceEqual(x))
                {
                    var correctData = string.Join(", ", _testData);
                    var incorrectData = string.Join(", ", x);
                    exception = new Exception($"data is not match \n({correctData})\n({incorrectData})");
                }

                client.Dispose();
            };

            await client.StartAsync(room);
            if (exception is TaskCanceledException)
            {
                exception = null;
            }

            Assert.IsNull(exception, exception?.Message);
        }

        private async Task<Room> CreateRoom()
        {
            using var api = new RoomApiClient(BaseUri);
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
