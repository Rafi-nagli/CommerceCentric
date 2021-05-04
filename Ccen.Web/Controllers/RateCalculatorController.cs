using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Validation;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Addresses;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Emails;
using Amazon.Web.ViewModels.Mailing;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Orders;
using Amazon.Web.ViewModels.Pages;
using Amazon.Web.ViewModels.Results;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;


namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllClients)]
    public partial class RateCalculatorController : BaseController
    {
        public override string TAG
        {
            get { return "RateCalculatorController."; }
        }

        public virtual ActionResult Index(string orderId)
        {
            LogI("Index, orderId=" + orderId);

            var company = AccessManager.Company;


            var model = new MailPageViewModel()
            {
                OrderId = orderId,
                ReturnAddress = MailViewModel.GetFromAddress(CompanyAddress.GetReturnAddress(MarketIdentifier.Empty()), MarketplaceType.Amazon),
                PickupAddress = MailViewModel.GetFromAddress(CompanyAddress.GetPickupAddress(MarketIdentifier.Empty()), MarketplaceType.Amazon)
            };

            return View(model);
        }

        public virtual ActionResult CheckAddress(AddressViewModel model, bool onlyCheck)
        {
            var serviceFactory = new ServiceFactory();
            var addressProviders = AccessManager.Company.AddressProviderInfoList
                .Where(a => a.Type != (int)AddressProviderType.SelfCorrection)
                .ToList(); //NOTE: exclude self correction
            var addressCheckServices = serviceFactory.GetAddressCheckServices(LogService,
                Time,
                DbFactory,
                addressProviders);

            var addressService = new AddressService(addressCheckServices,
                null,
                null);

            var sourceAddress = model.GetAddressDto();

            var validatorService = new OrderValidatorService(LogService,
                DbFactory,
                EmailService,
                Settings,
                OrderHistoryService,
                ActionService,
                PriceService,
                HtmlScraper,
                addressService,
                null,
                null,
                Time,
                AccessManager.Company);

            AddressDTO correctedAddress = null;
            var checkResults = validatorService.CheckAddress(CallSource.UI,
                Db,
                sourceAddress,
                null,
                out correctedAddress);
            var isSuccess = checkResults.Any(r => r.Status < (int)AddressValidationStatus.Invalid
                && r.Status != (int)AddressValidationStatus.None);

            AddressViewModel correctedModel = correctedAddress != null ?
                new AddressViewModel(correctedAddress) : null;

            var result = new AddressValidationResultViewModel()
            {
                IsSuccess = isSuccess,
                CheckResults = checkResults,
                CorrectedAddress = correctedModel
            };

            return new JsonResult
            {
                Data = result,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpPost]
        public virtual ActionResult GetShippingOptions(RateCalculatorViewModel model)
        {
            LogI("GetShippingOptions, countryTo=" + model.ToAddress.Country
                + ", countryFrom=" + model.FromAddress.Country
                + ", weightLb" + model.WeightLb
                + ", weightOz=" + model.WeightOz);

            var shipmentProviderTypes = AccessManager.Company.ShipmentProviderInfoList.Where(p => p.Type == (int)ShipmentProviderType.FedexGeneral
                || p.Type == (int)ShipmentProviderType.FedexOneRate
                || p.Type == (int)ShipmentProviderType.Stamps)
                .Select(p => p.Type)
                .ToList();

            var orderItems = new List<OrderItemRateInfo>()
        {
            new OrderItemRateInfo()
            {
                PackageWidth = model.PackageWidth,
                PackageHeight = model.PackageHeight,
                PackageLength = model.PackageLenght,
                Quantity = 1,
            }
        };
            var shippingMethods = new List<ShippingMethodViewModel>();
            foreach (var shipmentProviderId in shipmentProviderTypes)
            {
                var rateProvider = ServiceFactory.GetShipmentProviderByType((ShipmentProviderType)shipmentProviderId,
                        LogService,
                        Time,
                        DbFactory,
                        WeightService,
                        AccessManager.Company.ShipmentProviderInfoList,
                        null,
                        null,
                        null,
                        null);

                var callResult = model.GetShippingOptionsModel(Db,
                        Time,
                        LogService,
                        rateProvider,
                        WeightService,
                        ShippingService,
                        orderItems);

                if (callResult.Status == CallStatus.Success)
                {
                    shippingMethods.AddRange(callResult.Data);
                }
                else
                {
                    var errorResult = new ValueResult<List<ShippingMethodViewModel>>(callResult.Status == CallStatus.Success,
                        callResult.Message,
                        callResult.Data);
                    return Json(errorResult, JsonRequestBehavior.AllowGet);
                }
            }

            var result = new ValueResult<List<ShippingMethodViewModel>>(true,
                "",
                shippingMethods);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}