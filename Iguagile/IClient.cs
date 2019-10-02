using System;

namespace Iguagile
{
    public delegate void ConnectionEventHandler();
    public delegate void ReceivedEventHandler(byte[] message);
    public delegate void ExceptionEventHandler(Exception exception);

    public enum Protocol
    {
        Tcp
    }

    public interface IClient
    {
        event ConnectionEventHandler Open;
        event ConnectionEventHandler Close;
        event ReceivedEventHandler Received;
        event ExceptionEventHandler OnError;

        bool IsConnected { get; }

        void Connect(string address, int port);
        void Disconnect();
        void Send(byte[] data);
    }
}
