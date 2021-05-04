using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;

namespace Amazon.InventoryUpdateService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            this.MainServiceInstaller.ServiceName = new ConfigUtils().GetServiceName();
            this.MainServiceInstaller.DisplayName = new ConfigUtils().GetServiceName();
        }

        private void MainServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {

        }

        private void MainServiceInstaller_BeforeInstall(object sender, InstallEventArgs e)
        {

        }
    }
}
