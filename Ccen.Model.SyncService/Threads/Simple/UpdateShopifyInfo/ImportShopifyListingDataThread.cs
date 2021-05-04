using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO.Users;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets.Shopify;
using Amazon.Web.Models;
using Shopify.Api;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateShopifyInfo
{
    public class ImportShopifyListingDataThread : ThreadBase
    {
        private readonly ShopifyApi _api;
        private TimeSpan _betweenProcessingInverval;

        public ImportShopifyListingDataThread(ShopifyApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval,
            TimeSpan betweenProcessingInverval)
            : base("ImportShopifyListingData", companyId, messageService, callbackInterval)
        {
            _api = api;
            _betweenProcessingInverval = betweenProcessingInverval;
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();
            CompanyDTO company = null;

            using (var db = dbFactory.GetRDb())
            {
                company = db.Companies.GetFirstWithSettingsAsDto();
            }

            var settings = new SettingsService(dbFactory);
            var styleHistoryService = new StyleHistoryService(log, time, dbFactory);

            var lastSyncDate = settings.GetListingsReadDate(_api.Market, _api.MarketplaceId);

            LogWrite("Last sync date=" + lastSyncDate);

            if (!lastSyncDate.HasValue ||
                (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
            {
                try
                {
                    var itemSyncService = new ShopifyItemsImporter(log,
                        time,                        
                        dbFactory,
                        styleHistoryService,
                        null);

                    var isImportDescription = company.ShortName != PortalEnum.HDEA.ToString() && company.ShortName != PortalEnum.BS.ToString();
                    itemSyncService.Import(_api, 
                        null, 
                        ShopifyItemsImporter.ImportModes.Full, 
                        processOnlyNewStyles: false,
                        overrideStyleIds: true, 
                        overrideStyleTitles: true,
                        importDescription: isImportDescription);

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
