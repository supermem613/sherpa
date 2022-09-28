namespace Sherpa.Observers
{
    using System;

    /// <summary>
    /// Connection Observer.
    /// </summary>
    public abstract class ConnectionObserver
    {        
        /// <summary>
        /// Gets or sets the listener.
        /// </summary>
        /// <value>
        /// The listener.
        /// </value>
        public Route Server
        {
            protected get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public Connection Client
        {
            protected get;
            set;
        }

        /// <summary>
        /// Handles a receive buffer call.
        /// </summary>
        /// <param name="receiveBuffer">The receive buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <remarks>
        /// Should not throw!
        /// </remarks>
        public abstract void HandleReceive(byte[] receiveBuffer, int offset, int count);

        /// <summary>
        /// Handles the local disconnect.
        /// </summary>
        /// <remarks>
        /// Should not throw!
        /// </remarks>
        public abstract void HandleLocalDisconnect();

        /// <summary>
        /// Handles the remote disconnect.
        /// </summary>
        /// <remarks>
        /// Should not throw!
        /// </remarks>
        public abstract void HandleRemoteDisconnect();
    }
}
