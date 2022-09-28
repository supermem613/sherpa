namespace Test
{
    using Sherpa;
    using System.Collections.Generic;
    using System;

    /// <summary>
    /// A setting for this service.
    /// </summary>
    public class MockSettings : Settings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockSettings"/> class.
        /// </summary>
        public MockSettings()
        {
            this.RouteSettings = new Dictionary<string, RouteSetting>(StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Loads the settings.
        /// </summary>
        /// <param name="reload">if set to <c>true</c> [reload].</param>
        protected override void Load(bool reload)
        {
        }
    }
}
