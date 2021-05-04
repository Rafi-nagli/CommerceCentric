using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Notifications;

namespace Amazon.Core.Contracts.Parser
{
    public class ParseContext
    {
        public long CompanyId { get; set; }

        public ILogService Log { get; set; }
        public IDbFactory DbFactory { get; set; }

        public ISystemActionService ActionService { get; set; }

        public IStyleManager StyleManager { get; set; }

        public INotificationService NotificationService { get; set; }

        public IStyleHistoryService StyleHistoryService { get; set; }
        public IItemHistoryService ItemHistoryService { get; set; }

        public ISyncInformer SyncInformer { get; set; }

        public ITime Time { get; set; }
    }
}
