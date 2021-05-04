using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets.eBay;
using Amazon.Model.Implementation.Markets.SupplierOasis;
using Amazon.Web.Models;
using eBay.Api;
using Supplieroasis.Api;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateOverstock
{
    public class ReadListingOverstockInfoFromMarketThread : ThreadBase
    {
        private readonly SupplieroasisApi _api;
        private TimeSpan _betweenProcessingInverval;

        public ReadListingOverstockInfoFromMarketThread(SupplieroasisApi api, 
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval, 
            TimeSpan betweenProcessingInverval)
            : base("ReadListingOverstockInfoFromMarket", companyId, messageService, callbackInterval)
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
                                    
            var lastSyncDate = settings.GetListingsReadDate(_api.Market, _api.MarketplaceId);

            LogWrite("Last sync date=" + lastSyncDate);

            var syncer = new SupplierOasisItemsSync(GetLogger(),
                time,
                dbFactory);

            if (!lastSyncDate.HasValue ||
                (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
            {
                try
                {
                    syncer.ReadItemsInfo(_api);
                }
                catch (Exception ex)
                {
                    log.Error("ReadItemsInfo", ex);
                }

                settings.SetListingsReadDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
            }
        }
    }
}
