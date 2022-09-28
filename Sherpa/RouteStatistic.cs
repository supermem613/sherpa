namespace Sherpa
{
    using System;

    /// <summary>
    /// Route Statistic.
    /// </summary>
    public struct RouteStatistic
    {
        /// <summary>
        /// Gets or sets the connection count.
        /// </summary>
        /// <value>
        /// The connection count.
        /// </value>
        public ulong ConnectionCount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the last connection.
        /// </summary>
        /// <value>
        /// The last connection.
        /// </value>
        public DateTime LastConnection
        {
            get;
            set;
        }
    }
}
