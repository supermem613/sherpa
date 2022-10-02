using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.ComponentModel;

namespace Sherpa
{
    [RunInstaller(true)]
    public class EventLogInstaller : Installer
    {
        private System.Diagnostics.EventLogInstaller eventLogInstaller;

        public EventLogInstaller()
        {
            // Create an instance of an EventLogInstaller.
            eventLogInstaller = new System.Diagnostics.EventLogInstaller();

            // Set the source name of the event log.
            eventLogInstaller.Source = "Sherpa";

            // Set the event log that the source writes entries to.
            eventLogInstaller.Log = "Application";

            // Add myEventLogInstaller to the Installer collection.
            Installers.Add(eventLogInstaller);
        }
    }
}
