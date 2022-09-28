namespace Sherpa
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Service installer.
    /// </summary>
    public class ServiceInstaller : System.ServiceProcess.ServiceInstaller
    {
        /// <summary>
        /// Raises the <see cref="E:System.Configuration.Install.Installer.Committed"/> event.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary"/> that contains the state of the computer after all the installers in the <see cref="P:System.Configuration.Install.Installer.Installers"/> property run.</param>
        protected override void OnCommitted(System.Collections.IDictionary savedState)
        {
            System.ServiceProcess.ServiceController sc = new System.ServiceProcess.ServiceController("Sherpa");
            sc.Start();
        }
    }
}
