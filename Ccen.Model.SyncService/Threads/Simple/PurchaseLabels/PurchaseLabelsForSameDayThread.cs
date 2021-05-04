using System;
using System.Collections.Generic;
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
    public class PurchaseLabelsForSameDayThread : TimerThreadBase
    {
        public PurchaseLabelsForSameDayThread(long companyId, ISystemMessageService messageService, IList<TimeSpan> callTimeStamps, ITime time)
            : base("PurchaseLabelsForSameDay", companyId, messageService, callTimeStamps, time)
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
            var weigthService = new WeightService();
            var orderHistoryService = new OrderHistoryService(log, time, dbFactory);
            var batchManager = new BatchManager(log, time, orderHistoryService, weigthService);
            
            var labelBatchService = new LabelBatchService(dbFactory,
                actionService,
                log,
                time,
                weigthService,
                serviceFactory,
                emailService,
                batchManager,
                pdfMaker,
                addressService,
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
                weigthService,
                company.Id);

            autoPurchaseService.PurchaseForSameDay();
        }
    }
}
