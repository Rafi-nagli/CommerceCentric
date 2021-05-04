using Amazon.Core.Models.Calls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface IMonitoringHub
    {
        void AddCriticalNotification(string key, string message, Exception ex = null);
        void AddErrorNotification(string key, string message, Exception ex = null);
        void AddWarningNotification(string key, string message, Exception ex = null);
        void Ping(string key);
    }
}
