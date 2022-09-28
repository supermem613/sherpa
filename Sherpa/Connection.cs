namespace Sherpa
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using Sherpa.Observers;

    /// <summary>
    /// Describes a connection to a remote listener.
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// Receive buffer size.
        /// </summary>
        private const int ReceiveBufferSize = 2 * 8192;

        /// <summary>
        /// Synchronization object.
        /// </summary>
        private static object syncObject = new object();

        /// <summary>
        /// TCP client.
        /// </summary>
        private TcpClient client;

        /// <summary>
        /// Receive buffer.
        /// </summary>
        private byte[] receiveBuffer;

        /// <summary>
        /// If true, we are receiving data.
        /// </summary>
        private bool receiving;

        /// <summary>
        /// If true, we are waiting to receive.
        /// </summary>
        private bool waitingToReceive;

        /// <summary>
        /// The connection observer.
        /// </summary>
        private ConnectionObserver observer;

        /// <summary>
        /// Logger object.
        /// </summary>
        private Logger logger;

        /// <summary>
        /// Prevents a default instance of the <see cref="Connection"/> class from being created.
        /// </summary>
        private Connection()
        {
            this.receiveBuffer = new byte[ReceiveBufferSize];
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Connection"/> is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if connected; otherwise, <c>false</c>.
        /// </value>
        public bool Connected
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the local end point.
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates the a connection.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="client">The Tcp client.</param>
        /// <returns>
        /// A connection object.
        /// </returns>
        public static Connection Create(Logger logger, TcpClient client)
        {
            Connection connection = new Connection();

            connection.client = client;
            connection.Connected = true;
            connection.receiving = false;
            connection.LocalEndPoint = client.Client.LocalEndPoint as IPEndPoint;
            connection.RemoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            connection.logger = logger;

            return connection;
        }

        /// <summary>
        /// Accepts the specified connection.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="server">The server.</param>
        /// <param name="asyncResult">The async result.</param>
        /// <returns>
        /// A connection object.
        /// </returns>
        public static Connection Accept(Logger logger, TcpListener server, IAsyncResult asyncResult)
        {
            TcpClient client = server.EndAcceptTcpClient(asyncResult);
            return Connection.Create(logger, client);
        }

        /// <summary>
        /// Connects to the specified remote host.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="remoteHost">The remote host.</param>
        /// <param name="remotePort">The remote port.</param>
        /// <returns>
        /// A connection object.
        /// </returns>
        public static Connection Connect(Logger logger, string remoteHost, int remotePort)
        {
            TcpClient client = new TcpClient();
            client.Connect(remoteHost, remotePort);

            return Connection.Create(logger, client);
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            this.logger.LogVerbose(
                string.Format(
                    "Connection.Close invoked by client code: {0}", 
                    this));

            lock (Connection.syncObject)
            {
                if (!this.Connected)
                {
                    return;
                }
            
                this.Connected = false;

                this.Close(true);
            }
        }
                
        /// <summary>
        /// Begins receiving.
        /// </summary>
        public void BeginReceiving()
        {
            this.logger.LogVerbose(
                string.Format(
                    "{0} --> BeginReceiving() Invoked",
                    this));

            lock (Connection.syncObject)
            {
                if (!this.Connected || this.receiving || this.waitingToReceive)
                {
                    return;
                }
            }

            this.receiving = true;
            this.BeginReceivingInternal();
        }

        /// <summary>
        /// Stops receiving.
        /// </summary>
        public void StopReceiving()
        {
            this.logger.LogVerbose(
                string.Format(
                    "{0} --> StopReceiving() Invoked",
                    this));

            lock (Connection.syncObject)
            {
                if (!this.Connected || !this.receiving)
                {
                    return;
                }
            }
            
            this.receiving = false;
        }

        /// <summary>
        /// Sends the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>If true, the send was successful.</returns>
        public bool Send(byte[] buffer, int offset, int count)
        {
            lock (Connection.syncObject)
            {
                if (!this.Connected)
                {
                    return false;
                }

                try
                {
                    this.client.GetStream().Write(buffer, offset, count);

                    this.logger.LogVerbose(
                        string.Format(
                            "{0} --> Sent {1} bytes of data to destination",
                            this,
                            count));
                }
                catch (System.IO.IOException e)
                {
                    this.logger.LogVerbose(
                        string.Format(
                            "{0} --> Send() Failed with: \r\n\r\n {1}",
                            this,
                            e));

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Sets the observer.
        /// </summary>
        /// <param name="observer">The observer.</param>
        public void SetObserver(ConnectionObserver observer)
        {
            this.observer = observer;
            if (this.observer != null)
            {
                this.observer.Client = this;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0} - {1}", this.LocalEndPoint, this.RemoteEndPoint);
        }

        /// <summary>
        /// Closes the specified notify observer.
        /// </summary>
        /// <param name="notifyObserver">if set to <c>true</c> [notify observer].</param>
        private void Close(bool notifyObserver)
        {
            if (this.client == null)
            {
                return;
            }

            this.logger.LogVerbose(
                string.Format(
                    "{0} --> Close(bool) Invoked with values {1}",
                    this,
                    notifyObserver));

            this.Connected = false;
            this.client.Close();
            this.client = null;

            if (notifyObserver && this.observer != null)
            {
                this.observer.HandleLocalDisconnect();
            }
        }

        /// <summary>
        /// Handles a receive request.
        /// </summary>
        /// <param name="asyncResult">The async result.</param>
        private void HandleReceive(IAsyncResult asyncResult)
        {
            lock (Connection.syncObject)
            {
                if (!this.Connected)
                {
                    return;
                }

                this.waitingToReceive = false;

                int readBytes = 0;

                try
                {
                    readBytes = this.client.GetStream().EndRead(asyncResult);
                }
                catch (System.IO.IOException)
                {
                }

                if (readBytes == 0)
                {
                    this.logger.LogVerbose(
                        string.Format(
                            " *** CLOSE *** Read Bytes = 0: {0}",
                            this));

                    if (this.observer != null)
                    {
                        this.observer.HandleRemoteDisconnect();
                    }

                    this.Close(false);
                    return;
                }

                this.logger.LogVerbose(
                    string.Format(
                        " *** RCV *** Read Bytes = {0} Connection: {1}", 
                        readBytes, 
                        this));

                if (this.observer != null)
                {
                    this.observer.HandleReceive(this.receiveBuffer, 0, readBytes);
                }

                this.BeginReceivingInternal();
            }
        }
                
        /// <summary>
        /// Begins the receiving internal.
        /// </summary>
        private void BeginReceivingInternal()
        {
            this.logger.LogVerbose(
                 string.Format(
                     "{0} --> BeginReceivingInternal() Invoked", 
                     this));

            lock (Connection.syncObject)
            {
                if (!this.Connected || !this.receiving || this.waitingToReceive)
                {
                    return;
                }

                try
                {
                    this.client.GetStream().BeginRead(this.receiveBuffer, 0, ReceiveBufferSize, this.HandleReceive, null);
                    this.waitingToReceive = true;
                }
                catch (System.IO.IOException)
                {
                    this.receiving = false;
                    this.waitingToReceive = false;
                    this.Close(false);
                }
            }
        }
    }
}
