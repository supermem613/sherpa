namespace Sherpa
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using Sherpa.Observers;

    /// <summary>
    /// Configuration route.
    /// </summary>
    public class ConfigurationServer
    {
        /// <summary>
        /// The Tcp listener.
        /// </summary>
        private TcpListener listener;

        /// <summary>
        /// Settings object.
        /// </summary>
        private Settings settings;

        /// <summary>
        /// Version string.
        /// </summary>
        private string version;

        /// <summary>
        /// Cached Header.
        /// </summary>
        private string header;

        /// <summary>
        /// Last Updated.
        /// </summary>
        private DateTime lastUpdatedRoutes;

        /// <summary>
        /// Cached routes information.
        /// </summary>
        private string routesInformation;

        /// <summary>
        /// Last Updated.
        /// </summary>
        private DateTime lastUpdatedEvents;

        /// <summary>
        /// Cached events information.
        /// </summary>
        private string eventsInformation;

        /// <summary>
        /// Cached footer.
        /// </summary>
        private string footer;

        /// <summary>
        /// Route manager.
        /// </summary>
        private RouteManager routeManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationServer"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="routeManager">The route manager.</param>
        public ConfigurationServer(Settings settings, RouteManager routeManager)
        {
            this.settings = settings;
            this.Port = this.settings.ConfigurationPort;
            this.listener = new TcpListener(IPAddress.Any, this.Port);
            this.version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            this.routeManager = routeManager;

            this.settings.ConfigurationPortChange += new Settings.ConfigurationPortChangeHandler(this.UpdateConfigurationServer);
        }

        /// <summary>
        /// Gets the port.
        /// </summary>
        public int Port
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is listening.
        /// </summary>
        /// <value>
        /// <c>True</c> if this instance is listening; otherwise, <c>false</c>.
        /// </value>
        public bool Listening
        {
            get;
            private set;
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            this.Listening = false;
            this.listener.Stop();
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            if (this.Listening)
            {
                return;
            }
            
            this.listener.Start();
            this.Listening = true;
            this.listener.BeginAcceptTcpClient(this.HandleConnectionRequest, null);
        }

        /// <summary>
        /// Updates the configuration route.
        /// </summary>
        /// <param name="port">The port.</param>
        private void UpdateConfigurationServer(int port)
        {
            if (this.Port != port)
            {
                this.Stop();

                this.Port = port;
                this.listener = new TcpListener(IPAddress.Any, this.Port);

                this.Start();
            }
        }

        /// <summary>
        /// Handles the connection request.
        /// </summary>
        /// <param name="asyncResult">The async result.</param>
        private void HandleConnectionRequest(IAsyncResult asyncResult)
        {
            if (!this.Listening)
            {
                return;
            }

            Connection connection = Connection.Accept(this.settings.Logger, this.listener, asyncResult);

            try
            {
                this.listener.BeginAcceptTcpClient(this.HandleConnectionRequest, null);
            }
            catch (System.Net.Sockets.SocketException)
            {
                return;
            }

            try
            {
                CannedResponseRouteObserver observer = new CannedResponseRouteObserver(this.GetResponse());

                observer.HandleConnectionRequest(this.settings.Logger, connection);
            }
            catch (Exception e)
            {
                this.settings.Logger.LogError(
                    string.Format(
                    "Fail to return configuration: ",
                    e));
                
               connection.Close();
            }
        }

        /// <summary>
        /// Gets the response.
        /// </summary>
        /// <returns>Response string.</returns>
        private string GetResponse()
        {
            StringBuilder sb = new StringBuilder(2 * 8192);

            if (this.header == null)
            {
                this.header = this.GenerateHeader();
            }

            sb.Append(this.header);

            if (this.lastUpdatedRoutes < this.settings.LastUpdated)
            {
                this.routesInformation = this.GenerateRoutesInformation();
                this.lastUpdatedRoutes = this.settings.LastUpdated;
            }

            sb.Append(this.routesInformation);
            
            this.GenerateRouteStatistics(sb);

            if (this.lastUpdatedEvents < this.settings.Logger.LastLogged)
            {
                this.eventsInformation = this.GenerateEventsInformation();
                this.lastUpdatedEvents = this.settings.Logger.LastLogged;
            }

            sb.Append(this.eventsInformation);

            if (this.footer == null)
            {
                this.footer = this.GenerateFooter();
            }

            sb.Append(this.footer);

            return sb.ToString();
        }

        /// <summary>
        /// Generates the route statistics.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        private void GenerateRouteStatistics(StringBuilder sb)
        {
            sb.Append("<h3>Route Statistics</h3>");

            sb.Append("<table border=1><tbody><tr><th>Route</th><th>Statistics</th></tr>");

            foreach (RouteStatistics stats in this.routeManager.GetRouteStatistics())
            {
                sb.Append("<tr><td>");
                sb.Append(stats.Route.Name);

                if (stats.ConnectionStatistics.Count == 0)
                {
                    sb.Append("</td><td>no connections</td></tr>");
                }
                else
                {
                    sb.Append("</td><td><table border=1><tbody><tr><th>Connection Source</th><th>Connection Count</th><th>Last Connection Time</th></tr>");

                    foreach (KeyValuePair<string, RouteStatistic> stat in stats.ConnectionStatistics)
                    {
                        sb.Append("<tr><td>");
                        sb.Append(stat.Key);
                        sb.Append("</td><td>");
                        sb.Append(stat.Value.ConnectionCount);
                        sb.Append("</td><td>");
                        sb.Append(stat.Value.LastConnection);
                        sb.Append("</td></tr>");
                    }

                    sb.Append("</tbody></table></td></tr>");
                }
            }

            sb.Append("</tbody></table>");
        }

        /// <summary>
        /// Generates the footer.
        /// </summary>
        /// <returns>The footer.</returns>
        private string GenerateFooter()
        {
            StringBuilder sb = new StringBuilder(4096);

            sb.Append("</br>");

            sb.AppendFormat("Sherpa version: {0}", this.version);

            sb.Append("</body></html>");

            return sb.ToString();
        }

        /// <summary>
        /// Generates the header.
        /// </summary>
        /// <returns>The header.</returns>
        private string GenerateHeader()
        {
            StringBuilder sb = new StringBuilder(4096);

            sb.Append("<html><title>");
            sb.Append(System.Environment.MachineName);
            sb.Append(" : Sherpa (");
            sb.Append(this.version);
            sb.Append(")</title>");

            sb.AppendLine("<head><style type=\"text/css\">");

            sb.AppendLine("body{font-family: Arial, Helvetica, sans-serif;}");
            sb.AppendLine("h1{padding:15px 0px 0 0;margin:0px;color:#484848;font-weight:bold; font-size:28px;}");
            sb.AppendLine("table, th, td{border-collapse:collapse; border: 1px solid black;}");
            sb.AppendLine("th{text-align:left;}");
            sb.AppendLine("td, th{padding:5px; font-size:14px;}");

            sb.AppendLine("</style></head>");

            return sb.ToString();
        }

        /// <summary>
        /// Generates the events information.
        /// </summary>
        /// <returns>Event information.</returns>
        private string GenerateEventsInformation()
        {
            StringBuilder sb = new StringBuilder(8192);

            sb.Append("<h3>Last events</h3>");

            sb.Append("<table border=1><tbody><tr><th>Timestamp</th><th>Level</th><th>Message</th></tr>");

            foreach (LogEntry entry in this.settings.Logger.GetEvents())
            {
                sb.Append("<tr><td>");
                sb.Append(entry.DateTime);
                sb.Append("</td><td>");

                switch (entry.EventLogEntryType)
                {
                    case EventLogEntryType.Error:
                        sb.Append("<font color=\"red\">Error</font>");
                        break;

                    case EventLogEntryType.Warning:
                        sb.Append("<font color=\"yellow\">Warning</font>");
                        break;

                    default:
                        sb.Append(entry.EventLogEntryType.ToString());
                        break;
                }

                sb.Append("</td><td>");
                sb.Append(entry.Message.Replace("\r\n", "<br/>"));
                sb.Append("</td></tr>");
            }

            sb.Append("</tbody></table>");

            return sb.ToString();
        }

        /// <summary>
        /// Generates the routes information.
        /// </summary>
        /// <returns>Route information.</returns>
        private string GenerateRoutesInformation()
        {
            StringBuilder sb = new StringBuilder(8192);

            sb.Append("<body><h3>Routes</h3>");

            sb.Append("<table border=1><tbody><tr><th>Name</th><th>Port</th><th>Destination</th></tr>");

            foreach (RouteSetting settings in this.settings.RouteSettings.Values)
            {
                sb.Append("<tr><td>");
                sb.Append(settings.Name);
                sb.Append("</td><td>");
                sb.AppendFormat("{0}", settings.Port);
                sb.Append("</td><td>");
                sb.Append(settings.Destination);
                sb.Append("</td></tr>");
            }

            sb.Append("</tbody></table>");

            return sb.ToString();
        }
    }
}
