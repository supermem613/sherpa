using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sherpa;

namespace Test
{
    [TestClass]
    public class FunctionalTest
    {
        [TestMethod]
        public void EndToEndTest()
        {
            MockSettings setting = new MockSettings();

            // 12999 -> 13000

            RouteSetting routeSetting = new RouteSetting("test", 12999);

            routeSetting.Destinations.Add(new KeyValuePair<string,int>("localhost", 13000));

            setting.RouteSettings.Add(routeSetting.Name, routeSetting);

            RouteManager routeManager = new RouteManager(setting);

            routeManager.Start();

            ManualResetEvent tcpClientConnected =  new ManualResetEvent(false);
            
            MockTcpServer server = new MockTcpServer(13000, 1, tcpClientConnected);
            server.Start();

            TcpClient client = new TcpClient();

            client.Connect("localhost", 12999);

            client.GetStream().WriteByte((byte)3);

            client.Close();

            tcpClientConnected.WaitOne(30000);

            server.Stop();

            Assert.AreEqual<int>(1, server.RequestCount);
            
            routeManager.Stop();
        }
    }
}
