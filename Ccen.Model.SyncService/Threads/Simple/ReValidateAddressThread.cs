using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using System.Linq;
using Amazon.Model.Implementation;
using Amazon.Core.Entities.Enums;
using Amazon.Model.Implementation.Validation;
using Amazon.Model.General;
using Amazon.DTO;
using Amazon.DTO.Users;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class ReValidateAddressThread : ThreadBase
    {
        public ReValidateAddressThread(long companyId, ISystemMessageService messageService, TimeSpan? callbackInterval = null)
            : base("ReValidateAddress", companyId, messageService, callbackInterval)
        {

        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();
            var orderHistoryService = new OrderHistoryService(log, time, dbFactory);
            var serviceFactory = new ServiceFactory();

            CompanyDTO company = null;
            using (var db = dbFactory.GetRDb())
            {
                company = db.Companies.GetByIdWithSettingsAsDto(CompanyId);
            }

            var addressCheckServices = serviceFactory.GetAddressCheckServices(log,
               time,
               dbFactory,
               company.AddressProviderInfoList);

            var companyAddress = new CompanyAddressService(company);
            var addressService = new AddressService(addressCheckServices, companyAddress.GetReturnAddress(MarketIdentifier.Empty()), companyAddress.GetPickupAddress(MarketIdentifier.Empty()));


            var addressChecker = new AddressChecker(log, dbFactory, addressService, orderHistoryService, time);

            using (var db = dbFactory.GetRWDb())
            {
                addressChecker.RecheckAddressesWithException();
            }
        }
    }
}
