using System;
using Amazon.Api;
using Amazon.Common.Emails;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO.Users;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets.Amazon.Feeds;
using Amazon.Web.Models;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateAmazonInfo
{
    public class UpdateAdjustmentDataThread : ThreadBase
    {
        private readonly AmazonApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateAdjustmentDataThread(AmazonApi api, 
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval, 
            TimeSpan betweenProcessingInverval)
            : base("UpdateAdjustmentData", companyId, messageService, callbackInterval)
        {
            _api = api;
            _betweenProcessingInverval = betweenProcessingInverval;
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();
            CompanyDTO company;

            using (var db = dbFactory.GetRDb())
            {
                company = db.Companies.GetByIdWithSettingsAsDto(CompanyId);
            }

            var emailSmtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(company, AppSettings.IsDebug, AppSettings.IsSampleLabels);
            var addressService = AddressService.Default;
            var actionService = new SystemActionService(log, time);
            var settings = new SettingsService(dbFactory);
            var emailService = new EmailService(log, emailSmtpSettings, addressService);

            var lastSyncDate = settings.GetOrdersAdjustmentDate(_api.Market, _api.MarketplaceId);

            LogWrite("Last sync date=" + lastSyncDate);

            if (!lastSyncDate.HasValue ||
                (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
            {
                var updater = new AdjustmentDataUpdater(actionService,
                    emailService,
                    log, 
                    time,
                    dbFactory);
                var unprocessedFeed = updater.GetUnprocessedFeedId(_api.MarketplaceId);
                if (unprocessedFeed != null)
                {
                    LogWrite("Update unprocessed feed");
                    updater.UpdateSubmittedFeed(_api,
                        unprocessedFeed,
                        AppSettings.FulfillmentResponseDirectory);
                }
                else
                {
                    LogWrite("Submit new feed");
                    updater.SubmitFeed(_api, CompanyId, null, AppSettings.FulfillmentRequestDirectory);
                    settings.SetOrdersAdjustmentDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
                }
            }
        }
    }
}
