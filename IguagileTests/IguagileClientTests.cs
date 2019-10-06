using System;
using System.Threading;
using Iguagile;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IguagileTests
{
    [TestClass]
    public class IguagileClientTests
    {
        [TestMethod]
        public void Connect_Tcp_WithValidAddress()
        {
            var address = "localhost";
            var port = 4000;
            
            var client = new IguagileClient();
            client.Open += () => client.Disconnect();
            var finish = false;
            client.Close += () => finish = true;
            client.OnError += e => Assert.ThrowsException<Exception>(() => throw e);
            client.Connect(address, port, Protocol.Tcp);
            while (!finish)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
