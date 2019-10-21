using System.Runtime.Serialization;

namespace Iguagile.Api
{
    [DataContract]
    public class Server
    {
        [DataMember(Name = "host")]
        public string Host { get; set; }

        [DataMember(Name = "port")]
        public int Port { get; set; }
    }
}
