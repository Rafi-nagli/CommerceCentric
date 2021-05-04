using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Validation;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Validation;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.Implementation.OrderValidation.Checkers;
using Amazon.Model.Implementation.Validation;
using Amazon.Model.Implementation.Validation.Checkers;


namespace Amazon.Model.Implementation
{
    public class OrderValidatorService : IOrderValidatorService
    {
        private ILogService _log;
        private IEmailService _emailService;
        private ISettingsService _settings;
        private ISystemActionService _actionService;
        private IPriceService _priceService;
        private IAddressService _addressService;
        private IOrderHistoryService _orderHistory;
        private IHtmlScraperService _htmlScraper;
        private AddressDTO _returnAddress;
        private IShipmentApi _stampsRateApi;
        private ITime _time;
        private CompanyDTO _company;
        private IDbFactory _dbFactory;

        public OrderValidatorService(
            ILogService log, 
            IDbFactory dbFactory,
            IEmailService emailService,
            ISettingsService settings,
            IOrderHistoryService orderHistory,
            ISystemActionService actionService,
            IPriceService priceService,
            IHtmlScraperService htmlScraper,
            IAddressService addressService,
            AddressDTO returnAddress,
            IShipmentApi stampsRateApi,
            ITime time,
            CompanyDTO company)
        {
            _log = log;
            _dbFactory = dbFactory;
            _time = time;
            _emailService = emailService;
            _settings = settings;
            _orderHistory = orderHistory;
            _actionService = actionService;
            _priceService = priceService;
            _htmlScraper = htmlScraper;
            _addressService = addressService;
            _returnAddress = returnAddress;
            _stampsRateApi = stampsRateApi;
            _company = company;
        }

        public void OnNotFoundAllListings(DTOMarketOrder marketOrder,
            IList<ListingOrderDTO> sourceOrderItems)
        {
            _emailService.SendSystemEmailToAdmin(
                                   "Listing items weren't found for order, orderId: " + marketOrder.OrderId + ", market: " + marketOrder.Market + " - " + marketOrder.MarketplaceId,
                                   "Items: " + String.Join(",", sourceOrderItems.Select(i => "SKU: " + i.SKU).ToList()));
        }
                

        public IList<CheckResult<AddressDTO>> CheckAddress(CallSource callSource,
            IUnitOfWork db,
            AddressDTO address,
            long? orderId,
            out AddressDTO addressWithCorrection)
        {
            return new AddressChecker(_log, _dbFactory, _addressService, _orderHistory, _time).CheckAddress(CallSource.Service,
                    db,
                    address,
                    orderId,
                    out addressWithCorrection);
        }

        public void ShippingValidationStep(IUnitOfWork db,
            IList<ListingOrderDTO> orderItems,
            IList<ListingOrderDTO> sourceOrderItems,
            IList<OrderShippingInfoDTO> shippings,
            DTOMarketOrder marketOrder,
            Order dbOrder)
        {
            var exceededChecker = new ExceededShippingCostChecker(_log,
                _actionService,
                _priceService,
                _stampsRateApi,
                _returnAddress,
                _time);
            var resultIsExceededShippingCost = exceededChecker.Check(db,
                dbOrder.Id,
                sourceOrderItems,
                shippings,
                marketOrder);
            exceededChecker.ProcessResult(resultIsExceededShippingCost, db, dbOrder);

            var lowerPriceChecker = new LowerItemPriceChecker(_log,
                new PriceService(_dbFactory),
                _time);
            var resultIsLowerItemPrice = lowerPriceChecker.Check(db,
                dbOrder.Id,
                sourceOrderItems,
                shippings,
                marketOrder);
            lowerPriceChecker.ProcessResult(resultIsExceededShippingCost, db, dbOrder);

            var primeChecker = new PrimeShippingChecker(_log,
                _emailService,
                _time);
            var resultPrimeShipping = primeChecker.Check(db,
                dbOrder.Id,
                orderItems,
                shippings,
                marketOrder);
            primeChecker.ProcessResult(resultPrimeShipping, db, dbOrder);


            var replacePhoneChecker = new ReplacePhoneChecker(_log,
                _emailService,
                _time,
                _company);
            var resultReplacePhone = replacePhoneChecker.Check(db,
                marketOrder,
                orderItems);
            replacePhoneChecker.ProcessResult(resultReplacePhone, dbOrder);
        }

        public void OrderValidationStepAlwaysInitial(IUnitOfWork db,
            ITime time,
            DTOMarketOrder marketOrder,
            IList<ListingOrderDTO> orderItems,
            Order dbOrder)
        {
            var upgradeShippingService = new UpgradeShippingServiceChecker(_log, _time);
            var result = upgradeShippingService.Check(db, marketOrder, orderItems);
            upgradeShippingService.ProcessResult(result, dbOrder);

            db.Commit();
        }

        public void OrderValidationStepInitial(IUnitOfWork db,
            ITime time,
            CompanyDTO company,
            DTOMarketOrder marketOrder,
            IList<ListingOrderDTO> orderItems,
            Order dbOrder)
        {
            //var upgradeChecker = new UpgradeOrderChecker(_log, _time);
            //var result = upgradeChecker.Check(db, marketOrder, orderItems);
            //upgradeChecker.ProcessResult(result, dbOrder);
            


            var duplicateChecker = new DuplicateChecker(_log, _emailService, _time);
            var result = duplicateChecker.Check(db, marketOrder, orderItems);
            duplicateChecker.ProcessResult(result, dbOrder);
            

            var insureChecker = new InsureChecker(_log, _orderHistory);
            result = insureChecker.Check(db, marketOrder);
            insureChecker.ProcessResult(result, dbOrder);


            var primeChecker = new PrimeChecker(_log);
            result = primeChecker.Check(db, marketOrder);
            primeChecker.ProcessResult(result, dbOrder);


            var dhlChecker = new DhlChecker(_log);
            result = dhlChecker.Check(db, marketOrder, orderItems);
            dhlChecker.ProcessResult(result, dbOrder);


            var blackListChecker = new BlackListChecker(_log, _time);
            result = blackListChecker.Check(db, marketOrder);
            blackListChecker.ProcessResult(result, dbOrder);
            

            var restockChecker = new RestockChecker(_log, _time);
            result = restockChecker.Check(db, marketOrder, orderItems);
            restockChecker.ProcessResult(result, dbOrder);


            var signConfirmationByOrderCostChecker = new SignConfirmationByOrderCostChecker(_log, _time, db);
            result = signConfirmationByOrderCostChecker.Check(
                marketOrder,
                orderItems);
            signConfirmationByOrderCostChecker.ProcessResult(result, dbOrder);

            var replaceFrenchChecker = new ReplaceFrenchCharsChecker(_log, _time);
            result = replaceFrenchChecker.Check(db,
                marketOrder,
                orderItems,
                (AddressValidationStatus)dbOrder.AddressValidationStatus);
            replaceFrenchChecker.ProcessResult(result, dbOrder);
            
            //var signConfirmationByServiceTypeChecker = new SignConfirmationByServiceTypeChecker(_log, db, _emailService, _time);
            //result = signConfirmationByServiceTypeChecker.Check(marketOrder,
            //    orderItems);
            //signConfirmationByServiceTypeChecker.ProcessResult(result, dbOrder, marketOrder, orderItems);


            //var signConfirmationRemoveBuyerAskedBeforeChecker = new SignConfirmationRemoveBuyerAskedBeforeChecker(_log, db, _emailService, _time);
            //result = signConfirmationRemoveBuyerAskedBeforeChecker.Check(marketOrder,
            //    orderItems);
            //signConfirmationRemoveBuyerAskedBeforeChecker.ProcessResult(result, dbOrder, marketOrder, orderItems);


            var oversoldChecker = new OversoldChecker(_log, _time, _emailService, _settings);
            result = oversoldChecker.Check(db, marketOrder, orderItems);
            oversoldChecker.ProcessResult(result, dbOrder);

            var oversoldOnHoldChecker = new OnHoldOversoldChecker(_log, _time, _emailService);
            result = oversoldOnHoldChecker.Check(db, marketOrder, orderItems);
            oversoldOnHoldChecker.ProcessResult(result, dbOrder);


            CheckAddressStep(db, dbOrder, marketOrder.GetAddressDto());


            var recipientNameChecker = new RecipientNameChecker(_log, 
                _emailService, 
                _time,
                () => CheckAddressStep(db, dbOrder, marketOrder.GetAddressDto()));
            result = recipientNameChecker.Check(db,
                marketOrder,
                orderItems,
                (AddressValidationStatus)dbOrder.AddressValidationStatus);
            recipientNameChecker.ProcessResult(result, dbOrder);
            //NOTE: recheck address if name was corected
            if (result.IsSuccess)
                CheckAddressStep(db, dbOrder, marketOrder.GetAddressDto());


            var zipCodeChecker = new ZipCodeChecker(_log, _time);
            result = zipCodeChecker.Check(db,
                marketOrder,
                orderItems,
                (AddressValidationStatus)dbOrder.AddressValidationStatus);
            zipCodeChecker.ProcessResult(result, dbOrder);

            var shippingStateChecker = new ShippingStateChecker(_log, _time);
            result = shippingStateChecker.Check(db,
                marketOrder,
                orderItems,
                (AddressValidationStatus)dbOrder.AddressValidationStatus);
            shippingStateChecker.ProcessResult(result, dbOrder);

            var phoneNumberChecker = new PhoneNumberChecker(_log, _emailService, _time);
            result = phoneNumberChecker.Check(db,
                marketOrder,
                orderItems,
                (AddressValidationStatus)dbOrder.AddressValidationStatus);
            phoneNumberChecker.ProcessResult(result, dbOrder);


            var dismissFFAddressChecker = new DismissFFAddressChecker(_log, _emailService, db, _time);
            result = dismissFFAddressChecker.Check(db,
                marketOrder,
                orderItems,
                (AddressValidationStatus)dbOrder.AddressValidationStatus);
            dismissFFAddressChecker.ProcessResult(result, dbOrder);

            
            //var addressNotServedByUSPSChecker = new AddressNotServedByUSPSChecker(_log, _htmlScraper, _emailService, _time);
            //result = addressNotServedByUSPSChecker.Check(db,
            //    marketOrder,
            //    orderItems,
            //    (AddressValidationStatus)dbOrder.AddressValidationStatus);
            //addressNotServedByUSPSChecker.ProcessResult(result, dbOrder);


            var internationalExpressChecker = new InternationalExpressChecker(_log, _time);
            result = internationalExpressChecker.Check(marketOrder);
            internationalExpressChecker.ProcessResult(db, result, dbOrder);


            var sameDayChecker = new SameDayChecker(_log, _time);
            result = sameDayChecker.Check(marketOrder);
            sameDayChecker.ProcessResult(db, result, dbOrder);

            
            var noWeightChecker = new NoWeightChecker(_log, _emailService, _time, company);
            result = noWeightChecker.Check(db, marketOrder, orderItems);
            noWeightChecker.ProcessResult(result, dbOrder);


            dbOrder.CheckedDate = _time.GetAppNowTime();
            dbOrder.CheckedTimes = dbOrder.CheckedTimes + 1;

            db.Commit();
        }

        public void OrderValidationStepFinal(IUnitOfWork db,
            ITime time,
            CompanyDTO company,
            DTOMarketOrder marketOrder,
            IList<ListingOrderDTO> orderItems,
            IList<OrderShippingInfoDTO> shippings,
            Order dbOrder)
        {
            //NOTE: Disabled auto set signature confirmation
            //var signConfirmationSendEmailChecker = new SignConfirmationSendEmailChecker(_log, db, _emailService, _time);
            //var result = signConfirmationSendEmailChecker.Check(marketOrder,
            //    dbOrder,
            //    orderItems,
            //    shippings);
            //signConfirmationSendEmailChecker.ProcessResult(result, dbOrder, marketOrder, orderItems);
        }

        public void OrderValidationStepAlways(IUnitOfWork db,
            ITime time,
            IMarketApi api,
            CompanyDTO company,
            DTOMarketOrder marketOrder,
            IList<ListingOrderDTO> orderItems,
            IList<OrderShippingInfoDTO> shippings,
            Order dbOrder)
        {
            var hasCancellationChecker = new HasCancellationChecker(_log, _actionService, _time);
            var result = hasCancellationChecker.Check(db, marketOrder, orderItems);
            hasCancellationChecker.ProcessResult(result, dbOrder);
        }

        private void CheckAddressStep(IUnitOfWork db,
            Order dbOrder,
            AddressDTO sourceAddress)
        {
            _log.Info("CheckAddressStep, orderId=" + dbOrder.AmazonIdentifier);

            AddressDTO correctedAddress = null;
            try
            {
                var checkResults = new AddressChecker(_log, _dbFactory, _addressService, _orderHistory, _time).CheckAddress(CallSource.Service,
                    db,
                    sourceAddress,
                    dbOrder.Id,
                    out correctedAddress);

                bool? isResidential = null;
                var checkStatus = (int)AddressValidationStatus.None;
                var stampsStatus = checkResults.FirstOrDefault(r => r.AdditionalData != null
                                                                    && r.AdditionalData.Any()
                                                                    && r.AdditionalData[0] == OrderNotifyType.AddressCheckStamps.ToString());

                var fedexStatus = checkResults.FirstOrDefault(r => r.AdditionalData != null
                                                                    && r.AdditionalData.Any()
                                                                    && r.AdditionalData[0] == OrderNotifyType.AddressCheckFedex.ToString());


                if (stampsStatus != null) //Get main checker status (by default: stamps.com)
                    checkStatus = stampsStatus.Status;

                if (fedexStatus != null)
                    checkStatus = Math.Max(checkStatus, fedexStatus.Status);

                if (fedexStatus != null) //Get fedex status for isResidential
                    isResidential = fedexStatus.AdditionalData.Count > 1 && StringHelper.ContainsNoCase(fedexStatus.AdditionalData[1], "true");

                //NOTE: May be should write Valid if was provided corrected address
                dbOrder.AddressValidationStatus = checkStatus;

                if (!dbOrder.ShippingAddressIsResidential.HasValue)
                {
                    _log.Info("Set isResidential = " + isResidential);
                    dbOrder.ShippingAddressIsResidential = isResidential;
                }

                var correctedState = fedexStatus != null && fedexStatus.AdditionalData.Count > 2 ? fedexStatus.AdditionalData[2] : null;
                if (!String.IsNullOrEmpty(correctedState) 
                    && correctedState.Length == 2
                    && !StringHelper.IsEqualNoCase(correctedState, dbOrder.ShippingState))
                {
                    dbOrder.IsManuallyUpdated = true;
                    _log.Info("State changed: " + dbOrder.ShippingState + "=>" + correctedState);
                    dbOrder.ManuallyShippingState = correctedState;

                    dbOrder.ManuallyShippingCountry = dbOrder.ShippingCountry;
                    dbOrder.ManuallyShippingCity = dbOrder.ShippingCity;
                    dbOrder.ManuallyShippingZip = dbOrder.ShippingZip;
                    dbOrder.ManuallyShippingZipAddon = dbOrder.ShippingZipAddon;
                    dbOrder.ManuallyShippingAddress1 = dbOrder.ShippingAddress1;
                    dbOrder.ManuallyShippingAddress2 = dbOrder.ShippingAddress2;
                    dbOrder.ManuallyShippingPhone = dbOrder.ShippingPhone;
                    dbOrder.ManuallyPersonName = dbOrder.PersonName;
                }
            }
            catch (Exception ex)
            {
                _log.Error("[Unexpected] CheckAddress unexpected error", ex);
            }

            _log.Info("exist address correction=" + (correctedAddress != null) + ", with final status=" + dbOrder.AddressValidationStatus);
            if (correctedAddress != null)
            {
                dbOrder.IsManuallyUpdated = true;

                dbOrder.ManuallyPersonName = StringHelper.GetFirstNotEmpty(correctedAddress.FullName, dbOrder.PersonName);
                dbOrder.ManuallyShippingCountry = correctedAddress.Country;
                dbOrder.ManuallyShippingAddress1 = correctedAddress.Address1;
                dbOrder.ManuallyShippingAddress2 = correctedAddress.Address2;
                dbOrder.ManuallyShippingCity = correctedAddress.City;
                dbOrder.ManuallyShippingState = correctedAddress.State;
                dbOrder.ManuallyShippingZip = correctedAddress.Zip;
                dbOrder.ManuallyShippingZipAddon = correctedAddress.ZipAddon;
                dbOrder.ManuallyShippingPhone = StringHelper.GetFirstNotEmpty(correctedAddress.Phone, dbOrder.ShippingPhone);
            }
        }

        public void OrderStatusValidationStep(IUnitOfWork db, DTOMarketOrder marketOrder, Order dbOrder)
        {
            //Nothing
        }

        public void OrderListingsValidationStep(IUnitOfWork db, IList<ListingOrderDTO> orderItems, DTOMarketOrder marketOrder, bool isAllListingsFound)
        {
            
        }
    }
}
