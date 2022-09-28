namespace Sherpa
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Route Manager in the service.
    /// </summary>
    public class RouteManager
    {
        /// <summary>
        /// Connection Limit.
        /// </summary>
        private const int ConnectionLimit = 10000;

        /// <summary>
        /// Route object.
        /// </summary>
        private Dictionary<string, Route> routes;

        /// <summary>
        /// Settings object.
        /// </summary>
        private Settings settings;

        /// <summary>
        /// Synchronization object.
        /// </summary>
        private object syncObject;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RouteManager"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public RouteManager(Settings settings)
        {
            this.settings = settings;

            this.syncObject = new object();

            System.Net.ServicePointManager.DefaultConnectionLimit = RouteManager.ConnectionLimit;
            System.Net.ServicePointManager.MaxServicePoints = RouteManager.ConnectionLimit;

            this.settings.RoutesLoad += new Settings.RoutesLoadHandler(this.UpdateRoutes);
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            if (this.settings.RouteSettings == null)
            {
                return;
            }

            this.UpdateRoutes(this.settings.RouteSettings);
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            this.settings.Logger.LogInformation("Stopping all routes.");

            foreach (Route route in this.routes.Values)
            {
                route.Stop();
            }
        }

        /// <summary>
        /// Gets the routes statistics.
        /// </summary>
        /// <returns>Route statistics.</returns>
        public List<RouteStatistics> GetRouteStatistics()
        {
            lock (this.syncObject)
            {
                List<RouteStatistics> routeStatistics = new List<RouteStatistics>(this.routes.Count);

                foreach (Route route in this.routes.Values)
                {
                    if (route.Listening)
                    {
                        routeStatistics.Add(route.Statistics);
                    }
                }

                return routeStatistics;
            }
        }

        /// <summary>
        /// Updates the routes.
        /// </summary>
        /// <param name="newServers">The new routes.</param>
        private void UpdateRoutes(Dictionary<string, RouteSetting> newServers)
        {
            lock (this.syncObject)
            {
                this.settings.Logger.LogInformation("Loading routes configuration.");

                Dictionary<string, Route> newRouteList = new Dictionary<string, Route>(newServers.Count, StringComparer.InvariantCultureIgnoreCase);

                if (this.routes != null)
                {
                    foreach (Route route in this.routes.Values)
                    {
                        RouteSetting newSetting;

                        if (!newServers.TryGetValue(route.Setting.Name, out newSetting))
                        {
                            route.Stop();

                            continue;
                        }

                        switch (newSetting.Compare(route.Setting))
                        {
                            case RouteSetting.ServerSettingComparison.Same:

                                this.AddExistingRoute(newRouteList, route);
                                
                                break;

                            case RouteSetting.ServerSettingComparison.Different:

                                route.Stop();

                                this.AddNewRoute(newRouteList, newSetting);

                                break;

                            case RouteSetting.ServerSettingComparison.Reconcilable:

                                route.Update(newSetting);

                                newRouteList.Add(newSetting.Name, route);

                                break;
                        }
                    }
                }

                foreach (RouteSetting newServerSetting in newServers.Values)
                {
                    if (this.routes != null && this.routes.ContainsKey(newServerSetting.Name))
                    {
                        continue;
                    }

                    this.AddNewRoute(newRouteList, newServerSetting);
                }

                this.routes = newRouteList;
            }
        }

        /// <summary>
        /// Adds the new route.
        /// </summary>
        /// <param name="newRouteList">The new route list.</param>
        /// <param name="newSetting">The new setting.</param>
        private void AddNewRoute(Dictionary<string, Route> newRouteList, RouteSetting newSetting)
        {
            try
            {
                Route newRoute = new Route(this.settings.Logger, newSetting);

                newRouteList.Add(newSetting.Name, newRoute);

                newRoute.Start();
            }
            catch (ArgumentOutOfRangeException e)
            {
                this.settings.Logger.LogError(
                    string.Format(
                        "Failed to add route due to invalid port '{0}' : {1}",
                        newSetting.Port,
                        e));
            }
            catch (System.Net.Sockets.SocketException e)
            {
                this.settings.Logger.LogError(
                    string.Format(
                        "Failed to add route due to conflicting port '{0}' with another route or the configuration port : {1}",
                        newSetting.Port,
                        e));
            }
        }

        /// <summary>
        /// Adds the existring route.
        /// </summary>
        /// <param name="newRouteList">The new route list.</param>
        /// <param name="newSetting">The new setting.</param>
        private void AddExistingRoute(Dictionary<string, Route> newRouteList, Route route)
        {
            try
            {
                newRouteList.Add(route.Name, route);
            }
            catch (ArgumentOutOfRangeException e)
            {
                this.settings.Logger.LogError(
                    string.Format(
                        "Failed to add route due to invalid port '{0}' : {1}",
                        route.Setting.Port,
                        e));
            }
            catch (System.Net.Sockets.SocketException e)
            {
                this.settings.Logger.LogError(
                    string.Format(
                        "Failed to add route due to conflicting port '{0}' with another route or the configuration port : {1}",
                        route.Setting.Port,
                        e));
            }
        }
    }
}
