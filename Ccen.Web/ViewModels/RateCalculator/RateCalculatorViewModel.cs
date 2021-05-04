using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Validation;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Contracts;
using Amazon.DTO.Inventory;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Mailing;
using Amazon.Web.ViewModels.Results;
using Amazon.DTO.Users;
using UrlHelper = Amazon.Web.Models.UrlHelper;
using Ccen.Web;

namespace Amazon.Web.ViewModels
{
    public class RateCalculatorViewModel
    {
        public AddressViewModel ToAddress { get; set; }
        public AddressViewModel FromAddress { get; set; }

        [Required]
        public int? WeightLb { get; set; }
        [Required]
        public double? WeightOz { get; set; }

        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }
        public decimal? PackageLenght { get; set; }


        public List<MessageString> Messages { get; set; }


        public RateCalculatorViewModel()
        {
            ToAddress = new AddressViewModel();
            FromAddress = new AddressViewModel();
            Messages = new List<MessageString>();
        }


        #region Get Rates

        public CallResult<List<ShippingMethodViewModel>> GetShippingOptionsModel(IUnitOfWork db,
            ITime time,
            ILogService log,
            IShipmentApi rateProvider,
            IWeightService weightService,
            IShippingService shippingService,
            IList<OrderItemRateInfo> orderItems)
        {
            var result = new CallResult<List<ShippingMethodViewModel>>();

            var fromAddress = FromAddress.GetAddressDto();
            var toAddress = ToAddress.GetAddressDto();

            if (AddressHelper.IsEmpty(fromAddress)
                || AddressHelper.IsEmpty(toAddress))
            {
                result.Status = CallStatus.Fail;
                result.Message = "Empty from/to address";
            }


            result = GetShippingOptionsWithRate(db,
                log,
                time,
                rateProvider,
                shippingService,
                fromAddress,
                toAddress,
                time.GetAppNowTime(),
                WeightLb ?? 0,
                (decimal)(WeightOz ?? 0),
                0,
                new OrderRateInfo()
                {
                    Items = orderItems,
                });

            return result;
        }

        public static IList<ShippingMethodViewModel> GetShippingOptions(IUnitOfWork db,
            string countryTo,
            string countryFrom,
            int weightLb,
            decimal weightOz,
            ShipmentProviderType providerType)
        {
            var methodList = GetShippingMethods(db,
                countryFrom,
                countryTo,
                weightLb,
                weightOz,
                providerType);

            return methodList.Select(m => new ShippingMethodViewModel()
            {
                Id = m.Id,
                Name = m.Name
            }).ToList();
        }

        public static CallResult<List<ShippingMethodViewModel>> GetShippingOptionsWithRate(IUnitOfWork db,
            ILogService log,
            ITime time,
            IShipmentApi rateProvider,
            IShippingService shippingService,
            AddressDTO fromAddress,
            AddressDTO toAddress,
            DateTime shipDate,
            int weightLb,
            decimal weightOz,
            decimal insuredValue,
            OrderRateInfo orderInfo)
        {
            var result = new CallResult<List<ShippingMethodViewModel>>();
            var pickupAddress = fromAddress;

            var rateResult = rateProvider.GetAllRate(
                 fromAddress,
                 pickupAddress,
                 toAddress,
                 shipDate,
                 (double)(weightLb * 16 + weightOz),
                 null,
                 0,
                 false,
                 orderInfo,
                 RetryModeType.Fast);

            if (rateResult.Result != GetRateResultType.Success)
            {
                result.Status = CallStatus.Fail;
                result.Message = rateResult.Message;
                return result;
            }

            var methodList = GetShippingMethods(db,
                fromAddress.FinalCountry,
                toAddress.FinalCountry,
                weightLb,
                weightOz,
                rateProvider.Type);


            result.Data = new List<ShippingMethodViewModel>();
            result.Status = CallStatus.Success;

            foreach (var method in methodList)
            {
                var rate = rateResult.Rates.FirstOrDefault(r => r.ServiceIdentifier == method.ServiceIdentifier);

                if (rate != null)
                {
                    //var deliveryDays = time.GetBizDaysCount(rate.ShipDate, rate.DeliveryDate);
                    var deliveryDaysInfo = rate.DeliveryDaysInfo;
                    string providerPrefix = "";
                    switch ((ShipmentProviderType)method.ShipmentProviderType)
                    {
                        case ShipmentProviderType.Amazon:
                            providerPrefix = "AMZ ";
                            break;
                        case ShipmentProviderType.Stamps:
                            providerPrefix = "";
                            break;
                        case ShipmentProviderType.Dhl:
                            providerPrefix = "";
                            break;
                        case ShipmentProviderType.DhlECom:
                            providerPrefix = "";
                            break;
                    }

                    var adjustedAmount = shippingService.ApplyCharges(method.Id, rate.Amount);

                    result.Data.Add(new ShippingMethodViewModel()
                    {
                        Id = method.Id,
                        ProviderPrefix = providerPrefix,
                        Carrier = method.CarrierName,
                        Name = ShippingUtils.PrepareMethodNameToDisplay(method.Name, deliveryDaysInfo),
                        Rate = adjustedAmount,
                    });
                }
            }

            return result;
        }

        private static IList<ShippingMethodDTO> GetShippingMethods(IUnitOfWork db,
            string countryTo,
            string countryFrom,
            int? weightLb,
            decimal? weightOz,
            ShipmentProviderType providerType)
        {
            var isInternational = countryTo != countryFrom;

            var oz = (double)(weightOz + weightLb * 16);

            var providerList = new List<int>() { (int)providerType };
            if (providerType == ShipmentProviderType.Stamps)
                providerList.Add((int)ShipmentProviderType.StampsPriority);

            return db.ShippingMethods
                .GetAllAsDto()
                .Where(sh => providerList.Contains(sh.ShipmentProviderType)
                             && sh.IsInternational == isInternational
                             && (!sh.MaxWeight.HasValue || oz < sh.MaxWeight))
                .ToList();

            //if (!ShippingUtils.IsInternational(countryTo)
            //    && !ShippingUtils.IsInternational(countryFrom) 
            //    && weightLb == 0
            //    && weightOz == 0)
            //{
            //    return db.ShippingMethods.GetAllAsDto().ToList();
            //}
            //if (!ShippingUtils.IsInternational(countryTo)
            //    && !ShippingUtils.IsInternational(countryFrom)) //(string.IsNullOrEmpty(countryTo) || countryTo == "US") && (string.IsNullOrEmpty(countryFrom) || countryFrom == "US"))
            //{
            //    return db.ShippingMethods
            //        .GetAllAsDto()
            //        .Where(m => (!(weightLb > 0) && weightOz <= 16) || m.AllowOverweight || m.IsInternational)
            //        .ToList();
            //}

            ////TODO: m.b. add country correction / isInternational logic
            //return db.ShippingMethods.GetAllAsDto().Where(m => m.IsInternational == ((countryFrom != countryTo) || string.IsNullOrEmpty(countryFrom) || string.IsNullOrEmpty(countryTo))
            //    && ((!(weightLb > 0) && weightOz <= 16) || m.AllowOverweight || ((countryFrom != countryTo) || string.IsNullOrEmpty(countryFrom) || string.IsNullOrEmpty(countryTo))))
            //    .ToList();
        }
        #endregion



        public static MailViewModel GetByOrderId(IUnitOfWork db,
            IWeightService weightService,
            string id)
        {
            var order = db.Orders.GetMailDTOByOrderId(weightService, id);
            if (order == null)
            {
                return new MailViewModel
                {
                    ToAddress = new AddressViewModel
                    {
                        Address1 = String.Empty,
                        Address2 = String.Empty,
                        FullName = String.Empty,
                        City = String.Empty,
                        USAState = String.Empty,
                        NonUSAState = String.Empty,
                        Country = String.Empty,
                        Zip = String.Empty,
                        Phone = String.Empty,
                        Email = String.Empty,
                        ShipDate = null
                    },
                    Items = new List<MailItemViewModel>(),
                    MarketplaceCode = 1,
                    Notes = String.Empty,
                    OrderStatus = String.Empty,
                    OrderID = String.Empty,
                    OrderEntityId = null,
                    IsPrime = false,

                    ShipmentProviderId = (int)ShipmentProviderType.Stamps,
                    HasBatchLabels = false,
                    HasMailLabels = false,

                    WeightLb = null,
                    WeightOz = null,
                };
            }
            else
            {
                return new MailViewModel
                {
                    ToAddress = new AddressViewModel
                    {
                        Address1 = order.ToAddress.FinalAddress1,
                        Address2 = order.ToAddress.FinalAddress2,
                        FullName = order.ToAddress.FinalFullName,
                        City = order.ToAddress.FinalCity,
                        USAState = StringHelper.ToUpper(order.ToAddress.FinalState),
                        NonUSAState = StringHelper.ToUpper(order.ToAddress.FinalState),
                        Country = order.ToAddress.FinalCountry,
                        Zip = order.ToAddress.FinalZip,
                        ZipAddon = order.ToAddress.FinalZipAddon,
                        Phone = order.ToAddress.FinalPhone,
                        Email = order.ToAddress.BuyerEmail,
                        ShipDate = order.ToAddress.ShipDate
                    },
                    MarketplaceCode = 1,
                    Notes = "",

                    Market = order.Market,
                    MarketplaceId = order.MarketplaceId,

                    OrderStatus = order.OrderStatus,
                    OrderEntityId = order.OrderEntityId,
                    OrderID = order.OrderId,
                    IsPrime = order.OrderType == (int)OrderTypeEnum.Prime,
                    RequireAmazonProvider = ShippingUtils.RequireAmazonProvider(order.OrderType,
                        order.Market,
                        order.ToAddress.FinalCountry,
                        order.SourceShippingService),

                    ShipmentProviderId = order.ShipmentProviderType,
                    HasBatchLabels = order.Labels.Any(l => l.LabelFromType == (int)LabelFromType.Batch),
                    HasMailLabels = order.Labels.Any(l => l.LabelFromType == (int)LabelFromType.Mail),

                    WeightLb = order.WeightLb,
                    WeightOz = order.WeightOz,

                    TotalPrice = order.TotalPrice,
                    TotalPriceCurrency = order.TotalPriceCurrency,

                    Items = order.Items.Select(i => new MailItemViewModel(i)).ToList(),

                    IsInsured = order.IsInsured
                };
            }
        }

        public static IList<MessageString> ValidateQuickReturnLabel(IUnitOfWork db,
            string orderNumber)
        {
            var messages = new List<MessageString>();
            var order = db.Orders.GetAll().FirstOrDefault(o => o.AmazonIdentifier == orderNumber);
            if (ShippingUtils.IsInternational(order.ShippingCountry))
            {
                messages.Add(MessageString.Warning("The International return label cannot be generated automatically"));
            }
            var existReturnLabel = db.MailLabelInfos.GetAllAsDto()
                .Where(m => m.OrderId == order.Id
                    && m.MailReasonId == (int)MailLabelReasonCodes.ReturnLabelReasonCode).ToList();
            if (existReturnLabel.Any())
            {
                messages.Add(MessageString.Warning("Order already has return label"));
            }
            return messages;
        }

        public static ShippingMethodViewModel GetQuickPrintLabelRate(IUnitOfWork db,
            IDbFactory dbFactory,
            IServiceFactory serviceFactory,
            IShippingService shippingService,
            CompanyDTO company,
            ICompanyAddressService companyAddress,
            ILogService log,
            ITime time,
            IWeightService weightService,
            string orderNumber)
        {
            var model = MailViewModel.GetByOrderId(db, weightService, orderNumber);
            model.IsAddressSwitched = true;
            model.FromAddress = model.ToAddress;
            model.ToAddress = MailViewModel.GetFromAddress(companyAddress.GetReturnAddress(MarketIdentifier.Empty()), MarketplaceType.Amazon);
            model.ShipmentProviderId = (int)ShipmentProviderType.Stamps;
            model.ReasonCode = (int)MailLabelReasonCodes.ReturnLabelReasonCode;

            var rateProvider = serviceFactory.GetShipmentProviderByType((ShipmentProviderType)model.ShipmentProviderId,
                 log,
                 time,
                 dbFactory,
                 weightService,
                 company.ShipmentProviderInfoList,
                 null,
                 null,
                 null,
                 null);

            var shippingOptionsResult = model.GetShippingOptionsModel(db, time, log, rateProvider, shippingService, weightService);
            ShippingMethodViewModel chipestRate = null;
            if (shippingOptionsResult.IsSuccess)
            {
                chipestRate = shippingOptionsResult.Data.OrderBy(o => o.Rate).FirstOrDefault();
            }
            return chipestRate;
        }

        public static PrintLabelResult GenerateLabel(
            IUnitOfWork db,
            ILabelService labelService,
            IWeightService weightService,
            IShippingService shippingService,
            MailViewModel model,
            DateTime when,
            long? by)
        {
            var shippingMethod = db.ShippingMethods.GetByIdAsDto(model.ShippingMethodSelected.Value);

            var orderItems = model.Items.Select(i => i.GetItemDto()).ToList() ?? new List<DTOOrderItem>();
            //Fill with additional data
            MailViewModel.FillItemsWithAdditionalInfo(db, weightService, model.OrderID, orderItems);

            var mailInfo = new MailLabelDTO
            {
                FromAddress = model.FromAddress.GetAddressDto(),
                ToAddress = model.ToAddress.GetAddressDto(),

                Notes = model.Notes,
                Instructions = model.Instructions,
                OrderId = model.OrderID,
                WeightLb = model.WeightLb,
                WeightOz = model.WeightOz,
                IsAddressSwitched = model.IsAddressSwitched,
                IsUpdateRequired = model.UpdateAmazon,
                IsCancelCurrentOrderLabel = model.CancelCurrentOrderLabel,

                IsInsured = model.IsInsured,
                IsSignConfirmation = model.IsSignConfirmation,
                TotalPrice = model.TotalPrice,
                TotalPriceCurrency = model.TotalPriceCurrency,

                ShippingMethod = shippingMethod,
                ShipmentProviderType = shippingMethod.ShipmentProviderType,

                Items = orderItems,

                MarketplaceCode = model.MarketplaceCode,
                Reason = model.ReasonCode ?? 0
            };

            return labelService.PrintMailLabel(db,
                shippingService,
                mailInfo,
                when,
                by,
                AppSettings.LabelDirectory,
                AppSettings.TemplateDirectory,
                AppSettings.IsSampleLabels);
        }

        private static Dictionary<int, string> _reasonCodeNames = new Dictionary<int, string>()
        {
            { (int)MailLabelReasonCodes.ReplacementLabelCode, MailLabelReasonCodeHelper.GetName(MailLabelReasonCodes.ReplacementLabelCode) },
            { (int)MailLabelReasonCodes.ReplacingLostDamagedReasonCode, MailLabelReasonCodeHelper.GetName(MailLabelReasonCodes.ReplacingLostDamagedReasonCode) },
            { (int)MailLabelReasonCodes.ResendingOrderCode, MailLabelReasonCodeHelper.GetName(MailLabelReasonCodes.ResendingOrderCode)},
            { (int)MailLabelReasonCodes.ExchangeCode, MailLabelReasonCodeHelper.GetName(MailLabelReasonCodes.ExchangeCode) },
            { (int)MailLabelReasonCodes.ReturnLabelReasonCode, MailLabelReasonCodeHelper.GetName(MailLabelReasonCodes.ReturnLabelReasonCode) },
            { (int)MailLabelReasonCodes.ManualLabelCode, MailLabelReasonCodeHelper.GetName(MailLabelReasonCodes.ManualLabelCode)},
            { (int)MailLabelReasonCodes.OtherCode, MailLabelReasonCodeHelper.GetName(MailLabelReasonCodes.OtherCode)}
        };

        public static string GetReasonName(int reasonCode)
        {
            if (_reasonCodeNames.ContainsKey(reasonCode))
                return _reasonCodeNames[reasonCode];
            return "";
        }

        public static SelectList Reasons
        {
            get
            {
                return new SelectList(_reasonCodeNames.ToList(), "Key", "Value");
            }
        }

        public static SelectList ShippingProviderList
        {
            get
            {
                return new SelectList(ShipmentProviderHelper.GetMainProviderList(), "Key", "Value");
            }
        }

        public static AddressViewModel GetFromAddress(AddressDTO address, MarketplaceType type)
        {
            var model = new AddressViewModel
            {
                FullName = address.FullName,
                Address1 = address.Address1,
                Address2 = address.Address2,
                City = address.City,
                USAState = address.State,
                NonUSAState = address.State,
                Country = address.Country,// ?? Constants.DefaultCountryCode,
                Zip = address.Zip,
                ZipAddon = address.ZipAddon,
                Phone = address.Phone,
                Email = address.BuyerEmail,
                IsVerified = true
            };
            return model;
        }
    }
}