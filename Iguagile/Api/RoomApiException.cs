using System;

namespace Iguagile.Api
{
    public class RoomApiException : Exception
    {
        public RoomApiException() { }

        public RoomApiException(string message) : base(message) { }
    }
}
