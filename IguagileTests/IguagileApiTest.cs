using Iguagile.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace IguagileTests
{
    [TestClass]
    public class IguagileApiTest
    {
        private readonly string BaseUri = "http://localhost/api/v1";

        [TestMethod]
        public async Task CreateRoom()
        {
            using (var api = new RoomApiClient(BaseUri))
            {
                var req = new CreateRoomRequest
                {
                    ApplicationName = "appname",
                    Version = "0.0.0",
                    Password = "******",
                    MaxUser = 20,
                };

                var room = await api.CreateRoomAsync(req);

                Assert.AreEqual(req.ApplicationName, room.ApplicationName);
                Assert.AreEqual(req.Version, room.Version);
                Assert.AreEqual(req.Password, room.Password);
                Assert.AreEqual(req.MaxUser, room.MaxUser);
                Assert.AreEqual(!string.IsNullOrEmpty(req.Password), room.RequirePassword);
            }
        }
    }
}
