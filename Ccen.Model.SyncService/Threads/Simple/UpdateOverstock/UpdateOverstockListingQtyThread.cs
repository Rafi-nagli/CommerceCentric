using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation.Markets.SupplierOasis;
using Amazon.Model.Implementation.Markets.Walmart.Feeds;
using Amazon.Web.Models;
using Jet.Api;
using Supplieroasis.Api;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateOverstock
{
    public class UpdateOverstockListingQtyThread : ThreadBase
    {
        private readonly SupplieroasisApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateOverstockListingQtyThread(SupplieroasisApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval, 
            TimeSpan betweenProcessingInverval)
            : base("UpdateOverstockListingQty", companyId, messageService, callbackInterval)
        {
            _api = api;
            _betweenProcessingInverval = betweenProcessingInverval;
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();
            var settings = new SettingsService(dbFactory);

            var lastSyncDate = settings.GetListingsQuantityToAmazonSyncDate(_api.Market, _api.MarketplaceId);

            LogWrite("Last sync date=" + lastSyncDate);

            if (!lastSyncDate.HasValue ||
                (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
            {
                var sync = new SupplierOasisItemsSync(log,
                    time,
                    dbFactory);

                sync.SendInventoryUpdates(_api);

                settings.SetListingsQuantityToAmazonSyncDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
            }
        }
    }
}
