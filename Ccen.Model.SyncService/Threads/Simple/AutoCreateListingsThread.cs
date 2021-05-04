using System;
using System.Collections.Generic;
using Amazon.Common.Emails;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.AutoCreateListings;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class AutoCreateListingsThread : TimerThreadBase
    {
        public AutoCreateListingsThread(long companyId, ISystemMessageService messageService, IList<TimeSpan> callTimeStamps, ITime time)
            : base("AutoCreateListings", companyId, messageService, callTimeStamps, time)
        {
            
        }

        protected override void RunCallback()
        {
            CompanyDTO company = null;
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();

            var now = time.GetAppNowTime();
            if (!time.IsBusinessDay(now))
                return;

            using (var db = dbFactory.GetRDb())
            {
                company = db.Companies.GetByIdWithSettingsAsDto(CompanyId);
            }

            var companyAddress = new CompanyAddressService(company);
            var quantityManager = new QuantityManager(log, time);
            var actionService = new SystemActionService(log, time);
            var addressService = new AddressService(null, companyAddress.GetReturnAddress(MarketIdentifier.Empty()), companyAddress.GetPickupAddress(MarketIdentifier.Empty()));
            var emailSmtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(company, AppSettings.IsDebug, AppSettings.IsSampleLabels);
            var emailService = new EmailService(GetLogger(), emailSmtpSettings, addressService);
            var cacheService = new CacheService(log, time, actionService, quantityManager);
            var barcodeService = new BarcodeService(log, time, dbFactory);
            var itemHistoryService = new ItemHistoryService(log, time, dbFactory);

            var autoCreateWMListingService = new AutoCreateWalmartListingService(log,
                time,
                dbFactory,
                cacheService,
                barcodeService,
                emailService,
                null,
                itemHistoryService,
                AppSettings.IsDebug);
            var autoCreateEBayListingService = new AutoCreateEBayListingService(log,
                time,
                dbFactory,
                cacheService,
                barcodeService,
                emailService,
                itemHistoryService,
                AppSettings.IsDebug);
            var autoCreateAmazonUSPrimeService = new AutoCreateAmazonUSPrimeListingService(log,
                time,
                dbFactory,
                itemHistoryService);

            //autoCreateWMListingService.CreateListings();
            //log.Info("Walmart listings were created");

            //autoCreateEBayListingService.CreateListings();
            //log.Info("eBay listings were created");

            autoCreateAmazonUSPrimeService.CreateListings();
            log.Info("Amazon US Prime listings were created");
        }
    }
}
