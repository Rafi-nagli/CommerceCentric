using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Emails;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Core.Models.Search;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Threads.Simple.Notifications
{
    public class CheckEmailStatusThread : TimerThreadBase
    {
        public CheckEmailStatusThread(long companyId, ISystemMessageService messageService, IList<TimeSpan> callTimeStamps, ITime time)
            : base("CheckEmailStatus", companyId, messageService, callTimeStamps, time)
        {
            
        }

        protected override void RunCallback()
        {
            CompanyDTO company = null;
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            using (var db = dbFactory.GetRDb())
            {
                company = db.Companies.GetByIdWithSettingsAsDto(CompanyId);
            }

            var companyAddress = new CompanyAddressService(company);
            var addressService = new AddressService(null, companyAddress.GetReturnAddress(MarketIdentifier.Empty()), companyAddress.GetPickupAddress(MarketIdentifier.Empty()));
            //Checking email service, sent test message
            var emailSmtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(company, AppSettings.IsDebug, AppSettings.IsSampleLabels);
            var emailService = new EmailService(GetLogger(), emailSmtpSettings, addressService);

            var fromDate = time.GetAppNowTime().AddDays(-5);
            var emails = new List<EmailOrderDTO>();
            using (var db = dbFactory.GetRDb())
            {
                emails = db.Emails.GetAllWithOrder(new EmailSearchFilter()
                {
                    ResponseStatus = (int)EmailResponseStatusFilterEnum.ResponseNeeded
                })
                .Where(e => e.ReceiveDate > fromDate)
                .ToList();
            }

            emailService.SendEmailStatusNotification(emails);
        }
    }
}
