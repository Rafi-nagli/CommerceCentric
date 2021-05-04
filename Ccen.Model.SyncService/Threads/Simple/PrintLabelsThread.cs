using System;
using System.IO;
using System.Threading;
using Amazon.Common.Emails;
using Amazon.Common.Helpers;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Labels;
using Amazon.Model.Implementation.Pdf;
using Amazon.Model.Implementation.Rates;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class PrintLabelsThread : ThreadBase
    {
        public PrintLabelsThread(long companyId, ISystemMessageService messageService, TimeSpan? callbackInterval = null)
            : base("PrintLabels", companyId, messageService, callbackInterval)
        {
            ThreadPriority = ThreadPriority.Highest;
        }

        protected override void Init()
        {
            var log = GetLogger();
            var amazonOutput = AppSettings.LabelDirectory;
            var sampleFile = "access_test.txt";
            try
            {
                var filePath = amazonOutput + "\\" + sampleFile;
                using (var stream = File.Create(filePath))
                {
                    log.Info("File has been created, file=" + filePath);
                }
                File.Delete(filePath);
                log.Info("File has been deleted, file=" + filePath);
            }
            catch (Exception ex)
            {
                log.Fatal("Can't create test file", ex);
            }
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();
            var actionService = new SystemActionService(GetLogger(), time);
            var orderHistoryService = new OrderHistoryService(log, time, dbFactory);
            var serviceFactory = new ServiceFactory();
            var weightService = new WeightService();
            var pdfMaker = new PdfMakerByIText(GetLogger());
            var batchManager = new BatchManager(log, time, orderHistoryService, weightService);

            CompanyDTO company = null;
            using (var db = dbFactory.GetRDb())
            {
                company = db.Companies.GetByIdWithSettingsAsDto(CompanyId);
            }

            var addressService = AddressService.Default;
            var emailSmtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(company, AppSettings.IsDebug, AppSettings.IsSampleLabels);
            var emailService = new EmailService(GetLogger(), emailSmtpSettings, addressService);

            
            var labelBatchService = new LabelBatchService(dbFactory,
                actionService,
                GetLogger(),
                time,
                weightService,
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

            labelBatchService.ProcessPrintBatchActions(null);
        }
    }
}
