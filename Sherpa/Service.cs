namespace Sherpa
{
    using System.ServiceProcess;

    /// <summary>
    /// Service implementation.
    /// </summary>
    public partial class Service : ServiceBase
    {
        /// <summary>
        /// Route object.
        /// </summary>
        private RouteManager routeManager;

        /// <summary>
        /// Configuration Server.
        /// </summary>
        private ConfigurationServer configurationServer;

        /// <summary>
        /// Service settings;
        /// </summary>
        private Settings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        public Service()
        {
            this.InitializeComponent();

            this.settings = new Settings();
            this.routeManager = new RouteManager(this.settings);
            this.configurationServer = new ConfigurationServer(this.settings, this.routeManager);
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            this.configurationServer.Start();
            this.routeManager.Start();
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            this.routeManager.Stop();
            this.configurationServer.Stop();
        }
    }
}
