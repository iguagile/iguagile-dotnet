using System.Threading.Tasks;

namespace Iguagile
{
    public delegate void ConnectionEventHandler();
    public delegate void ReceivedEventHandler(byte[] message);

    public enum Protocol
    {
        Tcp
    }

    public interface IClient
    {
        event ConnectionEventHandler Open;
        event ConnectionEventHandler Close;
        event ReceivedEventHandler Received;

        bool IsConnect();
        Task ConnectAsync(string address, int port);
        void Disconnect();
        void Send(byte[] data);
    }
}
