namespace Sherpa.Observers
{
    using System;
    
    /// <summary>
    /// Forwards a connection.
    /// </summary>
    public class ForwardConnectionObserver : ConnectionObserver
    {
        /// <summary>
        /// The destination for the connection.
        /// </summary>
        private Connection destination;

        /// <summary>
        /// True if it is a client to listener connection.
        /// </summary>
        private bool clientToServer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardConnectionObserver"/> class.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="clientToServer">if set to <c>true</c> [is client to listener].</param>
        public ForwardConnectionObserver(Connection destination, bool clientToServer)
        {
            this.destination = destination;
            this.clientToServer = clientToServer;
        }

        /// <summary>
        /// Handles a receive buffer call.
        /// </summary>
        /// <param name="receiveBuffer">The receive buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        public override void HandleReceive(byte[] receiveBuffer, int offset, int count)
        {
            if (!this.destination.Send(receiveBuffer, offset, count))
            {
                this.Client.Close();
            }
        }

        /// <summary>
        /// Handles the local disconnect.
        /// </summary>
        public override void HandleLocalDisconnect()
        {
            this.destination.Close();
            this.destination = null;
        }

        /// <summary>
        /// Handles the remote disconnect.
        /// </summary>
        public override void HandleRemoteDisconnect()
        {
            this.destination.Close();
            this.destination = null;
        }
    }
}
