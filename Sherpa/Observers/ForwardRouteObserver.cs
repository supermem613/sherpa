namespace Sherpa.Observers
{
    using System;

    /// <summary>
    /// Forwards the listener calls to another listener.
    /// </summary>
    public class ForwardRouteObserver : RouteObserver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardRouteObserver"/> class.
        /// </summary>
        /// <param name="redirectHost">The redirect host.</param>
        /// <param name="redirectPort">The redirect port.</param>
        public ForwardRouteObserver(string redirectHost, int redirectPort)
        {
            this.RedirectHost = redirectHost;
            this.RedirectPort = redirectPort;
        }
        
        /// <summary>
        /// Gets the redirect host.
        /// </summary>
        public string RedirectHost
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the redirect port.
        /// </summary>
        public int RedirectPort
        {
            get;
            private set;
        }

        /// <summary>
        /// Handles the connection request.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="acceptedConnection">The accepted connection.</param>
        public override void HandleConnectionRequest(Logger logger, Connection acceptedConnection)
        {
            Connection redirectConnection = null;

            try
            {
                redirectConnection = Connection.Connect(
                    logger, 
                    this.RedirectHost, 
                    this.RedirectPort);
                
                logger.LogVerbose(
                    string.Format(
                        "Connected to Port Redirection Target {0}:{1}",
                        this.RedirectHost, 
                        this.RedirectPort));
                
                acceptedConnection.SetObserver(new ForwardConnectionObserver(redirectConnection, true));
                redirectConnection.SetObserver(new ForwardConnectionObserver(acceptedConnection, false));

                acceptedConnection.BeginReceiving();
                redirectConnection.BeginReceiving();
            }
            catch (Exception e) 
            {
                logger.LogError(
                    string.Format(
                        "Could not connect to target {0}:{1} on behalf of {2}. Reason: {3}",
                        this.RedirectHost,
                        this.RedirectPort,
                        acceptedConnection.RemoteEndPoint,
                        e));

                acceptedConnection.Close();

                if (redirectConnection != null)
                {
                    redirectConnection.Close();
                }
            }
        }
    }
}
