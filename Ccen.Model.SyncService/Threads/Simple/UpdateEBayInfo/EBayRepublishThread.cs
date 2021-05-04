using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using eBay.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateEBayInfo
{
    public class EBayRepublishThread : TimerThreadBase
    {
        private readonly eBayApi _api;

        public EBayRepublishThread(eBayApi api,
            long companyId,
            ISystemMessageService messageService,
            IList<TimeSpan> callTimeStamps,
            ITime time)
            : base(api.Market + api.MarketplaceId + "Republish", companyId, messageService, callTimeStamps, time)
        {
            _api = api;
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();

            _api.Connect();

            var service = new ItemRepublishService(log, time, dbFactory);
            service.PublishUnpublishedListings(MarketType.eBay, null);
        }
    }
}
