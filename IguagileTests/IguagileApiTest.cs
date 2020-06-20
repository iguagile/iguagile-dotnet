using Iguagile.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace IguagileTests
{
    [TestClass]
    public class IguagileApiTest
    {
        private const string BaseUri = "http://localhost/api/v1";
        private const string ApplicationName = "ApplicationName";
        private const string Version = "0.0.0";
        private const string Password = "******";
        private const int MaxUser = 20;

        [TestMethod]
        public async Task CreateRoom()
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

                var room = await api.CreateRoomAsync(req);

                Assert.AreEqual(ApplicationName, room.ApplicationName);
                Assert.AreEqual(Version, room.Version);
                Assert.AreEqual(Password, room.Password);
                Assert.AreEqual(MaxUser, room.MaxUser);
                Assert.AreEqual(!string.IsNullOrEmpty(req.Password), room.RequirePassword);
            }
        }

        [TestMethod]
        public async Task SearchRoom()
        {
            using (var api = new RoomApiClient(BaseUri))
            {
                var req = new SearchRoomRequest
                {
                    ApplicationName = ApplicationName,
                    Version = Version,
                };

                var rooms = await api.SearchRoomAsync(req);

                Assert.AreEqual(rooms.Length, 1);

                var room = rooms[0];

                Assert.AreEqual(ApplicationName, room.ApplicationName);
                Assert.AreEqual(Version, room.Version);
                Assert.AreEqual(MaxUser, room.MaxUser);
                Assert.AreEqual(!string.IsNullOrEmpty(Password), room.RequirePassword);
            }
        }
    }
}
