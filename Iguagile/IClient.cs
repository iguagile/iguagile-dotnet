using System;
using System.Threading.Tasks;
using Iguagile.Api;

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
        Task StartAsync(Room room);
        void Disconnect();
        Task SendAsync(byte[] data);
    }
}
