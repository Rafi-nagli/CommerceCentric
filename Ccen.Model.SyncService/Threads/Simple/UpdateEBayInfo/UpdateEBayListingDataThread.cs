using System;
using System.IO;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets.eBay;
using Amazon.Templates;
using Amazon.Web.Models;
using Ccen.Model.SyncService;
using eBay.Api;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateEBayInfo
{
    public class UpdateEBayListingDataThread : ThreadBase
    {
        private readonly eBayApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateEBayListingDataThread(eBayApi api, 
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval, 
            TimeSpan betweenProcessingInverval)
            : base("UpdateEBay" + api.MarketplaceId + "ListingData", companyId, messageService, callbackInterval)
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
            var styleHistoryService = new StyleHistoryService(log, time, dbFactory);
            var styleManager = new StyleManager(log, time, styleHistoryService);
                
            var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                Path.Combine(AppSettings.TemplateDirectory, TemplateHelper.EBayDescriptionTemplateName));
            var templateMultiListingPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                Path.Combine(AppSettings.TemplateDirectory, TemplateHelper.EBayDescriptionMultiListingTemplateName));

            var lastSyncDate = settings.GetListingsSendDate(_api.Market, _api.MarketplaceId);
            var isManualSyncRequested = settings.GetListingsManualSyncRequest(_api.Market, _api.MarketplaceId);
            var pauseStatus = settings.GetListingsSyncPause(_api.Market, _api.MarketplaceId) ?? false;

            LogWrite("Last sync date=" + lastSyncDate + ", manually sync=" + isManualSyncRequested);
            
            if (pauseStatus)
            {
                LogWrite("Listings sync in pause");
                return;
            }

            if (!lastSyncDate.HasValue ||
                (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval
                || isManualSyncRequested == true)
            {
                var sync = new eBayItemsSync(log,
                    time,
                    _api,
                    dbFactory,
                    styleManager,
                    AppSettings.eBayImageDirectory,
                    AppSettings.eBayImageBaseUrl,
                    AppSettings.LabelDirectory);
                
                sync.SendItemUpdates();

                settings.SetListingsSendDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
                settings.SetListingsManualSyncRequest(false, _api.Market, _api.MarketplaceId);
            }
        }
    }
}
