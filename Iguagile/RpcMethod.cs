using System.Reflection;

namespace Iguagile
{
    public class RpcMethod
    {
        public object Receiver { get; }

        private MethodInfo method;

        public RpcMethod(MethodInfo method, object receiver)
        {
            this.method = method;
            Receiver = receiver;
        }

        public object Invoke(params object[] args)
        {
            return method.Invoke(Receiver, args);
        }
    }
}
