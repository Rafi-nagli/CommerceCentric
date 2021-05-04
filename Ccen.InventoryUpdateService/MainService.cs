using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using Amazon.Api;
using Amazon.Common.Services;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.SyncService;
using Ccen.Model.SyncService;
using log4net;
using log4net.Config;

namespace Amazon.InventoryUpdateService
{
    public partial class MainService : ServiceBaseStarter, IService
    {
        private ThreadManager _threadManager;

        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public MainService()
        {
            _log = LogFactory.Default;
            _dbFactory = new DbFactory();
            _time = new TimeService(_dbFactory);

            InitializeComponent();
        } 

        protected override void OnStart(string[] args)
        {
            Database.SetInitializer<AmazonContext>(null);
            XmlConfigurator.Configure(new FileInfo(AppSettings.log4net_Config));
            _log.Fatal("Service STARTED! Self test SMTP message");

            _threadManager = new ThreadManager(_log, _time);
            _threadManager.Start();
        }

        protected override void OnStop()
        {
            _log.Info("OnStop");
            _log.Fatal("Service STOPPED!");
            _threadManager.Stop();
        }
    }
}
