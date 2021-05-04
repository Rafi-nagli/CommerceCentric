using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Amazon.Api;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Validation;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.Models;

namespace Amazon.Model.Implementation.Validation
{
    public class ExceededShippingCostChecker
    {
        private ILogService _log;
        private ITime _time;
        private ISystemActionService _actionService;
        private IShipmentApi _stampsRateProvider;
        private IPriceService _priceService;
        private AddressDTO _fromAddress;

        public ExceededShippingCostChecker(ILogService log,
            ISystemActionService actionService,
            IPriceService priceService,
            IShipmentApi stampsRateProvider,
            AddressDTO fromAddress,
            ITime time)
        {
            _log = log;
            _time = time;
            _fromAddress = fromAddress;
            _stampsRateProvider = stampsRateProvider;
            _actionService = actionService;
            _priceService = priceService;
        }

        public void ProcessResult(CheckResult result, IUnitOfWork db, Order dbOrder)
        {
            if (result.IsSuccess)
            {
                if (!dbOrder.OnHold)
                {
                    _log.Debug("Set OnHold by CheckIsExceededShippingCost");
                    dbOrder.OnHold = true;
                    db.Commit();
                }                
            }
        }

        public CheckResult Check(IUnitOfWork db,
            long orderId,
            IList<ListingOrderDTO> orderItems,
            IList<OrderShippingInfoDTO> shippings,
            DTOMarketOrder marketOrder)
        {
            if (!orderItems.Any() || !shippings.Any())
                return new CheckResult() { IsSuccess = false };

            if (marketOrder.UpgradeLevel > 0)
                return new CheckResult() { IsSuccess = false };

            if (marketOrder.Market == (int)MarketType.Groupon)
                return new CheckResult() { IsSuccess = false };

            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;

            decimal paidShppingCost = orderItems.Sum(i => i.ShippingPrice);
            string currency = orderItems.First().ShippingPriceCurrency;
            paidShppingCost = PriceHelper.RougeConvertToUSD(currency, paidShppingCost);
            if (marketOrder.OrderType == (int)OrderTypeEnum.Prime)
            {
                paidShppingCost += orderItems.Sum(oi => AmazonPrimeHelper.GetShippingAmount(oi.Weight));
            }

            decimal? actualShippingsCost = null;
            if (shippings != null && shippings.Any(sh => sh.IsActive))
            {
                actualShippingsCost = shippings.Where(sh => sh.IsActive 
                    && sh.ShippingMethodId != ShippingUtils.FedexSmartPost).Sum(sh => sh.StampsShippingCost ?? 0);                
            }

            _log.Info("CheckIsExceededShippingCost: paid=" + paidShppingCost + " < actual=" + actualShippingsCost);


            if (shippings != null)
            {
                //"Excessive shipping cost. Because priority flat went up I get lots of “excesive” in cases like. We need to ignore those cases…"
                var activeShipping = shippings.FirstOrDefault(sh => sh.IsActive);
                if (activeShipping != null
                    && activeShipping.ShippingMethod != null
                    && activeShipping.ShippingMethod.Id == ShippingUtils.PriorityFlatShippingMethodId)
                    return new CheckResult() { IsSuccess = false };
            }

            #region New Checking
            //price/1.17-shipping price-product cost-2 > -exc

            if (actualShippingsCost != null)
            {
                decimal totalThreashold = 0;
                decimal totalCost = 0;
                bool allHaveCost = true;
                foreach (var item in orderItems)
                {
                    decimal? styleThreashold = null;
                    if (item.StyleId.HasValue)
                    {
                        var featureValue = db.StyleFeatureTextValues.GetFeatureValueByStyleIdByFeatureId(
                            item.StyleId.Value, StyleFeatureHelper.EXCESSIVE_SHIPMENT);
                        if (featureValue != null && !String.IsNullOrEmpty(featureValue.Value))
                        {
                            styleThreashold = StringHelper.TryGetDecimal(featureValue.Value);
                        }
                        var cost = db.StyleItemCaches.GetAllAsDto().Select(i => new { i.Id, i.Cost }).FirstOrDefault(i => i.Id == item.StyleItemId)?.Cost;
                        if (cost.HasValue)
                            totalCost += cost.Value * item.QuantityOrdered;
                        else
                            allHaveCost = false;
                    }
                    if (styleThreashold.HasValue)
                    {
                        totalThreashold += styleThreashold.Value * item.QuantityOrdered;
                    }
                }

                if (allHaveCost)
                {
                    var totalPaid = PriceHelper.RougeConvertToUSD(currency, orderItems.Sum(i => i.ShippingPrice + i.ItemPrice));

                    //Please ignore income disparity<1 like 180-111-825-1659 / 381-205-041-7263
                    if (totalThreashold < 1)
                        totalThreashold = 1;

                    var isValid = totalPaid / 1.17M - actualShippingsCost - totalCost - 2 > -totalThreashold;
                    if (!isValid)
                    {
                        var excpectIncome = totalPaid / 1.17M - actualShippingsCost.Value - totalCost - 2;

                        _log.Info(String.Format("Added Income disparity, income: {0}, totalPaid: {1}, actualShippingCost: {2}, totalCost: {3}",
                            excpectIncome.ToString("C", culture),
                            totalPaid,
                            actualShippingsCost,
                            totalCost));

                        var message = String.Format("Income disparity, income: {0}",
                            excpectIncome.ToString("C", culture));

                        db.OrderComments.Add(new OrderComment()
                        {
                            OrderId = orderId,
                            Message = message,
                            Type = (int)CommentType.System,
                            CreateDate = _time.GetAppNowTime(),
                        });
                        db.Commit();

                        return new CheckResult() { IsSuccess = true };
                    }
                }
            }

            #endregion

            #region Old Checking
            //TASK: When order has 2 robes, and they sent as 2 First class (like 102-1792536-3635439) don’t show Excess ship. cost
            if (shippings.Where(sh => sh.IsActive).All(sh => sh.ShippingMethodId == ShippingUtils.AmazonFirstClassShippingMethodId
                                    || sh.ShippingMethodId == ShippingUtils.FirstClassShippingMethodId
                                    || sh.ShippingMethodId == ShippingUtils.DhlEComSMParcelGroundShippingMethodId
                                    || sh.ShippingMethodId == ShippingUtils.DhlEComSMParcelExpeditedShippingMethodId))
            {
                if (orderItems.All(
                        i => ItemStyleHelper.GetFromItemStyleOrTitle(i.ItemStyle, i.Title) == ItemStyleType.Robe))
                {
                    return new CheckResult() { IsSuccess = false };
                }
            }

            //NOTE: used default threashold: $1
            //NOTE: if price disparity <$2 it's ok
            var threshold = 2.0M; 

            //TASK: When order has 2 or more items and service "Standard" made threashold $2
            if (orderItems.Sum(oi => oi.QuantityOrdered) >= 2
                && ShippingUtils.IsServiceStandard(marketOrder.InitialServiceType))
                threshold = 2.5M;

            var withEmptyThreashold = 0;
            var withNotEmptyThreashold = 0; 
            foreach (var item in orderItems)
            {
                decimal? styleThreashold = null;
                if (item.StyleId.HasValue)
                {
                    var featureValue = db.StyleFeatureTextValues.GetFeatureValueByStyleIdByFeatureId(
                        item.StyleId.Value, StyleFeatureHelper.EXCESSIVE_SHIPMENT);
                    if (featureValue != null && !String.IsNullOrEmpty(featureValue.Value))
                    {
                        styleThreashold = StringHelper.TryGetDecimal(featureValue.Value);
                    }
                }
                if (styleThreashold.HasValue)
                {
                    threshold += styleThreashold.Value * item.QuantityOrdered;
                    withNotEmptyThreashold++;
                }
                else
                {
                    withEmptyThreashold ++;
                }
            }
            //if (withEmptyThreashold > 0)
            //    threshold += 1.0M;

            //if (withNotEmptyThreashold == 0)
            //    threshold = 1.0M;
            
            if (actualShippingsCost > 0 && paidShppingCost > 0 && paidShppingCost + threshold < actualShippingsCost)
            {
                bool isOverchargeSkipped = false;
                decimal totalIntlListingPriceInUSD = 0M;
                decimal totalUsListingPrice = 0;
                if (ShippingUtils.IsInternational(marketOrder.FinalShippingCountry))
                {
                    #region Calc US Shipping Cost
                    decimal? actualUsShippingCost = 0M;

                    var shippingService = ShippingUtils.StandardServiceName; //ShippingUtils.InitialShippingServiceIncludeUpgrade(marketOrder.InitialServiceType.Replace("i:", ""), //convert to local
                        //marketOrder.UpgradeLevel);
                    decimal? paidUsShippingCost = ShippingUtils.GetRougePaidUSShippingAmount(shippingService, orderItems.Sum(i => i.QuantityOrdered));

                    var usRates = RateHelper.GetRougeChipestUSRate(_log,
                        _stampsRateProvider,
                        _fromAddress,
                        marketOrder,
                        shippingService,
                        orderItems,
                        orderItems);

                    if (usRates.Any())
                    {
                        actualUsShippingCost = usRates.Sum(r => r.Amount);
                    }
                    #endregion

                    foreach (var orderItem in orderItems)
                    {
                        totalIntlListingPriceInUSD += PriceHelper.RougeConvertToUSD(orderItem.ItemPriceCurrency, orderItem.ItemPrice);
                        var usListingPrice = GetUSListingPrice(db, orderItem);
                        if (usListingPrice == null)
                        {
                            totalUsListingPrice = 0;
                            break;
                        }
                        totalUsListingPrice += (usListingPrice * orderItem.QuantityOrdered) ?? 0;
                    }

                    decimal? usEarnedValue = ((totalUsListingPrice) + (paidUsShippingCost ?? 0) - (actualUsShippingCost ?? 0));
                    decimal? marketEarnedValue = (totalIntlListingPriceInUSD + paidShppingCost - actualShippingsCost.Value);
                    decimal? howMachEarnedValue = null;
                    if (actualUsShippingCost.HasValue 
                        && paidUsShippingCost.HasValue
                        && totalUsListingPrice > 0
                        && actualShippingsCost.HasValue)
                    {
                        
                        howMachEarnedValue = (totalIntlListingPriceInUSD + paidShppingCost - actualShippingsCost.Value) -
                                             //how much we have earned now
                                             (totalUsListingPrice + paidUsShippingCost.Value - actualUsShippingCost.Value); //how much we have earned if we sell it in US
                                    
                        isOverchargeSkipped = howMachEarnedValue > -threshold; //NOTE: Threashold
                    }

                    if (!isOverchargeSkipped)
                    {
                        //var message = internationalOverchargeSkip ? "No Excessive Shipping cost" : "Excessive Shipping cost";
                        var message = "";
                        if (totalUsListingPrice > 0)
                        {
                            message = String.Format("Income disparity: {0}+{1}-{2}={3} vs {4} => income diff.: {5}",
                                //MarketHelper.GetShortName((int)marketOrder.Market, marketOrder.MarketplaceId),
                                (totalUsListingPrice).ToString("C", culture),
                                (paidUsShippingCost ?? 0).ToString("C", culture),
                                (actualUsShippingCost ?? 0).ToString("C", culture),
                                usEarnedValue?.ToString("C", culture),
                                marketEarnedValue?.ToString("C", culture),
                                (howMachEarnedValue ?? 0).ToString("C", culture));
                        }
                        else
                        {
                            isOverchargeSkipped = true; //SKIP
                            message = "Excessive Shipping validation: no similar US listing";
                        }

                        db.OrderComments.Add(new OrderComment()
                        {
                            OrderId = orderId,
                            Message = message,
                            Type = (int)CommentType.System,
                            CreateDate = _time.GetAppNowTime(),
                        });
                        db.Commit();
                    }
                }
                else
                {
                    //сделай пока $3.02 threashold
                    var localThreshold = Math.Max(threshold, 3.02M);
                    if (paidShppingCost + localThreshold < actualShippingsCost)
                    {
                        var message = String.Format("Paid shipping ({0}) lower shipping cost ({1}) more than threshold ({2})",
                                (paidShppingCost).ToString("C", culture),
                                (actualShippingsCost ?? 0).ToString("C", culture),
                                (localThreshold).ToString("C", culture));
                        db.OrderComments.Add(new OrderComment()
                        {
                            OrderId = orderId,
                            Message = message,
                            Type = (int)CommentType.System,
                            CreateDate = _time.GetAppNowTime(),
                        });
                    }
                    else
                    {
                        //NOTE: Temp Do Nothing
                        isOverchargeSkipped = true;
                    }
                }

                if (!isOverchargeSkipped)
                {
                    db.OrderNotifies.Add(
                        ComposeNotify(orderId,
                            (int) OrderNotifyType.OverchargedShpppingCost,
                            1,
                            paidShppingCost + "<" + actualShippingsCost,
                            _time.GetAppNowTime()));
                    db.Commit();
                }

                if (!isOverchargeSkipped)
                {
                    foreach (var orderItem in orderItems)
                    {
                        if (orderItem.SourceListingId.HasValue)
                        {
                            var listing = db.Listings.Get(orderItem.SourceListingId.Value);
                            if (listing != null)
                            {
                                SystemActionHelper.RequestPriceRecalculation(db, _actionService, listing.Id, null);
                                if (listing.Market == (int)MarketType.Walmart) //NOTE: need to update Second Day flag
                                    SystemActionHelper.RequestItemUpdate(db, _actionService, listing.Id, null);
                            }
                        }
                    }
                }

                return new CheckResult() { IsSuccess = !isOverchargeSkipped };
            }
            #endregion

            return new CheckResult() { IsSuccess = false };
        }

        private decimal? GetUSListingPrice(IUnitOfWork db, ListingOrderDTO item)
        {
            var styleItemId = item.SourceStyleItemId ?? item.StyleItemId;
            var cheapestUSListing = db.Items.GetAllViewAsDto()
                .Where(l => l.StyleItemId == styleItemId && l.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId)
                .OrderBy(l => l.CurrentPrice)
                .FirstOrDefault();
            if (cheapestUSListing != null)
            {
                var usPrice = cheapestUSListing.SalePrice ?? cheapestUSListing.CurrentPrice;
                if (cheapestUSListing.IsPrime)
                    usPrice -= _priceService.GetPrimePart((decimal?)cheapestUSListing.Weight);

                return usPrice;
            }
            return null;
        }

        private OrderNotify ComposeNotify(
            long orderId, 
            int type, 
            int status, 
            string message,
            DateTime when)
        {
            return new OrderNotify()
            {
                OrderId = orderId,
                Status = status,
                Type = type,
                Message = message,
                CreateDate = when
            };
        }
    }
}
