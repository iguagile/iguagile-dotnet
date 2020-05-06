using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Iguagile.Api
{
    [DataContract]
    public class Room
    {
        [DataMember(Name = "room_id")]
        public int RoomId { get; set; }

        [DataMember(Name = "require_password")]
        public bool RequirePassword { get; set; }

        [DataMember(Name = "server")]
        public Server Server { get; set; }

        [DataMember(Name = "max_user")]
        public int MaxUser { get; set; }

        [DataMember(Name = "connected_user")]
        public int ConnectedUser { get; set; }

        [DataMember(Name = "token")]
        public string Token { get; set; }

        [DataMember(Name = "information")]
        public Dictionary<string, string> Information { get; set; }

        public string ApplicationName{ get; set; }

        public string Version { get; set; }

        public string Password { get; set; }

        public override string ToString()
        {
            return $"RoomId={RoomId}, RequirePassword={RequirePassword}, Host={Server?.Host}, Port={Server?.Port}, MaxUser={MaxUser}, ConnectedUser={ConnectedUser}, ";
        }
    }
}
