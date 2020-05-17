using Iguagile.Api;
using System;
using System.Threading.Tasks;

namespace Iguagile
{
    public class IguagileClient : IDisposable
    {
        private IClient _client;

        public bool IsConnected => _client?.IsConnected ?? false;

        public event Action OnConnected = delegate { };
        public event Action OnClosed = delegate { };
        public event Action<Exception> OnError = delegate { };
        public event Action<byte[]> OnReceived = delegate { };

        public async Task StartAsync(Room room)
        {
            _client = new TcpClient();
            _client.OnReceived += OnReceived;
            _client.OnConnected += OnConnected;
            _client.OnClosed += OnClosed;
            _client.OnError += OnError;
            await _client.StartAsync(room);
        }

        public async Task SendAsync(byte[] data)
        {
            if (data.Length >= (1 << 16) - 16)
            {
                throw new Exception("too long data");
            }

            if (data.Length != 0)
            {
                await _client.SendAsync(data);
            }
        }
        public void Disconnect()
        {
            if (_client != null)
            {
                _client.Disconnect();
                _client = null;
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

    }
}
