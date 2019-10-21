using System.Runtime.Serialization;

namespace Iguagile.Api
{
    [DataContract]
    public class RoomCreateResponse : RoomApiResponse
    {
        [DataMember(Name = "result")]
        public Room Room { get; set; }
    }
}
