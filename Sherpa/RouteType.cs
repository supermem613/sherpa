namespace Sherpa
{
    /// <summary>
    /// Route Types.
    /// </summary>
    public enum RouteType
    {
        /// <summary>
        /// Round robin to multiple machines.
        /// </summary>
        RoundRobin = 0,

        /// <summary>
        /// Forward to a single machine.
        /// </summary>
        Forward,
    }
}
