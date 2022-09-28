namespace Sherpa
{
    using System;
    using System.Collections.Generic;
    using System.Timers;
    using Microsoft.Win32;

    /// <summary>
    /// Settings for the service.
    /// </summary>
    public class Settings : Setting
    {
        /// <summary>
        /// Reload timer.
        /// </summary>
        private Timer reloadTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
        {
            this.Logger = new Logger();
            
            this.Load(false);

            this.reloadTimer = new Timer();
            this.reloadTimer.Elapsed += new ElapsedEventHandler(this.ReloadTimerEvent);
            this.reloadTimer.Interval = 5 * 1000; // 5 seconds
            this.reloadTimer.Enabled = true;
        }

        /// <summary>
        /// RouteManager load handler.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public delegate void RoutesLoadHandler(Dictionary<string, RouteSetting> routes);
        
        /// <summary>
        /// Configuration Port changes handler.
        /// </summary>
        /// <param name="port">The port.</param>
        public delegate void ConfigurationPortChangeHandler(int port);

        /// <summary>
        /// Occurs when [routes load].
        /// </summary>
        public event RoutesLoadHandler RoutesLoad;
            
        /// <summary>
        /// Occurs when [configuration port change].
        /// </summary>
        public event ConfigurationPortChangeHandler ConfigurationPortChange;

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public Logger Logger
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the route settings.
        /// </summary>
        public Dictionary<string, RouteSetting> RouteSettings
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the configuration port.
        /// </summary>
        public int ConfigurationPort
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the last updated.
        /// </summary>
        public DateTime LastUpdated
        {
            get;
            protected set;
        }

        /// <summary>
        /// Loads the settings.
        /// </summary>
        /// <param name="reload">if set to <c>true</c> [reload].</param>
        protected virtual void Load(bool reload)
        {
            RegistryKey keyHKLM = Registry.LocalMachine;

            try
            {
                using (RegistryKey key = keyHKLM.OpenSubKey(@"SOFTWARE\Sherpa"))
                {
                    if (key == null)
                    {
                        this.Logger.LogError(@"Could not find the HKEY_LOCAL_MACHINE\SOFTWARE\Sherpa registry key.");
                        return;
                    }

                    this.Logger.LoggingLevel = this.GetRegistryValue<LoggingLevel>(key, "Logging");
                    
                    if (reload)
                    {
                        if (this.GetRegistryValue<int>(key, "ApplyNow") == 0)
                        {
                            return;
                        }

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Sherpa", "ApplyNow", 0, RegistryValueKind.DWord);
                    }
                    
                    this.ConfigurationPort = this.GetRegistryValue<int>(key, "ConfigurationPort");

                    if (this.ConfigurationPort == 0)
                    {
                        this.Logger.LogError(@"Invalid or missing configuration port registry key.");
                        return;
                    }

                    if (this.ConfigurationPortChange != null)
                    {
                        this.ConfigurationPortChange(this.ConfigurationPort);
                    }
                }

                using (RegistryKey key = keyHKLM.OpenSubKey(@"SOFTWARE\Sherpa\Routes"))
                {
                    if (key == null)
                    {
                        this.Logger.LogError(@"Could not find the HKEY_LOCAL_MACHINE\SOFTWARE\Sherpa\Routes registry key.");
                        return;
                    }

                    string[] routeKeys = key.GetSubKeyNames();
                    if (routeKeys == null || routeKeys.Length == 0)
                    {
                        this.Logger.LogError(@"The HKEY_LOCAL_MACHINE\SOFTWARE\Sherpa\Routes registry key is empty and does not have any routes defined.");
                        return;
                    }

                    Dictionary<string, RouteSetting> routeSettings = new Dictionary<string, RouteSetting>(routeKeys.Length, StringComparer.InvariantCultureIgnoreCase);

                    foreach (string route in routeKeys)
                    {
                        routeSettings[route] = new RouteSetting(route, key);
                    }

                    if (this.RoutesLoad != null)
                    {
                        // Notify routes.
                        this.RoutesLoad(routeSettings);
                    }

                    this.RouteSettings = routeSettings;
                }

                this.LastUpdated = DateTime.Now;
            }
            catch (System.Security.SecurityException e)
            {
                this.Logger.LogError("Could not load settings due to a security exception: " + e);
            }
        }

        /// <summary>
        /// Reloads the settings at the timer event.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        private void ReloadTimerEvent(object source, ElapsedEventArgs e)
        {
            this.Load(true);
        }        
    }
}