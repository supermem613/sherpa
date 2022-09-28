namespace Sherpa
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using Sherpa.Observers;

    /// <summary>
    /// Defines the route for incoming requests.
    /// </summary>
    public class Route
    {
        /// <summary>
        /// The Tcp listener.
        /// </summary>
        private TcpListener listener;
        
        /// <summary>
        /// Logger object.
        /// </summary>
        private Logger logger;

        /// <summary>
        /// Observer object.
        /// </summary>
        private RouteObserver observer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Route"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="setting">The setting.</param>
        public Route(Logger logger, RouteSetting setting)
        {
            this.logger = logger;
            this.Setting = setting;
            this.Name = this.Setting.Name;
            this.listener = new TcpListener(IPAddress.Any, this.Setting.Port);

            this.Statistics = new RouteStatistics(this);

            this.Load();
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Gets the local port.
        /// </summary>
        public int LocalPort
        {
            get
            {
                return ((IPEndPoint)this.listener.LocalEndpoint).Port;
            }
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
        /// Gets the setting.
        /// </summary>
        public RouteSetting Setting
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the statistics.
        /// </summary>
        public RouteStatistics Statistics
        {
            get;
            private set;
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            this.logger.LogInformation(
                string.Format(
                    "Stopping route {0}.",
                    this.Setting.Name));

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
            
            this.logger.LogInformation(
                string.Format(
                    "Starting route '{0}' with the following configuration:\r\n\r\nPort: {1}\r\nDestinations: {2}\r\nType: {3}",
                    this.Setting.Name,
                    this.Setting.Port,
                    this.Setting.Destination,
                    this.Setting.Type.ToString()));

            this.listener.Start();
            this.Listening = true;
            this.listener.BeginAcceptTcpClient(this.HandleConnectionRequest, null);
        }
        
        /// <summary>
        /// Updates the specified new setting.
        /// </summary>
        /// <param name="newSetting">The new setting.</param>
        public void Update(RouteSetting newSetting)
        {
            this.logger.LogInformation(
                string.Format(
                    "Reconfiguring route '{0}' with the following configuration:\r\n\r\nPort: {1}\r\nDestinations: {2}\r\nType: {3}",
                    newSetting.Name,
                    newSetting.Port,
                    newSetting.Destination,
                    newSetting.Type.ToString()));

            this.Setting = newSetting;
            this.Load();
        }
        
        /// <summary>
        /// Sets the observer.
        /// </summary>
        /// <param name="observer">The observer.</param>
        private void SetObserver(RouteObserver observer)
        {
            this.observer = observer;
            this.observer.ObservedRoute = this;
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

            Connection connection = Connection.Accept(this.logger, this.listener, asyncResult);
            
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
                this.Statistics.AddStatistic(connection.RemoteEndPoint.Address.ToString());
            }
            catch (System.Net.Sockets.SocketException)
            {
            }

            if (this.observer != null && connection != null)
            {
                this.observer.HandleConnectionRequest(this.logger, connection);
            }
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        private void Load()
        {
            switch (this.Setting.Type)
            {
                case RouteType.Forward:
                    this.SetObserver(new ForwardRouteObserver(this.Setting.Destinations[0].Key, this.Setting.Destinations[0].Value));
                    break;

                case RouteType.RoundRobin:
                    this.SetObserver(new RoundRobinRouteObserver(this.Setting.Destinations));
                    break;

                default:
                    throw new ArgumentException("ugh");
            }
        }
    }
}
