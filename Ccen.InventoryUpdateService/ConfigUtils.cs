using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.InventoryUpdateService
{
    public class ConfigUtils
    {
        public string GetServiceName()
        {
            string serviceName = string.Empty;

            try
            {
                Assembly executingAssembly = Assembly.GetAssembly(typeof(ProjectInstaller));
                string targetDir = executingAssembly.Location;
                Configuration config = ConfigurationManager.OpenExeConfiguration(targetDir);
                serviceName = config.AppSettings.Settings["ServiceName"].Value.ToString();

                return serviceName;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
