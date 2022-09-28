namespace Sherpa
{
    using System.Collections.Generic;
    using Microsoft.Win32;

    /// <summary>
    /// Settings for a listener.
    /// </summary>
    public class RouteSetting : Setting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RouteSetting"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="key">The key.</param>
        public RouteSetting(string name, RegistryKey key)
        {
            this.Name = name;

            // TODO: Validation.
            RegistryKey subkey = key.OpenSubKey(name);

            this.Port = this.GetRegistryValue<int>(subkey, "Port");
            this.Type = this.GetRegistryValue<RouteType>(subkey, "Type");

            this.Destinations = new List<KeyValuePair<string, int>>();

            this.Destination = this.GetRegistryValue<string>(subkey, "Destination");

            foreach (string destinationString in this.Destination.Split(','))
            {
                string[] pairs = destinationString.Split(':');

                KeyValuePair<string, int> destination = new KeyValuePair<string, int>(pairs[0].Trim(), int.Parse(pairs[1].Trim()));

                this.Destinations.Add(destination);
            }

            subkey.Close();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteSetting"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="port">The port.</param>
        /// <remarks>
        /// For testing only.
        /// </remarks>
        internal RouteSetting(string name, int port)
        {
            this.Name = name;
            this.Port = port;
            this.Destinations = new List<KeyValuePair<string, int>>();
        }

        /// <summary>
        /// Route setting comparison values.
        /// </summary>
        public enum ServerSettingComparison
        {
            /// <summary>
            /// Different settings altogether.
            /// </summary>
            Different,

            /// <summary>
            /// Same settings.
            /// </summary>
            Same,

            /// <summary>
            /// Reconciliable settings.
            /// </summary>
            Reconcilable,
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
        /// Gets the port.
        /// </summary>
        public int Port
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        public RouteType Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the destination.
        /// </summary>
        public string Destination
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the destinations.
        /// </summary>
        public List<KeyValuePair<string, int>> Destinations
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Compares the route setting with the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>Comparison result.</returns>
        public ServerSettingComparison Compare(RouteSetting other)
        {
            if ((this.Name != other.Name) ||
                (this.Port != other.Port) ||
                (this.Type != other.Type))
            {
                return ServerSettingComparison.Different;
            }

            if (this.Destination == other.Destination)
            {
                return ServerSettingComparison.Same;
            }

            return ServerSettingComparison.Reconcilable;
        }
    }
}
