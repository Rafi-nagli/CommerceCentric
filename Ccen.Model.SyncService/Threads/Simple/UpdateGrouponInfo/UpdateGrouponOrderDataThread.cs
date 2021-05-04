using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation.Sync;
using Amazon.Web.Models;
using Groupon.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateGrouponInfo
{
    public class UpdateGrouponOrderDataThread : ThreadBase
    {
        private readonly GrouponApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateGrouponOrderDataThread(GrouponApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval,
            TimeSpan betweenProcessingInverval)
            : base("Update" + api.Market + "OrderData", companyId, messageService, callbackInterval)
        {
            _api = api;
            _betweenProcessingInverval = betweenProcessingInverval;
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var settings = new SettingsService(dbFactory);
            var log = GetLogger();

            _api.Connect();

            using (var db = dbFactory.GetRWDb())
            {
                var lastSyncDate = settings.GetOrdersFulfillmentDate(_api.Market, _api.MarketplaceId);

                LogWrite("Last sync date=" + lastSyncDate);

                if (!lastSyncDate.HasValue ||
                    (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
                {
                    var updater = new BaseOrderUpdater(_api, log, time);
                    updater.UpdateOrders(db);
                    settings.SetOrdersFulfillmentDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
                }
            }
        }
    }
}
