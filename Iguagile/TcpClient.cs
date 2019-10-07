using System;
using System.Linq;
using System.Threading.Tasks;

namespace Iguagile
{
    class TcpClient : IClient
    {
        private System.Net.Sockets.TcpClient _client;
        private System.Net.Sockets.NetworkStream _stream;

        public event Action Open;
        public event Action Close;
        public event Action<byte[]> Received;
        public event Action<Exception> OnError;

        public bool IsConnected => _client?.Connected ?? false;

        public void Connect(string address, int port)
        {
            _client = new System.Net.Sockets.TcpClient(address, port);
            _stream = _client.GetStream();
            Open?.Invoke();
            var messageSize = new byte[2];
            Task.Run(() =>
            {
                try
                {
                    while (IsConnected)
                    {
                        _stream.Read(messageSize, 0, 2);
                        var size = BitConverter.ToUInt16(messageSize, 0);
                        var readSum = 0;
                        var buf = new byte[size];
                        var message = new byte[0];
                        while (readSum < size)
                        {
                            var readSize = _stream.Read(buf, 0, size - readSum);
                            message = message.Concat(buf.Take(readSize)).ToArray();
                            readSum += readSize;
                        }

                        Received?.Invoke(message);
                    }
                }
                catch (Exception exception)
                {
                    OnError?.Invoke(exception);
                }
            });

            Disconnect();
            Close?.Invoke();
        }

        public void Disconnect()
        {
            if (IsConnected)
            _stream?.Dispose();
            _client?.Dispose();
                _client.Close();
            _stream = null;
            _client = null;
        }

        public void Send(byte[] data)
        {
            if (IsConnected && (_stream?.CanWrite ?? false))
            {
                var size = data.Length;
                var message = BitConverter.GetBytes((ushort)size);
                message = message.Concat(data).ToArray();
                _stream.Write(message, 0, message.Length);
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
