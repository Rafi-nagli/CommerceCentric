using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets.Shopify;
using Amazon.Model.Implementation.Markets.WooCommerce;
using Amazon.Web.Models;
using Shopify.Api;
using WooCommerce.Api;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateShopifyInfo
{
    public class ImportWooCommerceListingDataThread : ThreadBase
    {
        private readonly WooCommerceApi _api;
        private TimeSpan _betweenProcessingInverval;

        public ImportWooCommerceListingDataThread(WooCommerceApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval,
            TimeSpan betweenProcessingInverval)
            : base("ImportWooCommerceListingData", companyId, messageService, callbackInterval)
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

            if (!lastSyncDate.HasValue ||
                (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
            {
                try
                {
                    var itemSyncService = new WooCommerceItemsImporter(log,
                        time,                        
                        dbFactory);
                    itemSyncService.Import(_api);

                    settings.SetListingsReadDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
                }
                catch (Exception ex)
                {
                    LogError(ex.Message, ex);
                }
            }
        }
    }
}
