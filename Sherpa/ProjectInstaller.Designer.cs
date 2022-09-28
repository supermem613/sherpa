namespace Sherpa
{
    /// <summary>
    /// Project installer.
    /// </summary>
    public partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        /// <summary>
        /// Process installer.
        /// </summary>
        private System.ServiceProcess.ServiceProcessInstaller processInstaller;
        
        /// <summary>
        /// Service installer.
        /// </summary>
        private ServiceInstaller serviceInstaller;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }

            base.Dispose(disposing);
        }
        
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.processInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstaller = new ServiceInstaller();

            this.processInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.processInstaller.Password = null;
            this.processInstaller.Username = null;

            this.serviceInstaller.DisplayName = "Sherpa";
            this.serviceInstaller.ServiceName = "Sherpa";
            this.serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;

            this.Installers.AddRange(new System.Configuration.Install.Installer[] 
                {
                    this.processInstaller,
                    this.serviceInstaller
                });
        }
    }
}