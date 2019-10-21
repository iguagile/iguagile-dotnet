using System.Runtime.Serialization;

namespace Iguagile.Api
{
    [DataContract]
    public class CreateRoomResponse : RoomApiResponse
    {
        [DataMember(Name = "result")]
        public Room Room { get; set; }
    }
}
