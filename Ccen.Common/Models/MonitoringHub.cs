using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Common.Models
{
    public class MonitoringHub : IMonitoringHub
    {
        private IEmailService _emailSevice;
        private ILogService _log;

        public MonitoringHub(IEmailService emailService,
            ILogService log)
        {
            _log = log;
            _emailSevice = emailService;
        }

        public void AddCriticalNotification(string key, string message, Exception ex = null)
        {
            LogMessage("Critical", key, message, ex);
        }

        public void AddErrorNotification(string key, string message, Exception ex = null)
        {
            LogMessage("Error", key, message, ex);
        }

        public void AddWarningNotification(string key, string message, Exception ex = null)
        {
            LogMessage("Warning", key, message, ex);
        }

        private void LogMessage(string type, 
            string key,
            string message,
            Exception ex)
        {
            _log.Info(type + ": " + key
                    + ",\r\nMessage: " + message
                    + ",\r\nException: " + ExceptionHelper.GetAllMessages(ex));
                            _emailSevice.SendSystemEmailToAdmin("Critical: " + key,
                message + (ex != null ? ". Details: " + ExceptionHelper.GetAllMessages(ex) : ""));
        }

        public void Ping(string key)
        {
            _log.Info("Ping: " + key);
        }
    }
}
