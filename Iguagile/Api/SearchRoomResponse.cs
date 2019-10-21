using System.Runtime.Serialization;

namespace Iguagile.Api
{
    [DataContract]
    public class SearchRoomResponse : RoomApiResponse
    {
        [DataMember(Name = "result")]
        public Room[] Rooms { get; set; }
    }
}
