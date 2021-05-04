using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation.Markets.Magento;
using Amazon.Web.Models;
using Magento.Api.Wrapper;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateMagentoInfo
{
    public class UpdateMagentoListingDataThread : ThreadBase
    {
        private readonly Magento20MarketApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateMagentoListingDataThread(Magento20MarketApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval, 
            TimeSpan betweenProcessingInverval)
            : base("UpdateMagento" + api.MarketplaceId + "ListingData", companyId, messageService, callbackInterval)
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

            var lastSyncDate = settings.GetListingsSendDate(_api.Market, _api.MarketplaceId);

            LogWrite("Last sync date=" + lastSyncDate);

            if (!lastSyncDate.HasValue ||
                (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
            {
                var sync = new MagentoItemsSync(_api,
                    dbFactory,
                    GetLogger(),
                    time);

                sync.SyncAttributeOptions();
                //sync.SyncCategories();
                sync.SendItemUpdates();
                settings.SetListingsSendDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
            }
        }
    }
}
