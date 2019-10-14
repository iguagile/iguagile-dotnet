using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Iguagile
{
    public class RpcMethod
    {
        public MethodInfo Method { get; set; }
        public object Receiver { get; set; }

        public RpcMethod(MethodInfo method, object receiver)
        {
            Method = method;
            Receiver = receiver;
        }
    }
}
