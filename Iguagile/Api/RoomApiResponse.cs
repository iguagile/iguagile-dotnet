using System.Runtime.Serialization;

namespace Iguagile.Api
{
    [DataContract]
    public class RoomApiResponse
    {
        [DataMember(Name = "success")]
        public bool Success { get; set; }

        [DataMember(Name = "error")]
        public string Error { get; set; }
    }
}
