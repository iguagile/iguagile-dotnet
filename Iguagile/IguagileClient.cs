using Iguagile.Api;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iguagile
{
    public class IguagileClient : IDisposable
    {
        private CancellationTokenSource _cts;
        private IClient _client;

        public bool IsConnected => _client?.IsConnected ?? false;

        public event Action OnConnected = delegate { };
        public event Action OnClosed = delegate { };
        public event Action<Exception> OnError = delegate { };
        public event Action<byte[]> OnReceived = delegate { };

        public async Task StartAsync(Room room)
        {
            if (_cts != null)
            {
                throw new InvalidOperationException("Client is already started");
            }

            using (_client = new TcpClient())
            using (_cts = new CancellationTokenSource())
            {
                var token = _cts.Token;
                try
                {
                    await _client.ConnectAsync(room.Server.Host, room.Server.Port);
                    var roomId = BitConverter.GetBytes(room.RoomId);
                    await SendAsync(roomId);
                    var applicationName = Encoding.UTF8.GetBytes(room.ApplicationName);
                    await SendAsync(applicationName);
                    var version = Encoding.UTF8.GetBytes(room.Version);
                    await SendAsync(version);
                    var password = Encoding.UTF8.GetBytes(room.Password);
                    await SendAsync(password);
                    if (!string.IsNullOrEmpty(room.Token))
                    {
                        var roomToken = Convert.FromBase64String(room.Token);
                        await SendAsync(roomToken);
                    }

                    OnConnected();

                    await ReceiveAsync(token);
                }
                catch (Exception exception)
                {
                    OnError(exception);
                }
            }

            _cts = null;
            OnClosed();
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

        private async Task ReceiveAsync(CancellationToken token)
        {
            while (true)
            {
                var bufsize = 1024;
                var buf = new byte[bufsize];
                var n = await _client.ReadAsync(buf, token);
                if (token.IsCancellationRequested)
                {
                    return;
                }

                OnReceived(buf.Take(n).ToArray());
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
        }
    }
}
