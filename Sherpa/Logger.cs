namespace Sherpa
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security;
    
    /// <summary>
    /// Logging facility for diagnostics.
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// Source for logging.
        /// </summary>
        private const string Source = "Sherpa";

        /// <summary>
        /// Maximum numbers of events kept.
        /// </summary>
        private const int MaxEvents = 50;

        /// <summary>
        /// Synchronization object.
        /// </summary>
        private object syncObject;

        /// <summary>
        /// The last events logged.
        /// </summary>
        private LinkedList<LogEntry> lastEvents;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        public Logger()
        {
            this.syncObject = new object();
            this.lastEvents = new LinkedList<LogEntry>();

            try
            {
                if (!EventLog.SourceExists(Logger.Source))
                {
                    EventLog.CreateEventSource(Logger.Source, "Application");
                }
            }
            catch (SecurityException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }
        
        /// <summary>
        /// Gets or sets the logging level.
        /// </summary>
        /// <value>
        /// The logging level.
        /// </value>
        public LoggingLevel LoggingLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the last logged time.
        /// </summary>
        public DateTime LastLogged
        {
            get;
            private set;
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="error">The error.</param>
        public void LogError(string error)
        {
            if (this.LoggingLevel >= LoggingLevel.Minimal)
            {
                this.LogMessage(error, EventLogEntryType.Error, 2000);
            }
        }

        /// <summary>
        /// Logs the information.
        /// </summary>
        /// <param name="information">The information.</param>
        public void LogInformation(string information)
        {
            if (this.LoggingLevel >= LoggingLevel.Minimal)
            {
                this.LogMessage(information, EventLogEntryType.Information, 1000);
            }
        }

        /// <summary>
        /// Logs the verbose.
        /// </summary>
        /// <param name="verbose">The verbose.</param>
        public void LogVerbose(string verbose)
        {
            if (this.LoggingLevel >= LoggingLevel.Verbose)
            {
                this.LogMessage(verbose, EventLogEntryType.Information, 3000);
            }
        }

        /// <summary>
        /// Gets the events.
        /// </summary>
        /// <returns>Latest events.</returns>
        public LogEntry[] GetEvents()
        {
            lock (this.syncObject)
            {
                LogEntry[] events = new LogEntry[this.lastEvents.Count];

                int i = 0;

                foreach (LogEntry entry in this.lastEvents)
                {
                    events[i] = entry;
                    i++;
                }

                return events;
            }
        }

        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="type">The type.</param>
        /// <param name="eventID">The event ID.</param>
        private void LogMessage(string message, EventLogEntryType type, int eventID)
        {
            lock (this.syncObject)
            {
                this.LastLogged = System.DateTime.Now;

                this.AppendLastEvent(message, type);

                System.Diagnostics.EventLog.WriteEntry(
                    Logger.Source,
                    message,
                    type,
                    eventID);
            }
        }

        /// <summary>
        /// Appends the last event.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="type">The type.</param>
        private void AppendLastEvent(string message, EventLogEntryType type)
        {
            this.lastEvents.AddFirst(new LogEntry(DateTime.Now, message, type));

            if (this.lastEvents.Count > Logger.MaxEvents)
            {
                this.lastEvents.RemoveLast();
            }
        }
    }
}
