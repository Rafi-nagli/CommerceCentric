using System;
using System.Threading;
using Amazon.Common.Services;
using Amazon.Model.SyncService;
using Ccen.Model.SyncService;

namespace Amazon.InventoryUpdateService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (AppSettings.IsDebug)
            {
                throw new Exception("Direct launch!");
                (new MainService()).Start(new string[] {});
                Thread.Sleep(Timeout.Infinite);
            }
            else
            {
                var serviceToRun = new MainService();
                CommandServiceRunner.RunService(args, serviceToRun, new ProjectInstaller());
            }
        }
    }
}
