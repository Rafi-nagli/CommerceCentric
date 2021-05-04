using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models.Calls;
using Amazon.DTO;
using Amazon.DTO.Orders;
using Amazon.Model.Implementation.Markets;

namespace Amazon.Model.Implementation.Sync
{
    public class BaseOrderUpdater
    {
        private IMarketOrderUpdaterApi _api;
        private ILogService _log;
        private ITime _time;

        public BaseOrderUpdater(IMarketOrderUpdaterApi api, ILogService log, ITime time)
        {
            _api = api;
            _log = log;
            _time = time;
        }

        public void UpdateOrders(IUnitOfWork db)
        {
            UpdateOrders(db, null);
        }

        public void UpdateOrders(IUnitOfWork db, IList<string> orderNumbers)
        {
            var shippingLabels = db.OrderShippingInfos.GetOrdersToFulfillAsDTO(_api.Market, _api.MarketplaceId)
                .Where(o => o.OrderStatus != OrderStatusEnumEx.Shipped) //NOTE: Exclude shipped for Walmart
                .ToList();
            var shippingInfoIds = shippingLabels.Select(i => i.Id).ToList();
            var dbShippings = db.OrderShippingInfos.GetFiltered(sh => shippingInfoIds.Contains(sh.Id)).ToList();

            var mailLabels = db.MailLabelInfos.GetInfosToFulfillAsDTO(_api.Market, _api.MarketplaceId)
                .Where(o => o.OrderStatus != OrderStatusEnumEx.Shipped) //NOTE: Exclude shipped for Walmart
                .ToList();
            var mailInfoIds = mailLabels.Select(i => i.Id).ToList();
            var dbMailInfoes = db.MailLabelInfos.GetFiltered(m => mailInfoIds.Contains(m.Id)).ToList();
            
            var combinedList = shippingLabels;
            combinedList.AddRange(mailLabels);

            if (orderNumbers != null && orderNumbers.Any())
            {
                combinedList = combinedList.Where(o => orderNumbers.Contains(o.AmazonIdentifier)).ToList();
            }

            _log.Info("Orders to update, count=" + combinedList.Count);

            if (combinedList.Any())
            {
                foreach (var shipping in combinedList)
                {
                    if (UpdateOrder(db, shipping))
                    {
                        if (shipping.IsFromMailPage)
                        {
                            var dbMailShipping = dbMailInfoes.FirstOrDefault(m => m.Id == shipping.Id);
                            if (dbMailShipping != null)
                                dbMailShipping.IsFulfilled = true;
                        }
                        else
                        {
                            var dbOrderShipping = dbShippings.FirstOrDefault(sh => sh.Id == shipping.Id);
                            if (dbOrderShipping != null)
                                dbOrderShipping.IsFulfilled = true;
                        }
                    }
                }

                db.Commit();
            }
        }

        private bool UpdateOrder(IUnitOfWork db, ShippingDTO shipping)
        {
            _log.Info("Update order: id=" + shipping.OrderId + ", orderId=" + shipping.AmazonIdentifier + ", marketId=" + shipping.MarketOrderId);

            IList<OrderItemDTO> orderItems;

            if (shipping.IsFromMailPage)
            {
                orderItems = db.OrderItems.GetByOrderIdAsDto(shipping.OrderId)
                    //Remove canceled items with 0 price
                    .Where(m => m.ItemPrice > 0 || m.QuantityOrdered > 0).ToList();
            }
            else
            {
                orderItems = db.OrderItems.GetByShippingInfoIdAsDto(shipping.Id)
                    //Remove canceled items with 0 price
                    .Where(m => m.ItemPrice > 0 || m.QuantityOrdered > 0).ToList();
            }

            OrderHelper.PrepareSourceItemOrderId(orderItems);
            orderItems = OrderHelper.GroupBySourceItemOrderId(orderItems);

            DateTime? orderDate = null;
            if (shipping.OrderDate.HasValue)
                orderDate = shipping.OrderDate.Value.ToUniversalTime();
            var shippingDate = shipping.ShippingDate.ToUniversalTime();
            if (orderDate.HasValue && shippingDate < orderDate)
                shippingDate = orderDate.Value.AddHours(2);


            var result = _api.SubmitTrackingInfo(shipping.MarketOrderId,
                shipping.TrackingNumber,
                MarketUrlHelper.GetTrackingUrl(shipping.TrackingNumber, shipping.ShippingMethod.CarrierName),
                shipping.ShippingMethod,
                shipping.ShippingMethod.Name,
                ShippingUtils.GetShippingType(shipping.ShippingMethodId),
                ShippingUtils.FormattedToMarketCurrierName(shipping.ShippingMethod.CarrierName, shipping.ShippingMethod.IsInternational, _api.Market),
                shippingDate,
                orderItems,
                shipping.MessageIdentifier.ToString());

            if (result.Status == CallStatus.Success)
            {
                _log.Info("Order was updated");
            }
            else
            {
                _log.Info("Order update errors: Message=" + result.Message);
                /*
                 * Walmart.Api.WalmartException: No response, statusCode=InternalServerError. 
                 * Details: <?xml version="1.0" encoding="UTF-8" standalone="yes"?><ns4:errors xmlns:ns2="http://walmart.com/mp/orders" xmlns:ns3="http://walmart.com/mp/v3/orders" xmlns:ns4="http://walmart.com/"><ns4:error><ns4:code>INVALID_REQUEST_CONTENT.GMP_ORDER_API</ns4:code><ns4:field>data</ns4:field><ns4:description>Unable to process this request. The Line: 4 of PO: 4576930294354 is in SHIPPED status</ns4:description><ns4:info>Request content is invalid.</ns4:info><ns4:severity>ERROR</ns4:severity><ns4:category>DATA</ns4:category><ns4:causes/><ns4:errorIdentifiers/></ns4:error></ns4:errors> ---> System.Net.WebException: The remote server returned an error: (400) Bad Request.
                 */
            }

            return result.Status == CallStatus.Success;
        }

        public void ResetIsFulfilledStatus(IUnitOfWork db)
        {
            var shippingToReset = (from o in db.Orders.GetAll()
                                join sh in db.OrderShippingInfos.GetAll() on o.Id equals sh.OrderId
                                where o.Market == (int)_api.Market
                                    && (o.MarketplaceId == _api.MarketplaceId || String.IsNullOrEmpty(_api.MarketplaceId))
                                    && sh.IsFulfilled
                                    && !String.IsNullOrEmpty(sh.TrackingNumber)
                                    && sh.IsActive
                                    && o.OrderStatus == OrderStatusEnumEx.Unshipped
                                select sh)
                                .ToList();
            
            foreach (var shipping in shippingToReset)
            {
                _log.Info("Reset IsFulfilled flag, orderId: " + shipping.OrderId + ", shippingId: " + shipping.Id + ", label date: " + shipping.LabelPurchaseDate);
                shipping.IsFulfilled = false;
            }
            db.Commit();
        }
    }
}
