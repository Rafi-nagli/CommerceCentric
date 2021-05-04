using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.Implementation
{
    public class ManageService
    {
        private TimeSpan _waitStopTimeout = TimeSpan.FromSeconds(300);
        private TimeSpan _waitStartTimeout = TimeSpan.FromSeconds(300);

        private ILogService _log;
        private IDbFactory _dbFactory;
        private ISystemActionService _actionService;

        public ManageService(ILogService log,
            IDbFactory dbFactory,
            ISystemActionService actionService)
        {
            _log = log;
            _dbFactory = dbFactory;
            _actionService = actionService;
        }

        public void ProcessRestartActions()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var restartActions = _actionService.GetUnprocessedByType(db, SystemActionType.RestartService, null, null);
                foreach (var action in restartActions)
                {
                    Restart("Ccen.Dws.Full.Sync.Service");
                    
                    _actionService.SetResult(db, action.Id, SystemActionStatus.Done, null);
                    
                    db.Commit();
                }
            }
        }

        public CallResult<bool> Restart(string serviceName)
        {
            _log.Info("Begin Restart: " + serviceName);
            ServiceController service = new ServiceController(serviceName);
            try
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, _waitStopTimeout);
                _log.Info("Stopped");
            }
            catch (Exception ex)
            {
                _log.Info("Stop Error: " + ex.Message, ex);
            }
            
            try
            { 
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, _waitStartTimeout);

                _log.Info("Started");

                return CallResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _log.Info("Start Error: " + ex.Message, ex);
                return CallResult<bool>.Fail(ex.Message, ex);
            }
        }
    }
}
