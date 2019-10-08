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
        event Action Open;
        event Action Close;
        event Action<byte[]> Received;
        event Action<Exception> OnError;

        bool IsConnected { get; }

        Task StartAsync(string address, int port);
        void Disconnect();
        Task SendAsync(byte[] data);
    }
}
