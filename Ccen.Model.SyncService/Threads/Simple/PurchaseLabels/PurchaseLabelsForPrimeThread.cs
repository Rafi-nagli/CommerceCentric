using System;
using Amazon.Common.Emails;
using Amazon.Common.Helpers;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Labels;
using Amazon.Model.Implementation.Pdf;
using Amazon.Model.Implementation.Rates;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Threads.Simple.PurchaseLabels
{
    public class PurchaseLabelsForPrimeThread : ThreadBase
    {
        public PurchaseLabelsForPrimeThread(long companyId, ISystemMessageService messageService, TimeSpan? callbackInterval = null)
            : base("PurchaseLabelsForPrime", companyId, messageService, callbackInterval)
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
            var fromAddressList = new CompanyAddressService(company);

            var serviceFactory = new ServiceFactory();
            var pdfMaker = new PdfMakerByIText(GetLogger());
            var actionService = new SystemActionService(log, time);
            var addressService = new AddressService(null, 
                fromAddressList.GetReturnAddress(MarketIdentifier.Empty()), 
                fromAddressList.GetPickupAddress(MarketIdentifier.Empty()));
            var emailSmtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(company, AppSettings.IsDebug, AppSettings.IsSampleLabels);
            var emailService = new EmailService(GetLogger(), emailSmtpSettings, addressService);

            var weightService = new WeightService();

            var orderHistoryService = new OrderHistoryService(log, time, dbFactory);
            var batchManager = new BatchManager(log, time, orderHistoryService, weightService);
            var labelBatchService = new LabelBatchService(dbFactory,
                actionService,
                log,
                time,
                weightService,
                serviceFactory,
                emailService,
                batchManager,
                pdfMaker,
                AddressService.Default,
                orderHistoryService,
                AppSettings.DefaultCustomType,
                AppSettings.LabelDirectory,
                AppSettings.ReserveDirectory,
                AppSettings.TemplateDirectory,
                new LabelBatchService.Config()
                {
                    PrintErrorsToEmails = new[] { company.SellerEmail, company.SellerWarehouseEmailAddress },
                    PrintErrorsCCEmails = new[] { EmailHelper.RaananEmail, EmailHelper.SupportDgtexEmail },
                },
                AppSettings.IsSampleLabels);

            var autoPurchaseService = new LabelAutoBuyService(dbFactory,
                log,
                time,
                batchManager,
                labelBatchService,
                actionService,
                emailService,
                weightService,
                company.Id);

            autoPurchaseService.PurchaseForPrime();
        }
    }
}
