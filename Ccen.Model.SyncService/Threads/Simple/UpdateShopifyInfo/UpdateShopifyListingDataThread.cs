using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation.Markets.Shopify;
using Amazon.Web.Models;
using Shopify.Api;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateShopifyInfo
{
    public class UpdateShopifyListingDataThread : ThreadBase
    {
        private readonly ShopifyApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateShopifyListingDataThread(ShopifyApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval,
            TimeSpan betweenProcessingInverval)
            : base("UpdateShopify" + api.MarketplaceId + "ListingData", companyId, messageService, callbackInterval)
        {
            _api = api;
            _betweenProcessingInverval = betweenProcessingInverval;
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);

            var settings = new SettingsService(dbFactory);

            var lastSyncDate = settings.GetListingsSendDate(_api.Market, _api.MarketplaceId);

            LogWrite("Last sync date=" + lastSyncDate);

            if (!lastSyncDate.HasValue ||
                (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
            {
                try
                {
                    var sync = new ShopifyItemsSync(GetLogger(),
                        time,
                        dbFactory);
                    sync.SendItemUpdates(_api);
                    //sync.ProcessUnpublishedRequests(_api);
                    settings.SetListingsSendDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
                }
                catch (Exception ex)
                {
                    LogError(ex.Message, ex);
                }
            }
        }
    }
}
