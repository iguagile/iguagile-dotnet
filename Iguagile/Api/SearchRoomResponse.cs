using System.Runtime.Serialization;

namespace Iguagile.Api
{
    [DataContract]
    public class SearchRoomResponse
    {
        [DataMember(Name = "room_id")]
        public int RoomId { get; set; }

        [DataMember(Name = "is_set_password")]
        public bool IsSetPassword { get; set; }

        [DataMember(Name = "max_user")]
        public int MaxUser { get; set; }

        [DataMember(Name = "connected_user")]
        public int ConnectedUser { get; set; }
    }
}
