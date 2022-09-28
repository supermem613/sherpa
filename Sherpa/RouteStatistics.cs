namespace Sherpa
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Route Statistics.
    /// </summary>
    public class RouteStatistics
    {
        /// <summary>
        /// Route statistics.
        /// </summary>
        private Dictionary<string, RouteStatistic> routeStatistics;

        /// <summary>
        /// Synchronization object.
        /// </summary>
        private object syncObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteStatistics"/> class.
        /// </summary>
        /// <param name="route">The route.</param>
        public RouteStatistics(Route route)
        {
            this.Route = route;
            this.routeStatistics = new Dictionary<string, RouteStatistic>(StringComparer.InvariantCultureIgnoreCase);
            this.syncObject = new object();
        }

        /// <summary>
        /// Gets the route.
        /// </summary>
        public Route Route
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the connection statistics.
        /// </summary>
        public Dictionary<string, RouteStatistic> ConnectionStatistics
        {
            get
            {
                lock (this.syncObject)
                {
                    return new Dictionary<string, RouteStatistic>(this.routeStatistics);
                }
            }
        }

        /// <summary>
        /// Adds the statistic.
        /// </summary>
        /// <param name="incomingServer">The incoming server.</param>
        public void AddStatistic(string incomingServer)
        {
            lock (this.syncObject)
            {
                RouteStatistic value;

                if (this.routeStatistics.TryGetValue(incomingServer, out value))
                {
                    value.ConnectionCount++;
                }
                else
                {
                    value.ConnectionCount = 1;
                }

                value.LastConnection = DateTime.Now;
                this.routeStatistics[incomingServer] = value;                
            }
        }
    }
}
