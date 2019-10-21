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

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append($"RoomId={RoomId}, ");
            builder.Append($"RequirePassword={RequirePassword}, ");
            builder.Append($"Host={Server?.Host}, ");
            builder.Append($"Port={Server?.Port}, ");
            builder.Append($"MaxUser={MaxUser}, ");
            builder.Append($"ConnectedUser={ConnectedUser}, ");
            return builder.ToString();
        }
    }
}
