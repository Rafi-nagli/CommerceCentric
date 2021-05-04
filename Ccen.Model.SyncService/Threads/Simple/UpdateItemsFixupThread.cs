using System;
using System.Collections.Generic;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Web.Models;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class ListingsFixupThread : TimerThreadBase
    {
        public ListingsFixupThread(long companyId, ISystemMessageService messageService, IList<TimeSpan> callTimes, ITime time)
            : base("ListingsFixup", companyId, messageService, callTimes, time)
        {

        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var log = GetLogger();
            var time = new TimeService(dbFactory);
            var settings = new SettingsService(dbFactory);
            var actionService = new SystemActionService(log, time);
            var itemHistoryService = new ItemHistoryService(log, time, dbFactory);

            //var itemFixupService = new ItemAutoFixIssueService(log, time, dbFactory, actionService, itemHistoryService);

            //using (var db = dbFactory.GetRWDb())
            //{
            //    db.DisableValidation();

            //    itemFixupService.RequestUpdatesForUngroupedListings();

            //    //itemFixupService.RequestUpdatesForPublishingInProgressListings();

            //    itemFixupService.RequestUnpublishWithUPCIssueListings();

            //    itemFixupService.ProcessDefectsRules();
            //}
        }
    }
}
