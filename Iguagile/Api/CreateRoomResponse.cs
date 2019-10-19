using System.Runtime.Serialization;

namespace Iguagile.Api
{
    [DataContract]
    public class CreateRoomResponse
    {
        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "room_id")]
        public int RoomId { get; set; }
    }
}
