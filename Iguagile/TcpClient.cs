using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iguagile.Api;

namespace Iguagile
{
    class TcpClient : IClient
    {
        private CancellationTokenSource _cts;
        private System.Net.Sockets.NetworkStream _stream;

        public event Action OnConnected = delegate { };
        public event Action OnClosed = delegate { };
        public event Action<byte[]> OnReceived = delegate { };
        public event Action<Exception> OnError = delegate { };

        public bool IsConnected { get; private set; }

        public async Task StartAsync(Room room)
        {
            if(_cts != null)
            {
                throw new InvalidOperationException("Client is already started");
            }

            using (_cts = new CancellationTokenSource())
            using (var client = new System.Net.Sockets.TcpClient())
            {
                var token = _cts.Token;
                try
                {
                    await client.ConnectAsync(room.Server.Host, room.Server.Port);
                    IsConnected = true;
                    _stream = client.GetStream();
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

            IsConnected = false;
            _stream.Dispose();
            _cts = null;
            _stream = null;
            OnClosed();
        }

        public void Disconnect()
        {
            _cts?.Cancel();
        }

        public async Task SendAsync(byte[] data)
        {
            if (IsConnected && (_stream?.CanWrite ?? false))
            {
                var size = data.Length;
                var message = BitConverter.GetBytes((ushort)size);
                message = message.Concat(data).ToArray();
                await _stream.WriteAsync(message, 0, message.Length);
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        private async Task ReceiveAsync(CancellationToken token)
        {
            var messageSize = new byte[2];
            while (true)
            {
                if (token.IsCancellationRequested) {
                    return;
                }

                await _stream.ReadAsync(messageSize, 0, 2);

                var size = BitConverter.ToUInt16(messageSize, 0);
                var readSum = 0;
                var buf = new byte[size];
                var message = new byte[0];
                while (readSum < size)
                {
                    var readSize = await _stream.ReadAsync(buf, 0, size - readSum);
                    message = message.Concat(buf.Take(readSize)).ToArray();
                    readSum += readSize;
                }

                OnReceived(message);
            }
        }
    }
}
