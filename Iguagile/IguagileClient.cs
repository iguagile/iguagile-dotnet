using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Iguagile
{
    public enum MessageType : byte
    {
        NewConnection,
        ExitConnection,
        Instantiate,
        Destroy,
        RequestObjectControlAuthority,
        TransferObjectControlAuthority,
        MigrateHost,
        Register,
        Transform,
        Rpc
    }

    public enum RpcTargets : byte
    {
        AllClients,
        OtherClients,
        AllClientsBuffered,
        OtherClientsBuffered,
        Host,
        Server
    }

    public class IguagileClient
    {
        private IClient _client;

        private Dictionary<int, User> _users = new Dictionary<int, User>();
        private Dictionary<string, object> _rpcMethods = new Dictionary<string, object>();

        public int UserId { get; private set; }
        public bool IsHost { get; private set; }
        
        public ConnectionEventHandler Open;
        public ConnectionEventHandler Close;

        public async Task ConnectAsync(string address, int port, Protocol protocol)
        {
            switch (protocol)
            {
                case Protocol.Tcp:
                    _client = new TcpClient();
                    break;
                default:
                    throw new ArgumentException("invalid protocol");
            }

            _client.Open += Open;
            _client.Close += Close;
            _client.Received += ClientReceived;
            await _client.ConnectAsync(address, port);
        }

        public void AddRpc(string methodName, object receiver)
        {
            _rpcMethods[methodName] = receiver;
        }

        public void RemoveRpc(object receiver)
        {
            var removeList = new List<string>();
            foreach (var rpcMethod in _rpcMethods)
            {
                if (ReferenceEquals(rpcMethod.Value, receiver))
                {
                    removeList.Add(rpcMethod.Key);
                }
            }

            foreach (var method in removeList)
            {
                _rpcMethods.Remove(method);
            }
        }

        public void Rpc(string methodName, RpcTargets target, params object[] args)
        {
            var objects = new object[] { methodName };
            objects = objects.Concat(args).ToArray();
            var data = Serialize(target, MessageType.Rpc, objects);
            Send(data);
        }

        private byte[] Serialize(RpcTargets target, MessageType messageType, params object[] message)
        {
            var serialized = LZ4MessagePackSerializer.Serialize(message);
            var data = new byte[] { (byte)target, (byte)messageType };
            return data.Concat(serialized).ToArray();
        }

        private void Send(byte[] data)
        {
            if (data.Length >= (1 << 16) - 16)
            {
                throw new Exception("too long data");
            }

            if (data.Length != 0)
            {
                _client.Send(data);
            }
        }

        private const int HeaderSize = 3;

        private void ClientReceived(byte[] message)
        {
            var id = BitConverter.ToInt16(message, 0) << 16;
            var messageType = (MessageType)message[2];
            switch (messageType)
            {
                case MessageType.Rpc:
                    InvokeRpc(message.Skip(HeaderSize).ToArray());
                    break;
                case MessageType.NewConnection:
                    AddUser(id);
                    break;
                case MessageType.ExitConnection:
                    RemoveUser(id);
                    break;
                case MessageType.MigrateHost:
                    MigrateHost();
                    break;
                case MessageType.Register:
                    Register(id);
                    break;
            }
        }

        private void InvokeRpc(byte[] data)
        {
            var objects = LZ4MessagePackSerializer.Deserialize<object[]>(data);
            var methodName = (string)objects[0];
            var args = objects.Skip(1).ToArray();
            if (!_rpcMethods.ContainsKey(methodName))
            {
                return;
            }

            var behaviour = _rpcMethods[methodName];
            var type = behaviour.GetType();
            var flag = BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var method = type.GetMethod(methodName, flag);
            method?.Invoke(behaviour, args);
        }

        private void AddUser(int id)
        {
            _users[id] = new User(id);
        }

        private void RemoveUser(int id)
        {
            _users.Remove(id);
        }

        private void MigrateHost()
        {
            IsHost = true;
        }

        private void Register(int id)
        {
            UserId = id;
        }
    }
}
