﻿using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Orders;
using eBay.Api;

namespace Amazon.Model.Implementation.Markets.eBay
{
    public class eBayOrderUpdater
    {
        private eBayApi _api;
        private ILogService _log;
        private ITime _time;

        public eBayOrderUpdater(eBayApi api, ILogService log, ITime time)
        {
            _api = api;
            _log = log;
            _time = time;
        }

        public void UpdateOrders(IUnitOfWork db)
        {
            var shippingLabels = db.OrderShippingInfos.GetOrdersToFulfillAsDTO(_api.Market, _api.MarketplaceId).ToList();
            var shippingInfoIds = shippingLabels.Select(i => i.Id).ToList();
            var dbShippings = db.OrderShippingInfos.GetFiltered(sh => shippingInfoIds.Contains(sh.Id)).ToList();

            var mailLabels = db.MailLabelInfos.GetInfosToFulfillAsDTO(_api.Market, _api.MarketplaceId).ToList();
            var mailInfoIds = mailLabels.Select(i => i.Id).ToList();
            var dbMailInfoes = db.MailLabelInfos.GetFiltered(m => mailInfoIds.Contains(m.Id)).ToList();

            var combinedList = shippingLabels;
            combinedList.AddRange(mailLabels);
            
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

        public bool UpdateOrder(IUnitOfWork db, ShippingDTO shipping)
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

            var allItemWasUpdated = true;
            foreach (var item in orderItems)
            {
                _log.Info("OrderItem, itemId=" + item.ItemOrderIdentifier);

                DateTime? orderDate = null;
                if (shipping.OrderDate.HasValue) 
                    orderDate = shipping.OrderDate.Value.ToUniversalTime();
                var shippingDate = shipping.ShippingDate.ToUniversalTime();
                if (orderDate.HasValue && shippingDate < orderDate)
                    shippingDate = orderDate.Value.AddHours(2);

                var result = _api.UpdateOrder(shipping.MarketOrderId,
                    item.ItemOrderIdentifier,
                    shippingDate,
                    shipping.TrackingNumber,
                    ShippingUtils.FormattedToMarketCurrierName(shipping.ShippingMethod.CarrierName, shipping.ShippingMethod.IsInternational, MarketType.eBay));

                _log.Info(String.Format("Order date: {0}, shipping date: {1}", orderDate, shippingDate));

                allItemWasUpdated = allItemWasUpdated && result.IsSuccess;
                if (result.IsSuccess)
                {
                    _log.Info("Order was updated");
                }
                else
                {
                    _log.Info("Order update errors: ErrorCode=" + result.ErrorCode + ", Message=" + result.Details);
                }
            }

            return allItemWasUpdated;
        }

    }
}
