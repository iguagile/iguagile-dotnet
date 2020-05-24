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
        private System.Net.Sockets.TcpClient _client;
        private System.Net.Sockets.NetworkStream _stream;

        public bool IsConnected { get; private set; }

        public async Task ConnectAsync(string host, int port)
        {
            if (IsConnected)
            {
                throw new InvalidOperationException("Client is already started");
            }

            _client = new System.Net.Sockets.TcpClient();
            await _client.ConnectAsync(host, port);
            IsConnected = true;
            _stream = _client.GetStream();
        }

        public async Task<int> ReadAsync(byte[] buffer, CancellationToken token)
        {
            await _stream.ReadAsync(buffer, 0, 2, token);
            if (token.IsCancellationRequested)
            {
                return 0;
            }
            var size = BitConverter.ToUInt16(buffer, 0);
            var readSum = 0;
            var buf = new byte[size];
            while (readSum < size)
            {
                var readSize = await _stream.ReadAsync(buffer, readSum, size - readSum, token);
                if (token.IsCancellationRequested)
                {
                    return 0;
                }
                readSum += readSize;
            }

            return readSum;
        }

        public async Task SendAsync(byte[] data)
        {
            if (IsConnected && (_stream?.CanWrite ?? false))
            {
                var size = data.Length;
                var message = BitConverter.GetBytes((ushort) size);
                message = message.Concat(data).ToArray();
                await _stream.WriteAsync(message, 0, message.Length);
            }
        }

        public void Dispose()
        {
            _stream.Dispose();
            _client.Dispose();
            IsConnected = false;
        }
    }
}