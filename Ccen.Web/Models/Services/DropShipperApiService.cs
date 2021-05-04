using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.Model.Models;
using Amazon.Web.ViewModels;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Amazon.Web.Models.Services
{
    public class DropShipperApiService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private ISystemActionService _actionService;
        private IOrderHistoryService _orderHistoryService;

        public DropShipperApiService(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            ISystemActionService actionService,
            IOrderHistoryService orderHistoryService)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _actionService = actionService;
            _orderHistoryService = orderHistoryService;
        }

        public CallResult UpdatePrice(ItemDTO item)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var dbListing = db.Listings.GetAll().FirstOrDefault(l => l.SKU == item.SKU);
                if (dbListing == null)
                {
                    return new CallResult()
                    {
                        Status = CallStatus.Fail,
                        Message = "Unable to find listing: " + item.SKU
                    };
                }

                if (dbListing.CurrentPrice != item.CurrentPrice)
                    _log.Info("Price changed, SKU=" + dbListing.SKU + ", " + dbListing.CurrentPrice + "=>" + item.CurrentPrice);
                dbListing.CurrentPrice = item.CurrentPrice;
                dbListing.PriceUpdateRequested = true;

                db.Commit();
            }

            return new CallResult()
            {
                Status = CallStatus.Success
            };
        }

        public CallResult UpdateQuantity(ItemDTO item)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var itemView = db.Items.GetAllViewAsDto().FirstOrDefault(i => i.SKU == item.SKU);
                if (itemView == null)
                {
                    return new CallResult()
                    {
                        Status = CallStatus.Fail,
                        Message = "Unable to find item: " + item.SKU
                    };
                }
                if (!itemView.StyleItemId.HasValue)
                {
                    return new CallResult()
                    {
                        Status = CallStatus.Fail,
                        Message = "Item hasn't linked StyleItemId: " + item.SKU
                    };
                }

                var dbStyleItem = db.StyleItems.GetAll().FirstOrDefault(si => si.Id == itemView.StyleItemId);
                if (dbStyleItem == null)
                {
                    return new CallResult()
                    {
                        Status = CallStatus.Fail,
                        Message = "Unable to find styleItem: " + dbStyleItem.Id
                    };
                }

                if (dbStyleItem.Quantity != item.RealQuantity)
                    _log.Info("Quantity changed, SKU=" + item.SKU + ", " + dbStyleItem.Quantity + "=>" + item.RealQuantity);
                dbStyleItem.Quantity = item.RealQuantity;
                dbStyleItem.QuantitySetDate = _time.GetAppNowTime();
                dbStyleItem.QuantitySetBy = null;

                db.Commit();

                SystemActionHelper.RequestQuantityDistribution(db, _actionService, dbStyleItem.StyleId, null);
            }

            return new CallResult()
            {
                Status = CallStatus.Success
            };
        }

        public CallResult UpdateShipments(DTOOrder order)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var intId = long.Parse(order.OrderId);
                var dbOrder = db.Orders.GetAll().FirstOrDefault(o => o.Id == intId);
                if (dbOrder == null)
                    return new CallResult()
                    {
                        Status = CallStatus.Fail,
                        Message = "Unable to find order: " + order.OrderId
                    };

                if (dbOrder.OrderStatus == OrderStatusEnumEx.Shipped)
                    return new CallResult()
                    {
                        Status = CallStatus.Fail,
                        Message = "Already shipped"
                    };

                var dbShippings = db.OrderShippingInfos.GetAll().Where(sh => sh.OrderId == dbOrder.Id).ToList();
                //if (!dbShippings.Any(sh => String.IsNullOrEmpty(sh.TrackingNumber)))
                //{
                //    return new CallResult()
                //    {
                //        Status = CallStatus.Fail,
                //        Message = "All shippings has tracking numbers"
                //    };
                //}

                var dbOrderItems = db.OrderItems.GetAll().Where(oi => oi.OrderId == dbOrder.Id).ToList();
                foreach (var shipping in order.ShippingInfos)
                {
                    foreach (var shippingItem in shipping.Items)
                    {
                        var existItem = dbOrderItems.FirstOrDefault(oi => oi.ItemOrderIdentifier == shippingItem.ItemOrderId);
                        if (existItem == null)
                        {
                            return new CallResult()
                            {
                                Status = CallStatus.Fail,
                                Message = "Unable to find order item: " + shippingItem.ItemOrderId
                            };
                        }
                    }
                }

                //TODO: validation if we have more shippings then items


                foreach (var shipping in dbShippings)
                {
                    if (String.IsNullOrEmpty(shipping.TrackingNumber))
                    {
                        _log.Info("Remove old shipping: " + shipping.Id);
                        db.OrderShippingInfos.Remove(shipping);
                    }
                }
                db.Commit();
                var newShippings = order.ShippingInfos;
                foreach (var shipping in newShippings)
                {
                    _log.Info("Create new shipping");
                    var newDbShipping = db.OrderShippingInfos.CreateShippingInfo(new RateDTO()
                    {
                        OfferId = shipping.ShipmentOfferId,
                        ProviderType = shipping.ShipmentProviderType,

                        Amount = shipping.StampsShippingCost,
                        InsuranceCost = shipping.InsuranceCost,
                        SignConfirmationCost = shipping.SignConfirmationCost,

                        ShipDate = shipping.ShippingDate.Value,
                    },
                        dbOrder.Id,
                        shipping.ShippingNumber ?? 1,
                        shipping.ShippingMethodId);

                    foreach (var shippingItem in shipping.Items)
                    {
                        _log.Info("Add shipping item: " + shippingItem.ItemOrderId + ", qty: " + shippingItem.Quantity);
                        var existItem = db.OrderItems.GetAll().FirstOrDefault(oi => oi.OrderId == dbOrder.Id
                                                                                    && oi.ItemOrderIdentifier == shippingItem.ItemOrderId);
                        db.ItemOrderMappings.Add(new ItemOrderMapping()
                        {
                            OrderItemId = existItem.Id,
                            ShippingInfoId = newDbShipping.Id,
                            MappedQuantity = shippingItem.Quantity,
                            CreateDate = _time.GetAppNowTime()
                        });
                        db.Commit();
                    }

                    //NOTE: after submit items, prevent fulfilment w/o items
                    newDbShipping.TrackingNumber = shipping.TrackingNumber;
                    newDbShipping.IsVisible = true;
                    newDbShipping.IsActive = true;
                    newDbShipping.LabelPath = "#";
                    newDbShipping.IsFulfilled = false;
                }
                db.Commit();
            }
            return new CallResult()
            {
                Status = CallStatus.Success
            };
        }

        public CallResult CancelOrder(long orderId)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                OrderEditViewModel.CancelOrder(db,
                    _log,
                    _time,
                    _actionService,
                    _orderHistoryService,
                    orderId,
                    null);
            }

            return new CallResult()
            {
                Status = CallStatus.Success
            };
        }

        public IList<DTOOrder> GetRecentByDropShipperId(long dropShipperId)
        {
            var fromDate = _time.GetAppNowTime().AddDays(-10);
            using (var db = _dbFactory.GetRWDb())
            {
                var orders = db.Orders.GetAllAsDto()
                    .Where(o => o.DropShipperId == dropShipperId
                        && o.CreateDate > fromDate)
                    .ToList();

                PrepareOrders(db, orders);

                return orders;
            }
        }

        public DTOOrder GetByOrderId(long id)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var order = db.Orders.GetAllAsDto()
                    .FirstOrDefault(o => o.Id == id);

                PrepareOrders(db, new List<DTOOrder>() { order });

                return order;
            }
        }

        public void PrepareOrders(IUnitOfWork db, IList<DTOOrder> orders)
        {
            var orderIds = orders.Select(o => o.Id).ToList();
            var orderItems = db.OrderItems.GetAllAsListingDto()
                .Where(oi => oi.OrderId.HasValue && orderIds.Contains(oi.OrderId.Value))
                .ToList();

            foreach (var orderItem in orderItems)
            {
                var dbListing = db.Listings.Get(orderItem.Id);
                if (dbListing != null)
                {
                    orderItem.SKU = dbListing.SKU;
                }

                if (orderItem.StyleItemId.HasValue)
                {
                    var dbStyleItem = db.StyleItems.Get(orderItem.StyleItemId.Value);
                    if (dbStyleItem != null)
                    {
                        orderItem.StyleSize = dbStyleItem.Size;
                        orderItem.StyleColor = dbStyleItem.Color;
                    }
                }

                if (orderItem.StyleId.HasValue)
                {
                    var styleString = db.Styles.GetAll().Where(st => st.Id == orderItem.StyleId.Value).Select(st => st.StyleID).FirstOrDefault();
                    if (!String.IsNullOrEmpty(styleString))
                    {
                        orderItem.StyleID = styleString;
                    }
                }
            }

            foreach (var order in orders)
            {
                var shippings = db.OrderShippingInfos.GetAll().Where(sh => sh.OrderId == order.Id
                    && sh.IsActive).ToList();

                if (order.OrderStatus == OrderStatusEnumEx.PartiallyShipped)
                {
                    order.OrderStatus = OrderStatusEnumEx.Unshipped;
                }
                if (shippings.Any() 
                    && shippings.All(sh => !String.IsNullOrEmpty(sh.TrackingNumber))
                    && order.OrderStatus == OrderStatusEnumEx.Unshipped)
                {
                    order.OrderStatus = OrderStatusEnumEx.Shipped;
                }


                order.Items = orderItems.Where(oi => oi.OrderId == order.Id).ToList();
                order.Quantity = order.Items.Sum(i => i.QuantityOrdered);

                order.MarketOrderId = order.Id.ToString();

                foreach (var item in order.Items)
                {
                    item.StyleId = null;
                    item.StyleItemId = null;
                    item.SourceListingId = null;
                    item.SourceStyleItemId = null;
                }
            }
        }

        public IList<StyleEntireDto> GetStyles(MarketType market, string marketplaceId)
        {
            using (var db = _dbFactory.GetRDb())
            {
                if (!String.IsNullOrEmpty(marketplaceId))
                    marketplaceId = marketplaceId.ToUpper();

                var items = db.Items.GetAllViewAsDto().Where(i => i.Market == (int)market && i.MarketplaceId == marketplaceId).ToList();
                var styleIds = items.Where(i => i.StyleId.HasValue).Select(i => i.StyleId.Value).ToList();

                var styles = db.Styles.GetAllAsDtoEx().Where(st => styleIds.Contains(st.Id)).ToList();
                var styleItems = db.StyleItems.GetAllAsDto().Where(si => styleIds.Contains(si.StyleId)).ToList();
                var styleItemIds = styleItems.Select(si => si.StyleItemId).ToList();
                var styleBarcodes = db.StyleItemBarcodes.GetAllAsDto().Where(si => styleItemIds.Contains(si.StyleItemId)).ToList();
                var styleImages = db.StyleImages.GetAllAsDto().Where(sim => styleIds.Contains(sim.StyleId)).ToList();
                var styleFeatures = db.StyleFeatureValues.GetAllWithFeature().Where(st => styleIds.Contains(st.StyleId)).ToList();
                styleFeatures.AddRange(db.StyleFeatureTextValues.GetAllWithFeature().Where(st => styleIds.Contains(st.StyleId)).ToList());

                IList<StyleEntireDto> results = new List<StyleEntireDto>();
                foreach (var style in styles)
                {
                    style.StyleItems = styleItems.Where(si => si.StyleId == style.Id).ToList();
                    foreach (var si in style.StyleItems)
                    {
                        si.Barcodes = styleBarcodes.Where(sib => sib.StyleItemId == si.StyleItemId).ToList();
                    }
                    style.Images = styleImages.Where(sim => sim.StyleId == style.Id).ToList();
                    style.StyleFeatures = styleFeatures.Where(sf => sf.StyleId == style.Id).ToList();

                    results.Add(style);
                }

                return results;
            }
        }

        public IList<ParentItemDTO> GetItems(MarketType market, string marketplaceId)
        {
            using (var db = _dbFactory.GetRDb())
            {
                if (!String.IsNullOrEmpty(marketplaceId))
                    marketplaceId = marketplaceId.ToUpper();

                var parentItems = db.ParentItems.GetAllAsDto().Where(pi => pi.Market == (int)market && pi.MarketplaceId == marketplaceId).ToList();
                var items = db.Items.GetAllViewAsDto().Where(i => i.Market == (int)market && i.MarketplaceId == marketplaceId).ToList();

                IList<ParentItemDTO> results = new List<ParentItemDTO>();
                foreach (var parentItem in parentItems)
                {
                    parentItem.Variations = items.Where(i => i.ParentASIN == parentItem.ASIN).ToList();
                    results.Add(parentItem);
                }

                return results;
            }
        }

        public IList<StyleEntireDto> GetQuantities(MarketType market, string marketplaceId)
        {
            using (var db = _dbFactory.GetRDb())
            {
                if (!String.IsNullOrEmpty(marketplaceId))
                    marketplaceId = marketplaceId.ToUpper();

                var items = db.Items.GetAllViewAsDto().Where(i => i.Market == (int)market && i.MarketplaceId == marketplaceId).ToList();
                var styleIds = items.Where(i => i.StyleId.HasValue).Select(i => i.StyleId.Value).ToList();

                var styles = db.Styles.GetAllAsDtoEx().Where(st => styleIds.Contains(st.Id)).ToList();
                var styleItems = db.StyleItems.GetAllAsDto().Where(si => styleIds.Contains(si.StyleId)).ToList();
                var styleItemIds = styleItems.Select(si => si.StyleItemId).ToList();

                IList<StyleEntireDto> results = new List<StyleEntireDto>();
                foreach (var style in styles)
                {
                    style.StyleItems = styleItems.Where(si => si.StyleId == style.Id).ToList();
                    foreach (var si in style.StyleItems)
                    {
                        var item = items.FirstOrDefault(i => i.StyleItemId == si.StyleItemId);
                        si.Quantity = item?.RealQuantity ?? 0;
                        si.QuantitySetDate = _time.GetAppNowTime();
                    }

                    results.Add(style);
                }

                return results;
            }
        }
    }
}