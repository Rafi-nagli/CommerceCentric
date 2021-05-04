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
using Amazon.Model.General;
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
using Ccen.Web;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;


namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class MailingController : BaseController
    {
        public override string TAG
        {
            get { return "MailingController."; }
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

        [HttpPost]
        public virtual ActionResult Generate(MailViewModel model)
        {
            LogI("Index, model=" + model);

            if (ModelState.IsValid)
            {
                model.OrderID = model.OrderID.RemoveWhitespaces();
                if (model.IsAddressSwitched)
                    model.ToAddress.IsVerified = true;
                else
                    model.FromAddress.IsVerified = true;


                var shipmentProviders = ServiceFactory.GetShipmentProviders(LogService,
                    Time,
                    DbFactory,
                    WeightService,
                    AccessManager.ShipmentProviderInfoList,
                    AppSettings.DefaultCustomType,
                    AppSettings.LabelDirectory,
                    AppSettings.ReserveDirectory,
                    AppSettings.TemplateDirectory);

                var labelService = new LabelService(shipmentProviders, LogService, Time, DbFactory, EmailService, PdfMaker, AddressService);
                var quantityManager = new QuantityManager(LogService, Time);

                var results = model.Generate(LogService,
                    Time,
                    labelService,
                    quantityManager,
                    Db,
                    WeightService,
                    ShippingService,
                    AppSettings.IsSampleLabels,
                    Time.GetAppNowTime(),
                    AccessManager.UserId);

                model.Messages.AddRange(results);
            }
            else
            {
                model.Messages.AddRange(ModelState.GetErrors().Select(m => MessageString.Error(m)));
            }

            LogI("Index, Generate results=");
            model.Messages.ForEach(m => LogI(m.Status + ": " + m.Message));

            return new JsonResult { Data = model, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        public virtual ActionResult GetQuickReturnLabelCost(string orderNumber)
        {
            LogI("GetQuickReturnLabelCost, orderNumber=" + orderNumber);

            var messages = MailViewModel.ValidateQuickReturnLabel(Db, orderNumber);

            ShippingMethodViewModel quickRate = null;
            if (!messages.Any())
            {
                quickRate = MailViewModel.GetQuickPrintLabelRate(Db,
                    DbFactory,
                    ServiceFactory,
                    ShippingService,
                    AccessManager.Company,
                    LogService,
                    Time,
                    WeightService,
                    orderNumber);

                if (quickRate == null)
                {
                    messages.Add(MessageString.Warning("System hasn\'t received any rates"));
                }
            }

            LogI("ChipestRate: " + (quickRate != null ? quickRate.Id.ToString() + ", cost=" + quickRate.Rate : ""));

            return JsonGet(new ValueMessageResult<ShippingMethodViewModel>()
            {
                IsSuccess = quickRate != null,
                Data = quickRate,
                Messages = messages,
            });
        }

        public virtual ActionResult QuickPrintReturnLabel(string orderNumber,
            int shippingMethodId)
        {
            var companyAddress = new CompanyAddressService(AccessManager.Company);

            var model = MailViewModel.GetByOrderId(Db, WeightService, orderNumber);
            model.IsAddressSwitched = true;
            model.FromAddress = model.ToAddress;
            model.ToAddress = MailViewModel.GetFromAddress(companyAddress.GetReturnAddress(MarketIdentifier.Empty()), MarketplaceType.Amazon);
            model.ShipmentProviderId = (int)ShipmentProviderType.Stamps;
            model.ReasonCode = (int)MailLabelReasonCodes.ReturnLabelReasonCode;
            model.OrderComment = "Return label was generated by " + AccessManager.User.Name;
            model.ShippingMethodSelected = shippingMethodId;

            var shipmentProviders = ServiceFactory.GetShipmentProviders(LogService,
                    Time,
                    DbFactory,
                    WeightService,
                    AccessManager.ShipmentProviderInfoList,
                    AppSettings.DefaultCustomType,
                    AppSettings.LabelDirectory,
                    AppSettings.ReserveDirectory,
                    AppSettings.TemplateDirectory);

            var labelService = new LabelService(shipmentProviders, LogService, Time, DbFactory, EmailService, PdfMaker, AddressService);
            var quantityManager = new QuantityManager(LogService, Time);

            var results = model.Generate(LogService,
                Time,
                labelService,
                quantityManager,
                Db,
                WeightService,
                ShippingService,
                AppSettings.IsSampleLabels,
                Time.GetAppNowTime(),
                AccessManager.UserId);

            if (!String.IsNullOrEmpty(model.PrintedLabelUrl))
            {
                results.Insert(0, new MessageString()
                {
                    Message = "Label has been successfully printed, tracking number: " + model.PrintedTrackingNumber
                        + " <a href='" + model.PrintedLabelUrl + "' target='_blank'>download</a>",
                    Status = MessageStatus.Success
                });
            }

            return JsonGet(new ValueMessageResult<EmailAttachmentViewModel>()
            {
                IsSuccess = !String.IsNullOrEmpty(model.PrintedLabelUrl),
                Data = new EmailAttachmentViewModel()
                {
                    ServerFileName = model.PrintedLabelPath,
                    ViewUrl = model.PrintedLabelUrl,
                },
                Messages = results
            });
        }


        public virtual ActionResult CheckCanceledLabel(long orderId)
        {
            DateTime? minLabelToCancelDate;
            var result = MailViewModel.ValidateCanceled(Db, orderId, Time.GetAppNowTime(), out minLabelToCancelDate);

            return new JsonResult { Data = new MessageResult(result, "Current label date: " + DateHelper.ToDateTimeString(minLabelToCancelDate), ""), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult CheckReasonCode(long orderId, int reasonCode)
        {
            var result = MailViewModel.ValidateReasonCode(Db, orderId, reasonCode);

            return new JsonResult() {Data = new MessageResult() {IsSuccess = result}, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        public virtual ActionResult CheckAddress(AddressViewModel model, bool onlyCheck)
        {
            var serviceFactory = new ServiceFactory();
            var addressProviders = AccessManager.Company.AddressProviderInfoList
                .Where(a => a.Type != (int) AddressProviderType.SelfCorrection)
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
        
        public virtual ActionResult GetModelByOrderId(string orderId)
        {
            LogI("GetModelByOrderId, orderId=" + orderId);

            MailViewModel model = null;

            if (!String.IsNullOrWhiteSpace(orderId))
            {
                orderId = orderId.RemoveWhitespaces();
                var orderNumber = OrderHelper.RemoveOrderNumberFormat(orderId);

                model = MailViewModel.GetByOrderId(Db, WeightService, orderId);
            }
            return new JsonResult { Data = ValueResult<MailViewModel>.Success("", model), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        /// <summary>
        /// Retrieves Mail grid rows
        /// </summary>
        /// <param name="request"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public virtual ActionResult GetOrderById([DataSourceRequest] DataSourceRequest request, string orderId)
        {
            LogI("GetOrderById, orderId=" + orderId);

            if (String.IsNullOrWhiteSpace(orderId))
                return new JsonResult { Data = new List<OrderViewModel>(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            orderId = orderId.RemoveWhitespaces();

            try
            {
                var model = new OrderSearchFilterViewModel
                {
                    EqualOrderNumber = orderId
                };
                var items = OrderViewModel.GetFilteredForDisplay(Db, LogService, WeightService, model, AccessManager.IsFulfilment);
                var dataSource = items.Items.ToDataSourceResult(request);
                return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception ex)
            {
                LogE("GetOrderById", ex);
                return new JsonResult { Data = new List<OrderViewModel>(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }
        
        [HttpPost]
        public virtual ActionResult GetShippingOptions(MailViewModel model)
        {
            LogI("GetShippingOptions, countryTo=" + model.ToAddress.Country 
                + ", countryFrom=" + model.FromAddress.Country 
                + ", weightLb" + model.WeightLb 
                + ", weightOz=" + model.WeightOz
                + ", packageLength=" + model.PackageLength
                + ", packageWidth=" + model.PackageWidth
                + ", packageHeight=" + model.PackageHeight);

           var rateProvider = ServiceFactory.GetShipmentProviderByType((ShipmentProviderType)model.ShipmentProviderId,
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
                ShippingService,
                WeightService);

            var result = new ValueResult<List<ShippingMethodViewModel>>(callResult.Status == CallStatus.Success,
                callResult.Message,
                callResult.Data);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
