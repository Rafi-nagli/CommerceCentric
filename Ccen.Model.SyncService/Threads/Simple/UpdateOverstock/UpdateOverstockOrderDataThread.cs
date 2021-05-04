using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation.Markets.Walmart;
using Amazon.Model.Implementation.Sync;
using Amazon.Web.Models;
using Jet.Api;
using Supplieroasis.Api;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateOverstock
{
    public class UpdateOverstockOrderDataThread : ThreadBase
    {
        private readonly SupplieroasisApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateOverstockOrderDataThread(SupplieroasisApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval,
            TimeSpan betweenProcessingInverval)
            : base("UpdateOverstockOrderData", companyId, messageService, callbackInterval)
        {
            _api = api;
            _betweenProcessingInverval = betweenProcessingInverval;
        }

        protected override void RunCallback()
        {
            _api.Connect();

            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var settings = new SettingsService(dbFactory);

            using (var db = dbFactory.GetRWDb())
            {
                var lastSyncDate = settings.GetOrdersFulfillmentDate(_api.Market, _api.MarketplaceId);

                LogWrite("Last sync date=" + lastSyncDate);

                if (!lastSyncDate.HasValue ||
                    (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
                {
                    var updater = new BaseOrderUpdater(_api, GetLogger(), time);
                    updater.UpdateOrders(db);
                    settings.SetOrdersFulfillmentDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
                }
            }
        }
    }
}
