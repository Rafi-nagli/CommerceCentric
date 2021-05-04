using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO.Users;
using Amazon.Model.Implementation.Markets.Walmart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.SyncService.Threads.Simple.Walmart
{
    public class CheckWalmartListingStatusThread : TimerThreadBase
    {
        public CheckWalmartListingStatusThread(long companyId, ISystemMessageService messageService, IList<TimeSpan> callTimeStamps, ITime time)
            : base("CheckWalmartListingStatus", companyId, messageService, callTimeStamps, time)
        {

        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();

            var now = time.GetAppNowTime();
            if (!time.IsBusinessDay(now))
                return;

            var service = new WalmartListingService(log, time, dbFactory);

            service.RepublishListingWithImageIssue();

            service.RepublishListingWithSKUIssue();

            service.RepublishListingWithBarcodeIssue();
        }
    }
}
