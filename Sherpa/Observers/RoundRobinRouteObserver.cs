namespace Sherpa.Observers
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Forwards the listener calls to another listener.
    /// </summary>
    public class RoundRobinRouteObserver : RouteObserver
    {
        /// <summary>
        /// Last host it was round-robin to.
        /// </summary>
        private int lastHost;

        /// <summary>
        /// Maximum host it can be round-robin to.
        /// </summary>
        private int maxHost;

        /// <summary>
        /// Synchronization object.
        /// </summary>
        private object syncObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundRobinRouteObserver"/> class.
        /// </summary>
        /// <param name="redirectHosts">The redirect hosts.</param>
        public RoundRobinRouteObserver(List<KeyValuePair<string, int>> redirectHosts)
        {
            this.syncObject = new object();
            this.RedirectHosts = redirectHosts;
            this.maxHost = redirectHosts.Count;
        }
        
        /// <summary>
        /// Gets the redirect hosts.
        /// </summary>
        public List<KeyValuePair<string, int>> RedirectHosts
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
            KeyValuePair<string, int> redirectHost;

            lock (this.syncObject)
            {
                redirectHost = this.RedirectHosts[this.lastHost];

                this.lastHost = (this.lastHost + 1) % this.maxHost;
            }

            Connection redirectConnection = null;

            try
            {
                redirectConnection = Connection.Connect(
                    logger,
                    redirectHost.Key, 
                    redirectHost.Value);

                logger.LogVerbose(
                    string.Format(
                        "Connected to Port Redirection Target {0}:{1}",
                        redirectHost.Key,
                        redirectHost.Value));

                acceptedConnection.SetObserver(new ForwardConnectionObserver(redirectConnection, true));
                redirectConnection.SetObserver(new ForwardConnectionObserver(acceptedConnection, false));

                acceptedConnection.BeginReceiving();
                redirectConnection.BeginReceiving();
            }
            catch (Exception e)
            {
                logger.LogError(
                    string.Format(
                        "Could not connect to round-robin target {0}:{1} on behalf of {2}. Reason: {3}",
                        redirectHost.Key,
                        redirectHost.Value,
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
