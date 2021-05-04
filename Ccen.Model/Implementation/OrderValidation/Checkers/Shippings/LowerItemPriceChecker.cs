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

namespace Amazon.Model.Implementation.Validation
{
    public class LowerItemPriceChecker
    {
        private ILogService _log;
        private ITime _time;
        private PriceService _rePriceService;

        public LowerItemPriceChecker(ILogService log,
            PriceService rePriceService,
            ITime time)
        {
            _log = log;
            _time = time;
            _rePriceService = rePriceService;
        }

        public void ProcessResult(CheckResult result, IUnitOfWork db, Order dbOrder)
        {
            if (result.IsSuccess)
            {
                if (!dbOrder.OnHold)
                {
                    _log.Debug("Set OnHold by LowerItemPriceChecker");
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

            decimal paidShppingCost = orderItems.Sum(i => i.ShippingPrice);
            string currency = orderItems.First().ShippingPriceCurrency;
            paidShppingCost = PriceHelper.RougeConvertToUSD(currency, paidShppingCost);
            if (marketOrder.OrderType == (int)OrderTypeEnum.Prime)
            {
                paidShppingCost += orderItems.Sum(oi => AmazonPrimeHelper.GetShippingAmount(oi.Weight));
            }

            decimal? actualShippingsCost = null;
            if (shippings != null)
                actualShippingsCost = shippings.Where(sh => sh.IsActive).Sum(sh => sh.StampsShippingCost ?? 0);

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
            
            var threshold = 2.0M;
            var isLower = false;

            if (marketOrder.Market == (int)MarketType.Groupon)
            {
                var totalPaid = marketOrder.TotalPaid ?? marketOrder.TotalPrice;
                decimal totalCost = 0;
                bool allHaveCost = true;
                foreach (var orderItem in orderItems)
                {
                    var itemCost = GetListingCost(db, orderItem);
                    totalCost += (itemCost ?? 0) * orderItem.QuantityOrdered;
                    allHaveCost = allHaveCost || itemCost.HasValue;
                }

                if (allHaveCost && actualShippingsCost.HasValue)
                {
                    if (totalPaid < totalCost + actualShippingsCost + 2)
                    {
                        isLower = true;
                        var message = "Sold at loss. The price is potentially lower than the cost. Total cost of order items: " + totalCost;

                        db.OrderComments.Add(new OrderComment()
                        {
                            OrderId = orderId,
                            Message = message,
                            Type = (int)CommentType.System,
                            CreateDate = _time.GetAppNowTime(),
                        });
                    }
                }
            }

            if ((marketOrder.Market == (int)MarketType.Amazon 
                && marketOrder.OrderType == (int)OrderTypeEnum.Prime)
                || marketOrder.Market == (int)MarketType.Walmart)
            {
                var totalUSListingPrice = 0;
                foreach (var orderItem in orderItems)
                {
                    var sourcePrice = _rePriceService.GetSourcePrice(orderItem.ItemPrice,
                        marketOrder.OrderType == (int)OrderTypeEnum.Prime || 
                            (marketOrder.Market == (int)MarketType.Walmart && marketOrder.ShippingPrice == 0),
                        marketOrder.FulfillmentChannel == "AFN",
                        (decimal)(orderItem.Weight ?? 0),
                        (MarketType)marketOrder.Market,
                        marketOrder.MarketplaceId,
                        orderItem.StyleId,
                        orderItem.StyleItemId);
                    var usPrice = GetUSListingPrice(db, orderItem);
                    if (usPrice.HasValue && sourcePrice < usPrice - threshold)
                    {
                        isLower = true;
                        var message = "Sold at loss. Price lower than Amazon US price: $" + PriceHelper.PriceToString(sourcePrice) + " < $" + PriceHelper.PriceToString(usPrice ?? 0);
                    
                        db.OrderComments.Add(new OrderComment()
                        {
                            OrderId = orderId,
                            Message = message,
                            Type = (int)CommentType.System,
                            CreateDate = _time.GetAppNowTime(),
                        });
                    }
                }
            }
            

            return new CheckResult() { IsSuccess = isLower };
        }

        private decimal? GetUSListingPrice(IUnitOfWork db, ListingOrderDTO item)
        {
            var styleItemId = item.SourceStyleItemId ?? item.StyleItemId;
            var cheapestUSListing = db.Items.GetAllViewAsDto()
                .Where(l => l.StyleItemId == styleItemId && l.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId)
                .OrderBy(l => l.SalePrice ?? l.CurrentPrice)
                .FirstOrDefault();
            if (cheapestUSListing != null)
            {
                var salePrice = cheapestUSListing.SalePrice ?? cheapestUSListing.CurrentPrice;
                if (cheapestUSListing.IsPrime)
                    salePrice -= AmazonPrimeHelper.GetShippingAmount(item.Weight);
                return salePrice * item.QuantityOrdered;
            }
            return null;
        }

        private decimal? GetListingCost(IUnitOfWork db, ListingOrderDTO item)
        {
            var styleItemId = item.SourceStyleItemId ?? item.StyleItemId;
            return db.StyleItemCaches.GetAll().FirstOrDefault(sic => sic.Id == styleItemId)?.Cost;
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
