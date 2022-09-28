namespace Sherpa.Observers
{
    using System;
    
    /// <summary>
    /// Responds with a canned response.
    /// </summary>
    public class CannedResponseConnectionObserver : ConnectionObserver
    {
        /// <summary>
        /// Canned response to be sent.
        /// </summary>
        private string cannedResponse;

        /// <summary>
        /// Initializes a new instance of the <see cref="CannedResponseConnectionObserver"/> class.
        /// </summary>
        /// <param name="cannedResponse">The canned response.</param>
        public CannedResponseConnectionObserver(string cannedResponse)
        {
            this.cannedResponse = cannedResponse;
        }

        /// <summary>
        /// Handles a receive buffer call.
        /// </summary>
        /// <param name="receiveBuffer">The receive buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        public override void HandleReceive(byte[] receiveBuffer, int offset, int count)
        {
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(this.cannedResponse);

            this.Client.Send(msg, 0, msg.Length);
            this.Client.Close();
        }

        /// <summary>
        /// Handles the local disconnect.
        /// </summary>
        public override void HandleLocalDisconnect()
        {
        }

        /// <summary>
        /// Handles the remote disconnect.
        /// </summary>
        public override void HandleRemoteDisconnect()
        {
        }
    }
}
