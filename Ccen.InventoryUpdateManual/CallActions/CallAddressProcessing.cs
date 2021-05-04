using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.VendorOrders;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Validation;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Addresses;
using Amazon.Model.StampsCom;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallAddressProcessing
    {
        private ILogService _log;
        private CompanyDTO _company;
        private ITime _time;
        private IDbFactory _dbFactory;
        private IHtmlScraperService _htmlScraper;

        public CallAddressProcessing(
            ILogService log,
            ITime time,
            IDbFactory dbFactory,
            CompanyDTO company)
        {
            _log = log;
            _company = company;
            _time = time;
            _dbFactory = dbFactory;
            _htmlScraper = new HtmlScraperService(log, time, dbFactory);
        }

        public void CallCheckCompleteInvalidAddress1()
        {
            //108-6317828-8714644
            var addressTo = new AddressDTO()
            {
                FullName = "Fatma Ghirani",
                Address1 = "Roosevelt island 40 RD APT6J",
                Address2 = "Roosevelt island 40 RD APT6J",
                City = "NYCity",
                State = "NY",
                Country = "US",
                Zip = "10044"
            };

            CallCheckAddress(addressTo);
        }

        public void CallCheckCorrectableAddress2()
        {
            //106-3922253-0057852
            var addressTo = new AddressDTO()
            {
                FullName = "Kristine Reitz",
                Address1 = "One Meadowlands Plaza",
                Address2 = "Hudson Group",
                City = "East Rutherford",
                State = "NJ",
                Country = "US",
                Zip = "07073"
            };

            CallCheckAddress(addressTo);
        }

        public void CallCheckCorrectableAddress()
        {
            //102-6772262-4570640
            var addressTo = new AddressDTO()
            {
                FullName = "maha mana A A khowar",
                Address1 = "182-2115th Avenue",
                Address2 = "doh 3504",
                City = "Springfield Gardens",
                State = "NY",
                Country = "US",
                Zip = "11413"
            };

            CallCheckAddress(addressTo);
        }

        public void CallCheckValidAddress()
        {
            //107-6050813-7403412
            var addressTo = new AddressDTO()
            {
                FullName = "Ahmed Hussein Mohamed ali",
                Address1 = "18221 150TH AVE AUH 230694",
                Address2 = "",
                City = "SPRINGFIELD GARDENS",
                State = "NY",
                Country = "US",
                Zip = "11413",
                ZipAddon = "4001"
            };

            CallCheckAddress(addressTo);
        }

        public void RecheckAllUnshippedOrderAddress()
        {
            using (var db = new UnitOfWork(_log))
            {
                var orders = db.Orders.GetAll().Where(o => o.OrderStatus == OrderStatusEnumEx.Unshipped).ToList();
                foreach (var order in orders)
                {
                    var result = CallCheckAddress(db, order.AmazonIdentifier);
                    
                    //if (result.Status != order.AddressValidationStatus)
                    //{
                    //    order.AddressValidationStatus = result.Status;
                    //}
                }
                db.Commit();
            }
        }

        public void CallCheckAddressByMelissaScrapper(string orderId)
        {
            using (var db = new UnitOfWork(_log))
            {
                var inputAddress = db.Orders.GetAddressInfo(orderId);
                var addressCheckResult = new PersonatorAddressCheckService(_log, _htmlScraper, null)
                    .ScrappingCheckAddress(inputAddress);
                Console.WriteLine("IsNotServedByUSPSNote: " + addressCheckResult.IsNotServedByUSPSNote);
            }
        }

        public void CheckAddressByGoogleAPI(string orderId)
        {
            using (var db = new UnitOfWork(_log))
            {
                var inputAddress = db.Orders.GetAddressInfo(orderId);
                var provider = _company.AddressProviderInfoList.FirstOrDefault(p => p.Type == (int)AddressProviderType.Google);
                var checkService = new GoogleGeocodeAddressCheckService(_log, provider);

                var result = checkService.CheckAddress(CallSource.Service, inputAddress);
                Console.WriteLine(result.ToString());
            }
        }

        public void CheckAddressByFedexAPI(string orderId)
        {
            using (var db = new UnitOfWork(_log))
            {
                var inputAddress = db.Orders.GetAddressInfo(orderId);
                var provider = _company.AddressProviderInfoList.FirstOrDefault(p => p.Type == (int)AddressProviderType.Fedex);
                var checkService = new FedexAddressCheckService(_log, _time, provider, _company.ShortName);

                var result = checkService.CheckAddress(CallSource.Service, inputAddress);
                Console.WriteLine(result.ToString());
            }
        }

        public void CallCheckAddress(string orderId)
        {
            using (var db = new UnitOfWork(_log))
            {
                CallCheckAddress(db, orderId);
            }
        }

        public IList<CheckResult<AddressDTO>> CallCheckAddress(IUnitOfWork db, string orderId)
        {
            var order = db.Orders.GetFiltered(o => o.AmazonIdentifier == orderId).First();
            var orderInfo = db.ItemOrderMappings.GetSelectedOrdersWithItems(null, new[] { order.Id }, includeSourceItems: false).First();
            var addressTo = db.Orders.GetAddressInfo(orderInfo.OrderId);
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var serviceFactory = new ServiceFactory();
            var addressCheckServices = serviceFactory.GetAddressCheckServices(_log,
                _time,
                dbFactory,
                _company.AddressProviderInfoList);
            var priceService = new PriceService(dbFactory);

            var companyAddress = new CompanyAddressService(_company);
            var addressService = new AddressService(addressCheckServices, companyAddress.GetReturnAddress(MarketIdentifier.Empty()), companyAddress.GetPickupAddress(MarketIdentifier.Empty()));
            AddressDTO outAddress = null;
            var validatorService = new OrderValidatorService(_log, _dbFactory, null, null, null, null, priceService, _htmlScraper, addressService, null, null, time, _company);
            
            var result = validatorService.CheckAddress(CallSource.Service, 
                db,
                addressTo,
                order.Id,
                out outAddress);


            Console.WriteLine("Validation result: " + result);

            return result;
        }

        public void CallCheckAddress(AddressDTO addressTo)
        {
            //var result = DoubleCheckAddress(addressTo);

            //Console.WriteLine("Validation result: " + result);
            Console.ReadKey();
        }

    }
}
