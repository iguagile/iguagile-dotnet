using System;
using System.Threading.Tasks;

namespace Iguagile
{
    public enum Protocol
    {
        Tcp
    }

    public interface IClient : IDisposable
    {
        event Action OnConnected;
        event Action OnClosed;
        event Action<byte[]> OnReceived;
        event Action<Exception> OnError;

        bool IsConnected { get; }

        Task StartAsync(string address, int port);
        void Disconnect();
        Task SendAsync(byte[] data);
    }
}
