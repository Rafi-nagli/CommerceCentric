using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class RefreshRatesThread : TimerThreadBase
    {
        public RefreshRatesThread(long companyId, ISystemMessageService messageService, IList<TimeSpan> callTimeStamps, ITime time)
            : base("RefreshRates", companyId, messageService, callTimeStamps, time)
        {

        }

        protected override void RunCallback()
        {
            CompanyDTO company = null;
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();
            var weightService = new WeightService();
            var messageService = new SystemMessageService(log, time, dbFactory);
            var serviceFactory = new ServiceFactory();

            using (var db = dbFactory.GetRDb())
            {
                company = db.Companies.GetByIdWithSettingsAsDto(CompanyId);
            }

            var actionService = new SystemActionService(log, time);
            var rateProviders = serviceFactory.GetShipmentProviders(log,
                time,
                dbFactory,
                weightService,
                company.ShipmentProviderInfoList,
                null,
                null,
                null,
                null);


            //var addressService = new AddressService(null, company.GetReturnAddressDto(), company.GetPickupAddressDto());
            //Checking email service, sent test message
            //var emailSmtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(company, AppSettings.IsDebug, AppSettings.IsSampleLabels);
            //var emailService = new EmailService(GetLogger(), emailSmtpSettings, addressService);

            var rateService = new RateService(dbFactory,
                log,
                time,
                weightService,
                messageService,
                company,
                actionService,
                rateProviders);
            rateService.RefreshAmazonRates();
            rateService.RefreshSuspiciousFedexRates();
        }
    }
}
