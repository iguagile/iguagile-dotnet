using System;
using System.Threading;
using System.Threading.Tasks;

namespace Iguagile
{
    public interface IClient : IDisposable
    {
        bool IsConnected { get; }

        Task ConnectAsync(string host, int port);
        Task<int> ReadAsync(byte[] buffer, CancellationToken token);
        Task SendAsync(byte[] data);
    }
}
