namespace Test
{
    using Sherpa;
    using System.Collections.Generic;
    using System;
using System.Net.Sockets;
    using System.Net;
using System.Threading;

    /// <summary>
    /// A setting for this service.
    /// </summary>
    public class MockTcpServer
    {
        private TcpListener listener;

        private ManualResetEvent eventObject;

        public MockTcpServer(int port, int expected, ManualResetEvent eventObject)
        {
            this.Port = port;
            this.ExpectCount = expected;
            this.listener = new TcpListener(IPAddress.Any, port);
            this.eventObject = eventObject;
        }

        public int Port
        {
            get;
            private set;
        }

        public int ExpectCount
        {
            get;
            private set;
        }

        public int RequestCount
        {
            get;
            private set;
        }

        public void Reset()
        {
            this.RequestCount = 0;
        }

        public void Start()
        {
            this.listener.Start();
            this.listener.BeginAcceptTcpClient(this.HandleConnectionRequest, null);
        }

        public void Stop()
        {
        }

        /// <summary>
        /// Handles the connection request.
        /// </summary>
        /// <param name="asyncResult">The async result.</param>
        private void HandleConnectionRequest(IAsyncResult asyncResult)
        {
            this.RequestCount++;

            TcpClient client = this.listener.EndAcceptTcpClient(asyncResult);

            client.Close();

            if (this.RequestCount < this.ExpectCount)
            {
                this.listener.BeginAcceptTcpClient(this.HandleConnectionRequest, null);
            }
            else
            {
                this.listener.Stop();
                this.eventObject.Set();
            }
        }
    }
}
