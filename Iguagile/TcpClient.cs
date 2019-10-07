using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Iguagile
{
    class TcpClient : IClient
    {
        private CancellationTokenSource _cts;
        private System.Net.Sockets.NetworkStream _stream;

        public event Action Open;
        public event Action Close;
        public event Action<byte[]> Received;
        public event Action<Exception> OnError;

        public bool IsConnected { get; private set; }

        public async Task StartAsync(string address, int port)
        {
            if(_cts != null)
            {
                throw new InvalidOperationException("Client is already started");
            }

            using (_cts = new CancellationTokenSource())
            using (var client = new System.Net.Sockets.TcpClient())
            {
                var token = _cts.Token;
                await Task.Run(async () =>
                {
                    try
                    {
                        await client.ConnectAsync(address, port);
                        IsConnected = true;
                        _stream = client.GetStream();
                        Open?.Invoke();

                        await ReceiveAsync(token);
                    }
                    catch (Exception exception)
                    {
                        OnError?.Invoke(exception);
                    }
                }, token);
            }

            IsConnected = false;
            _stream.Dispose();
            _cts = null;
            _stream = null;
            Close?.Invoke();
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

                Received?.Invoke(message);
            }
        }
    }
}
