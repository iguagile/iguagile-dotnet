using System.Runtime.Serialization;

namespace Iguagile.Api
{
    [DataContract]
    public class CreateRoomRequest
    {
        [DataMember(Name = "password")]
        public string Password { get; set; }

        [DataMember(Name = "application_name")]
        public string ApplicationName { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }

        [DataMember(Name = "max_user")]
        public int MaxUser { get; set; }
    }
}
