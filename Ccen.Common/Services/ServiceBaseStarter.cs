using System.ServiceProcess;

namespace Amazon.Common.Services
{
    public partial class ServiceBaseStarter : ServiceBase
    {
        public void Start(string[] args)
        {
            OnStart(args);
        }
    }
}
