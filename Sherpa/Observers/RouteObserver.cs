namespace Sherpa.Observers
{
    using Sherpa.Observers;

    /// <summary>
    /// Route Observer Definition.
    /// </summary>
    public abstract class RouteObserver
    {
        /// <summary>
        /// Gets or sets the observed route.
        /// </summary>
        /// <value>
        /// The observed route.
        /// </value>
        public Route ObservedRoute
        {
            get;
            set;
        }

        /// <summary>
        /// Handles the connection request.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="acceptedConnection">The accepted connection.</param>
        /// <remarks>
        /// Should not throw!
        /// </remarks>
        public abstract void HandleConnectionRequest(Logger logger, Connection acceptedConnection);
    }
}
