namespace Sherpa.Observers
{
    using System;
    
    /// <summary>
    /// Responds with acanned response.
    /// </summary>
    public class CannedResponseRouteObserver : RouteObserver
    {
        /// <summary>
        /// Canned response.
        /// </summary>
        private string cannedResponse;

        /// <summary>
        /// Initializes a new instance of the <see cref="CannedResponseRouteObserver"/> class.
        /// </summary>
        /// <param name="cannedResponse">The canned response.</param>
        public CannedResponseRouteObserver(string cannedResponse)
        {
            this.cannedResponse = cannedResponse;
        }

        /// <summary>
        /// Handles the connection request.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="acceptedConnection">The accepted connection.</param>
        public override void HandleConnectionRequest(Logger logger, Connection acceptedConnection)
        {
            try
            {
                acceptedConnection.SetObserver(new CannedResponseConnectionObserver(this.cannedResponse));

                acceptedConnection.BeginReceiving();
            }
            catch (Exception e)
            {
                logger.LogError(
                    string.Format(
                        "Failed to respond with canned response: {0}",
                        e));
                
                acceptedConnection.Close();
            }
        }
    }
}
