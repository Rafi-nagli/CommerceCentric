using System;
using System.Collections;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace Amazon.Common.Services
{
    public class CommandServiceRunner
    {
        public enum Result
        {
            AsService,
            Install,
            Uninstall,
            Execute
        }

        public static bool RunService(string[] args,
            ServiceBaseStarter service,
            Installer serviceInstaller)
        {
            var command = new CommandServiceRunner();
            var result = command.CheckArgs(args);
            switch (result)
            {
                case Result.Install:
                    return command.Install(serviceInstaller);
                case Result.Uninstall:
                    return command.Uninstall(serviceInstaller);
                case Result.AsService:
                    var servicesToRun = new ServiceBase[]
                                            {
                                                service
                                            };
                    ServiceBase.Run(servicesToRun);
                    break;
                case Result.Execute:
                    service.Start(args);
                    break;
            }
            return true;
        }

        public Result CheckArgs(string[] args)
        {
            var command = String.Empty;
            if (args != null && args.Length > 0 && args[0] != null)
                command = args[0].ToLower().Trim();

            switch (command)
            {
                case "/service":
                    return Result.AsService;
                case "/install":
                    return Result.Install;
                case "/uninstall":
                    return Result.Uninstall;
                case "/exec":
                    return Result.Execute;
            }
            return Result.AsService;
        }

        public bool Uninstall(Installer serviceInstaller)
        {
            return RunTransactedInstaller(serviceInstaller, false);
        }

        public bool Install(Installer serviceInstaller)
        {
            return RunTransactedInstaller(serviceInstaller, true);
        }

        private bool RunTransactedInstaller(Installer serviceIinstaller, bool install)
        {
            bool flag;
            var installer = new TransactedInstaller();

            try
            {
                installer.Installers.Add(serviceIinstaller);
                var cmd = new[] { string.Format("/assemblypath={0}", Assembly.GetEntryAssembly().Location) };
                var context = new InstallContext("", cmd);
                installer.Context = context;

                if (install)
                {
                    installer.Install(new Hashtable());
                }
                else
                {
                    installer.Uninstall(null);
                }
                flag = true;
            }
            catch (Exception)
            {
                flag = false;
            }
            finally
            {
                installer.Dispose();
            }

            return flag;
        }
    }
}
