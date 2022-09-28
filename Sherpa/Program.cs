namespace Sherpa
{
    using System.Threading;

    /// <summary>
    /// Program implementation.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main()
        {
#if DEBUG
            Settings settings = new Settings();

            RouteManager routeManager = new RouteManager(settings);
            ConfigurationServer configurationServer = new ConfigurationServer(settings, routeManager);

            configurationServer.Start();
            routeManager.Start();

            ManualResetEvent mre = new ManualResetEvent(false);

            mre.WaitOne();
#else
            // we'll go ahead and create an array so that we
            // can add the different services that
            // we'll create over time.
            Service[] servicesToRun;

            // to create a new instance of a new service,
            // just add it to the list of services 
            // specified in the ServiceBase array constructor.
            servicesToRun = new Service[] { new Service() };

            // now run all the service that we have created.
            // This doesn't actually cause the services
            // to run but it registers the services with the
            // Service Control Manager so that it can
            // when you start the service the SCM will call
            // the OnStart method of the service.
            Service.Run(servicesToRun);
#endif
        }
    }
}
