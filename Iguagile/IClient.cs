namespace Iguagile
{
    public delegate void ConnectionEventHandler();
    public delegate void ReceivedEventHandler(byte[] message);

    public interface IClient
    {
        event ConnectionEventHandler Open;
        event ConnectionEventHandler Close;
        event ReceivedEventHandler Received;

        bool IsConnect();
        void Connect(string address, int port);
        void Disconnect();
        void Send(byte[] data);
    }
}
