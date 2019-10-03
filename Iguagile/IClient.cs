using System;

namespace Iguagile
{
    public enum Protocol
    {
        Tcp
    }

    public interface IClient
    {
        event Action Open;
        event Action Close;
        event Action<byte[]> Received;
        event Action<Exception> OnError;

        bool IsConnected { get; }

        void Connect(string address, int port);
        void Disconnect();
        void Send(byte[] data);
    }
}
