namespace Sherpa
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// A log entry.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntry"/> class.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="message">The message.</param>
        /// <param name="eventLogEntryType">Type of the event log entry.</param>
        public LogEntry(DateTime dateTime, string message, EventLogEntryType eventLogEntryType)
        {
            this.DateTime = dateTime;
            this.Message = message;
            this.EventLogEntryType = eventLogEntryType;
        }

        /// <summary>
        /// Gets the date time.
        /// </summary>
        public DateTime DateTime
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public string Message
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the type of the event log entry.
        /// </summary>
        /// <value>
        /// The type of the event log entry.
        /// </value>
        public EventLogEntryType EventLogEntryType
        {
            get;
            private set;
        }
    }
}
