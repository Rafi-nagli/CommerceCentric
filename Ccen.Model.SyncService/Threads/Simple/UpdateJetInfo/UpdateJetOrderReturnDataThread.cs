using System;
using Amazon.Common.Emails;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets.Walmart;
using Amazon.Web.Models;
using Ccen.Model.SyncService;
using Jet.Api;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateJetInfo
{
    public class UpdateJetOrderReturnDataThread : ThreadBase
    {
        private readonly JetApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateJetOrderReturnDataThread(JetApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval,
            TimeSpan betweenProcessingInverval)
            : base("UpdateJetOrderReturnData", companyId, messageService, callbackInterval)
        {
            _api = api;
            _betweenProcessingInverval = betweenProcessingInverval;
        }

        protected override void RunCallback()
        {
            CompanyDTO company = null;
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var settings = new SettingsService(dbFactory);
            var log = GetLogger();

            using (var db = dbFactory.GetRDb())
            {
                company = db.Companies.GetByIdWithSettingsAsDto(CompanyId);
            }

            var companyAddress = new CompanyAddressService(company);
            var addressService = new AddressService(null, companyAddress.GetReturnAddress(MarketIdentifier.Empty()), companyAddress.GetPickupAddress(MarketIdentifier.Empty()));
            var emailSmtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(company, AppSettings.IsDebug, AppSettings.IsSampleLabels);
            
            var actionService = new SystemActionService(log, time);
            var emailService = new EmailService(log, emailSmtpSettings, addressService);

            var lastSyncDate = settings.GetOrdersAdjustmentDate(_api.Market, _api.MarketplaceId);

            using (var db = dbFactory.GetRWDb())
            {
                LogWrite("Last sync date=" + lastSyncDate);

                if (!lastSyncDate.HasValue ||
                    (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
                {
                    var updater = new JetOrderReturn(_api, actionService, emailService, log, time);
                    updater.ProcessReturns(db);
                    settings.SetOrdersAdjustmentDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
                }
            }
        }
    }
}
