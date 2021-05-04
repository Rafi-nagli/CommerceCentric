using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Stamps;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.DTO;
using System.Data.Entity.Core.Objects;
using Amazon.DTO.Graphs;
using System.Data.Entity.SqlServer;
using System.Data.Entity;

namespace Amazon.DAL.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public Order GetAddressCorrectionForBuyer(string buyerEmail,
            string address1,
            string address2,
            string city,
            string state,
            string country,
            string zip,
            string zipAddon)
        {
            var query = from o in GetAll()
                join sh in unitOfWork.GetSet<OrderShippingInfo>()
                    on o.Id equals sh.OrderId
                orderby o.OrderDate descending
                where o.IsManuallyUpdated
                      && o.SalesChannel != FulfillmentChannelTypeEx.AFN
                      && o.BuyerEmail == buyerEmail
                      && o.ShippingAddress1 == address1
                      && o.ShippingAddress2 == address2
                      && o.ShippingCity == city
                      && o.ShippingState == state
                      && o.ShippingCountry == country
                      && o.ShippingZip == zip
                      && o.ShippingZipAddon == zipAddon
                      && o.OrderStatus == OrderStatusEnumEx.Shipped
                      && sh.ActualDeliveryDate.HasValue
                select o;

            return query.FirstOrDefault();
        }


        public Order GetById(long orderId)
        {
            return unitOfWork.GetSet<Order>().FirstOrDefault(o => o.Id == orderId);
        }

        public Order GetByOrderNumber(string orderNumber)
        {
            return unitOfWork.GetSet<Order>().FirstOrDefault(o => o.AmazonIdentifier == orderNumber);
        }

        public Order GetByCustomerOrderNumber(string orderNumber)
        {
            return unitOfWork.GetSet<Order>().FirstOrDefault(o => o.AmazonIdentifier == orderNumber || o.CustomerOrderId == orderNumber);
        }

        public IList<Order> GetAllByCustomerOrderNumber(string orderNumber)
        {
            return unitOfWork.GetSet<Order>().Where(o => o.AmazonIdentifier == orderNumber || o.CustomerOrderId == orderNumber).ToList();
        }

        public IList<Order> GetAllByCustomerOrderNumbers(IList<string> orderNumbers)
        {
            return unitOfWork.GetSet<Order>().Where(o => orderNumbers.Contains(o.AmazonIdentifier) || orderNumbers.Contains(o.CustomerOrderId)).ToList();
        }

        public MailLabelDTO GetMailDTOByOrderId(IWeightService weightService, string orderId)
        {
            var query = (from o in unitOfWork.GetSet<Order>()
                         join oi in unitOfWork.GetSet<OrderItem>() on o.Id equals oi.OrderId
                         join l in unitOfWork.GetSet<ViewListing>() on oi.ListingId equals l.Id
                         where (o.AmazonIdentifier == orderId 
                            || o.CustomerOrderId == orderId)
                            && oi.QuantityOrdered > 0
                         select new MailLabelDTO
                         {
                             ToAddress = new AddressDTO
                             {
                                 Address1 = o.ShippingAddress1,
                                 Address2 = o.ShippingAddress2,
                                 City = o.ShippingCity,
                                 Country = o.ShippingCountry,
                                 State = o.ShippingState,
                                 Zip = o.ShippingZip,
                                 ZipAddon = o.ShippingZipAddon,

                                 FullName = o.PersonName,
                                 Phone = o.ShippingPhone,
                                 BuyerEmail = o.BuyerEmail,

                                 IsManuallyUpdated = o.IsManuallyUpdated,
                                 ManuallyFullName = o.ManuallyPersonName,
                                 ManuallyAddress1 = o.ManuallyShippingAddress1,
                                 ManuallyAddress2 = o.ManuallyShippingAddress2,
                                 ManuallyCity = o.ManuallyShippingCity,
                                 ManuallyCountry = o.ManuallyShippingCountry,
                                 ManuallyPhone = o.ManuallyShippingPhone,
                                 ManuallyState = o.ManuallyShippingState,
                                 ManuallyZip = o.ManuallyShippingZip,
                                 ManuallyZipAddon = o.ManuallyShippingZipAddon,
                             },
                             OrderEntityId = o.Id,
                             OrderId = o.AmazonIdentifier,
                             CustomerOrderId = o.CustomerOrderId,
                             Market = o.Market,
                             MarketplaceId = o.MarketplaceId,
                             OrderStatus = o.OrderStatus,
                             ShipmentProviderType = o.ShipmentProviderType,
                             OrderType = o.OrderType,
                             SourceShippingService = o.SourceShippingService,

                             TotalPrice = o.TotalPrice,
                             TotalPriceCurrency = o.TotalPriceCurrency,
                             Items = new List<DTOOrderItem>
                             {
                                new DTOOrderItem
                                {
                                    OrderId = oi.OrderId,
                                    ItemOrderId = oi.ItemOrderIdentifier,
                                    SourceItemOrderId = oi.SourceItemOrderIdentifier,
                                    
                                    Weight = l.Weight ?? 0,
                                    PackageLength = l.PackageLength,
                                    PackageWidth = l.PackageWidth,
                                    PackageHeight = l.PackageHeight,

                                    Quantity = oi.QuantityOrdered,
                                    ItemPriceCurrency = o.TotalPriceCurrency, //NOTE: oi.ItemPriceCurrency is emtpy!
                                    ItemPrice = oi.ItemPrice,
                                    
                                    StyleItemId = l.StyleItemId,
                                    StyleEntityId = l.StyleId,
                                    StyleId = l.StyleString,
                                    Size = l.Size,
                                    ASIN = l.ASIN,
                                    ParentASIN = l.ParentASIN
                                }
                             },

                         }).ToList();

            var orders = query.ToList();
            var mainOrder = orders.FirstOrDefault();
            if (mainOrder != null)
            {
                var byItems = query.OrderByDescending(q => q.Items.First().Quantity)
                    .ToList()
                    .Select(q => q.Items.First())
                    .ToList();

                if (byItems.Any())
                {
                    var weight = byItems.All(i => i.Weight > 0) ? byItems.Sum(i => i.Weight * i.Quantity) : 0;
                    if (weight > 0)
                    {
                        weight = weightService == null ? weight : weightService.AdjustWeight(weight, byItems.Sum(i => i.Quantity));
                    }
                    mainOrder.WeightLb = weight > 16 ? (int)weight / 16 : 0;
                    mainOrder.WeightOz = weight % 16;

                    if (weightService != null)
                    {
                        var packageSize = weightService.ComposePackageSize(byItems);
                        mainOrder.PackageLength = packageSize.PackageLength;
                        mainOrder.PackageWidth = packageSize.PackageWidth;
                        mainOrder.PackageHeight = packageSize.PackageHeight;
                    }

                    mainOrder.Items = byItems;
                }

                var labels = unitOfWork.OrderShippingInfos.GetAllAsDto()
                    .Where(l => l.OrderId == mainOrder.OrderEntityId
                        && !String.IsNullOrEmpty(l.LabelPath)
                        && !l.LabelCanceled)
                    .ToList();

                labels.AddRange(unitOfWork.MailLabelInfos.GetAllAsDto()
                    .Where(l => l.OrderId == mainOrder.OrderEntityId
                        && !l.LabelCanceled));

                mainOrder.Labels = labels;
                mainOrder.OrderStatus = orders.Any(o => o.OrderStatus == OrderStatusEnumEx.Unshipped) ? OrderStatusEnumEx.Unshipped : mainOrder.OrderStatus;
            }
            return mainOrder;
        }

        public IQueryable<PurchaseByDateDTO> GetSalesInfoByDayAndMarket(IList<string> statusListToExclude)
        {
            var emptyList = !statusListToExclude.Any();
            var filteredQuery = unitOfWork.Orders.GetAllAsDto()
                .Where(o => 
                (o.OrderDate.HasValue && (emptyList || ((o.FulfillmentChannel == null || o.FulfillmentChannel != "AFN") && !statusListToExclude.Contains(o.OrderStatus)))))
                .Select(o => new { o.OrderId, o.MarketplaceId, o.Market, Date = DbFunctions.TruncateTime(o.OrderDate.Value) });

            var res = filteredQuery.GroupBy(o => new { o.Market, o.MarketplaceId, o.Date  })
                .Select(i => new PurchaseByDateDTO()
                {
                    Date = i.Key.Date.Value,
                    Market = i.Key.Market,
                    MarketplaceId = i.Key.MarketplaceId,
                    //Price = i.Price ?? 0,
                    Quantity = i.Count()
                });

            return res;            
        }

        public DTOOrder UniversalGetByOrderId(string orderId)
        {
            if (String.IsNullOrEmpty(orderId))
                return null;

            var query = from o in unitOfWork.GetSet<Order>()
                join oi in unitOfWork.GetSet<OrderItem>() on o.Id equals oi.OrderId
                where o.AmazonIdentifier == orderId
                      || o.SalesRecordNumber == orderId //Check eBay long order number
                      || o.MarketOrderId == orderId
                      || oi.RecordNumber == orderId //Check suborders (for eBay combined)
                select new DTOOrder()
                {
                    OrderId = o.AmazonIdentifier
                };

            return query.FirstOrDefault();
        }

        public DTOOrder GetByOrderIdAsDto(string orderId)
        {
            if (String.IsNullOrEmpty(orderId))
                return null;

            var query = from o in GetAllAsDto()
                        where o.OrderId == orderId
                            || o.CustomerOrderId == orderId
                        select o;

            return query.FirstOrDefault();
        }

        public DTOOrder GetByOrderIdAsDto(long orderId)
        {
            var query = from o in GetAllAsDto()
                        where o.Id == orderId
                        select o;

            return query.FirstOrDefault();
        }

        public IQueryable<DTOOrder> GetAllAsDto()
        {
            return GetAll().Select(o => new DTOOrder()
            {
                Id = o.Id,
                Market = o.Market,
                SalesChannel = o.SalesChannel,
                MarketplaceId = o.MarketplaceId,
                FulfillmentChannel = o.FulfillmentChannel,

                OrderId = o.AmazonIdentifier,
                MarketOrderId = o.MarketOrderId,
                CustomerOrderId = o.CustomerOrderId,
                SalesRecordNumber = o.SalesRecordNumber,

                SubOrderNumber = o.SubOrderNumber,
                SubOrderAmountPercent = o.SubOrderAmountPercent,

                DropShipperId = o.DropShipperId,

                SentToDropShipper = o.SentToDropShipper,

                OrderType = o.OrderType,

                OrderStatus = o.OrderStatus,
                OrderDate = o.OrderDate,

                EarliestShipDate = o.EarliestShipDate,
                LatestShipDate = o.LatestShipDate,

                EarliestDeliveryDate = o.EstDeliveryDate,
                LatestDeliveryDate = o.LatestDeliveryDate,
                PersonName = o.PersonName,

                TotalPaid = o.TotalPaid,
                ShippingPaid = o.ShippingPaid,

                TotalPrice = o.TotalPrice,
                TotalPriceCurrency = o.TotalPriceCurrency,

                TaxAmount = o.TaxAmount,

                ShippingPrice = o.ShippingPrice,
                ShippingDiscountAmount = o.ShippingDiscountAmount,
                ShippingTaxAmount = o.ShippingTaxAmount,

                DiscountAmount = o.DiscountAmount,
                DiscountTax = o.DiscountTax,
                DiscountDesc = o.DiscountDesc,

                Quantity = o.Quantity,

                BuyerName = o.BuyerName,
                BuyerEmail = o.BuyerEmail,

                ShippingCountry = o.ShippingCountry,
                ShippingAddress1 = o.ShippingAddress1,
                ShippingAddress2 = o.ShippingAddress2,
                ShippingCity = o.ShippingCity,
                ShippingState = o.ShippingState,
                ShippingZip = o.ShippingZip,
                ShippingZipAddon = o.ShippingZipAddon,
                ShippingPhone = o.ShippingPhone,
                AddressValidationStatus = o.AddressValidationStatus,

                BillingAddress1 = o.BillingAddress1,
                BillingAddress2 = o.BillingAddress2,
                BillingCity = o.BillingCity,
                BillingCountry = o.BillingCountry,
                BillingState = o.BillingState,
                BillingZip = o.BillingZip,
                BillingZipAddon = o.BillingZipAddon,
                BillingPersonName = o.BillingPersonName,
                BillingPhone = o.BillingPhone,
                BillingEmail = o.BillingEmail,

                ShippingCalculationStatus = 0,
                InitialServiceType = o.InitialServiceType,
                ShipmentProviderType = o.ShipmentProviderType,

                SourceOrderStatus = o.SourceOrderStatus,
                SourceShippingService = o.SourceShippingService,
                SourceShippedDate = o.SourceShippedDate,
                PaidDate = o.PaidDate,
                MarketLastUpdatedDate = o.MarketLastUpdatedDate,

                IsSignConfirmation = o.IsSignConfirmation,
                IsInsured = o.IsInsured,

                OnHold = o.OnHold,

                CreateDate = o.CreateDate,
                UpdateDate = o.UpdateDate,
            });
        }

        public IList<DTOOrder> GetOrdersWithSimilarDateAndBuyerAndAddress(DTOMarketOrder orderDto)
        {
            if (orderDto == null)
                throw new ArgumentNullException("orderDto");

            var fromDate = orderDto.OrderDate.Value.AddHours(-24);
            var toDate = orderDto.OrderDate.Value.AddHours(24);

            //1. Retriving similar orders. Сравниваем по имени, адресу доставки, заказанной пижаме, размер
            var query = from o in unitOfWork.Orders.GetAll()
                where o.OrderDate > fromDate
                      && o.OrderDate < toDate
                      && o.OrderStatus != OrderStatusEnumEx.Canceled
                      && o.OrderStatus != OrderStatusEnumEx.Pending
                      //&& o.OrderStatus.ToLower() == "unshipped" && 
                      && o.PersonName == orderDto.PersonName
                      && o.ShippingCountry == orderDto.ShippingCountry
                      && o.ShippingAddress1 == orderDto.ShippingAddress1
                      && o.ShippingAddress2 == orderDto.ShippingAddress2
                      && o.ShippingCity == orderDto.ShippingCity
                      && o.ShippingState == orderDto.ShippingState
                      && o.ShippingZip == orderDto.ShippingZip
                      && o.ShippingZipAddon == orderDto.ShippingZipAddon
                      && o.ShippingPhone == orderDto.ShippingPhone
                      && o.AmazonIdentifier != orderDto.OrderId //exclude itself
                select new DTOOrder
                {
                    Id = o.Id,
                    OrderId = o.AmazonIdentifier,
                    OrderStatus = o.OrderStatus,
                    OrderDate = o.OrderDate
                };

            return query.ToList();
        }


        public AddressDTO GetAddressInfo(long orderId)
        {
            return ToAddressDto(unitOfWork.GetSet<Order>().Where(o => o.Id == orderId)).FirstOrDefault();
        }


        public AddressDTO GetAddressInfo(string orderId)
        {
            return ToAddressDto(unitOfWork.GetSet<Order>().Where(o => o.AmazonIdentifier == orderId)).FirstOrDefault();
        }

        private IQueryable<AddressDTO> ToAddressDto(IQueryable<Order> query)
        {
            return query.Select(order => new AddressDTO
            {
                FullName = order.PersonName,
                BuyerEmail = order.BuyerEmail,

                Address1 = order.ShippingAddress1,
                Address2 = order.ShippingAddress2,
                City = order.ShippingCity,
                State = order.ShippingState,
                Country = order.ShippingCountry,
                Zip = order.ShippingZip,
                ZipAddon = order.ShippingZipAddon,

                IsResidential = order.ShippingAddressIsResidential,
                IsManuallyUpdated = order.IsManuallyUpdated,

                ManuallyFullName = order.ManuallyPersonName,
                ManuallyAddress1 = order.ManuallyShippingAddress1,
                ManuallyAddress2 = order.ManuallyShippingAddress2,
                ManuallyCity = order.ManuallyShippingCity,
                ManuallyState = order.ManuallyShippingState,
                ManuallyCountry = order.ManuallyShippingCountry,
                ManuallyZip = order.ManuallyShippingZip,
                ManuallyZipAddon = order.ManuallyShippingZipAddon,
                ManuallyPhone = order.ManuallyShippingPhone,

                Phone = order.ShippingPhone
            });
        }

        public IQueryable<OrderToTrackDTO> GetUnDeliveredMailInfoes(DateTime when,
            bool excludeRecentlyProcessed,
            IList<long> orderIds)
        {
            var now = when;
            var toDate = when.AddHours(-24);
            var toWhenExpDelDate = when.AddHours(-8);
            var deliveredReCheckPeriod = when.AddDays(-3);
            var oneYear = when.AddYears(-1); //NOTE: Tracking number was resused by USPS

            var query = from o in unitOfWork.GetSet<Order>()
                        join mail in unitOfWork.GetSet<MailLabelInfo>() on o.Id equals mail.OrderId
                        join shM in unitOfWork.GetSet<ShippingMethod>() on mail.ShippingMethodId equals shM.Id
                        where !String.IsNullOrEmpty(mail.TrackingNumber)
                        select new OrderToTrackDTO()
                        {
                            OrderId = o.Id,
                            MailInfoId = mail.Id,
                            OrderNumber = o.AmazonIdentifier,
                            OrderDate = o.OrderDate,
                            OrderStatus = o.OrderStatus,

                            ShippingName = shM.Name,
                            Carrier = shM.CarrierName,
                            TrackingNumber = mail.TrackingNumber,
                            LabelCanceled = mail.LabelCanceled,
                            CancelLabelRequested = mail.CancelLabelRequested,

                            ShipmentProviderType = mail.ShipmentProviderType,
                            ShippingMethodId = mail.ShippingMethodId,
                            
                            BuyDate = mail.BuyDate,

                            ReasonId = mail.ReasonId,

                            EstDeliveryDate = mail.EstimatedDeliveryDate,
                            ActualDeliveryDate = mail.ActualDeliveryDate,
                            DeliveredStatus = mail.DeliveredStatus,
                            IsDelivered = mail.IsDelivered,

                            LastTrackingRequestDate = mail.LastTrackingRequestDate,
                            TrackingRequestAttempts = mail.TrackingRequestAttempts,
                            TrackingStateDate = mail.TrackingStateDate,
                            TrackingStateEvent = mail.TrackingStateEvent,
                            TrackingLocation = mail.TrackingLocation,

                            ShippingAddress = new AddressDTO()
                            {
                                Address1 = o.ShippingAddress1,
                                Address2 = o.ShippingAddress2,
                                City = o.ShippingCity,
                                State = o.ShippingState,
                                Country = o.ShippingCountry,
                                Phone = o.ShippingPhone,
                                Zip = o.ShippingZip,
                                ZipAddon = o.ShippingZipAddon,

                                ManuallyAddress1 = o.ManuallyShippingAddress1,
                                ManuallyAddress2 = o.ManuallyShippingAddress2,
                                ManuallyCity = o.ManuallyShippingCity,
                                ManuallyState = o.ManuallyShippingState,
                                ManuallyCountry = o.ManuallyShippingCountry,
                                ManuallyPhone = o.ManuallyShippingPhone,
                                ManuallyZip = o.ManuallyShippingZip,
                                ManuallyZipAddon = o.ManuallyShippingZipAddon,
                            }
                        };

            if (orderIds == null || !orderIds.Any())
            {
                query = query.Where(sh => sh.OrderStatus == OrderStatusEnumEx.Shipped                                
                                && (!sh.ActualDeliveryDate.HasValue
                                    || sh.ActualDeliveryDate.Value > deliveredReCheckPeriod)
                                //&& (o.EstDeliveryDate <= when)
                                && sh.OrderDate > oneYear);

                if (excludeRecentlyProcessed)
                {
                    query = query.Where(mail => ((mail.TrackingRequestAttempts <= 200)
                                                && (!mail.LastTrackingRequestDate.HasValue ||
                                                    mail.LastTrackingRequestDate < toDate))
                                                || ((mail.EstDeliveryDate.HasValue
                                                    && (EntityFunctions.DiffHours(mail.EstDeliveryDate.Value, now) < 18
                                                        && EntityFunctions.DiffHours(mail.EstDeliveryDate.Value, now) > -8))
                                                        && (!mail.LastTrackingRequestDate.HasValue ||
                                                        mail.LastTrackingRequestDate < toWhenExpDelDate))
                                                );
                }
            }
            else
            {
                query = query.Where(sh => orderIds.Contains(sh.OrderId));
            }            

            return query;
        }

        public IQueryable<OrderToTrackDTO> GetUnDeliveredShippingInfoes(DateTime when,
            bool excludeRecentlyProcessed,
            IList<long> orderIds)
        {
            var now = when;
            var preventCheckDate = when.AddHours(-24);
            var toWhenExpDelDate = when.AddHours(-8);
            var preventCheckDateForDelivered = when.AddDays(-7); //~once per week
            //var minDate = when.AddMonths(-2);
            var deliveryReCheckPeriod = when.AddDays(-3);
            var minOrderDate = when.AddDays(-120); //Stop tracking old orders,  when.AddYears(-1); //NOTE: Tracking number was resused by USPS

            var query = from o in unitOfWork.GetSet<Order>()
                        join sh in unitOfWork.GetSet<OrderShippingInfo>() on o.Id equals sh.OrderId
                        join shM in unitOfWork.GetSet<ShippingMethod>() on sh.ShippingMethodId equals shM.Id
                        where sh.IsActive
                            && !String.IsNullOrEmpty(sh.TrackingNumber)
                        orderby o.OrderDate descending 
                select new OrderToTrackDTO()
                {
                    OrderId = o.Id,
                    ShipmentInfoId = sh.Id,
                    OrderNumber = o.AmazonIdentifier,
                    OrderDate = o.OrderDate,
                    OrderStatus = o.OrderStatus,
                    
                    ShipmentProviderType = sh.ShipmentProviderType,

                    Carrier = shM.CarrierName,
                    ShippingName = shM.Name,
                    TrackingNumber = sh.TrackingNumber,
                    LabelCanceled = sh.LabelCanceled,
                    CancelLabelRequested = sh.CancelLabelRequested,

                    BuyDate = sh.LabelPurchaseDate,
                    ShippingMethodId = sh.ShippingMethodId,

                    EstDeliveryDate = sh.EstimatedDeliveryDate,
                    ActualDeliveryDate = sh.ActualDeliveryDate,
                    DeliveredStatus = sh.DeliveredStatus,
                    IsDelivered = sh.IsDelivered,

                    LastTrackingRequestDate = sh.LastTrackingRequestDate,
                    TrackingRequestAttempts = sh.TrackingRequestAttempts,
                    //TrackingStateType = sh.TrackingStateType,
                    TrackingStateDate = sh.TrackingStateDate,
                    TrackingStateEvent = sh.TrackingStateEvent,
                    TrackingLocation = sh.TrackingLocation,

                    ShippingAddress = new AddressDTO()
                    {
                        Address1 = o.ShippingAddress1,
                        Address2 = o.ShippingAddress2,
                        City = o.ShippingCity,
                        State = o.ShippingState,
                        Country = o.ShippingCountry,
                        Phone = o.ShippingPhone,
                        Zip = o.ShippingZip,
                        ZipAddon = o.ShippingZipAddon,

                        ManuallyAddress1 = o.ManuallyShippingAddress1,
                        ManuallyAddress2 = o.ManuallyShippingAddress2,
                        ManuallyCity = o.ManuallyShippingCity,
                        ManuallyState = o.ManuallyShippingState,
                        ManuallyCountry = o.ManuallyShippingCountry,
                        ManuallyPhone = o.ManuallyShippingPhone,
                        ManuallyZip = o.ManuallyShippingZip,
                        ManuallyZipAddon = o.ManuallyShippingZipAddon,
                    }
                };

            if (orderIds == null || !orderIds.Any())
            {
                query = query.Where(sh => sh.OrderStatus == OrderStatusEnumEx.Shipped                        
                        && (!sh.ActualDeliveryDate.HasValue
                            || sh.ActualDeliveryDate.Value > deliveryReCheckPeriod)
                        && sh.OrderDate > minOrderDate);

                if (excludeRecentlyProcessed)
                {
                    query = query.Where(sh =>
                         (!sh.ActualDeliveryDate.HasValue
                         && sh.TrackingRequestAttempts <= 250
                         && (!sh.LastTrackingRequestDate.HasValue || sh.LastTrackingRequestDate < preventCheckDate))
                         || (sh.ActualDeliveryDate.HasValue && sh.LastTrackingRequestDate < preventCheckDateForDelivered)
                         || ((!sh.ActualDeliveryDate.HasValue
                                && sh.EstDeliveryDate.HasValue
                                && (EntityFunctions.DiffHours(sh.EstDeliveryDate.Value, now) < 18
                                && EntityFunctions.DiffHours(sh.EstDeliveryDate.Value, now) > -8))
                                && (!sh.LastTrackingRequestDate.HasValue ||
                                    sh.LastTrackingRequestDate < toWhenExpDelDate)));
                }
            }
            else
            {
                query = query.Where(sh => orderIds.Contains(sh.OrderId));
            }

            return query;
        }
        
        public IQueryable<DTOOrder> GetNotDeliveredList()
        {
            var query = (from o in unitOfWork.GetSet<Order>()
                         join sh in unitOfWork.GetSet<OrderShippingInfo>() on o.Id equals sh.OrderId

                         join m in unitOfWork.GetSet<ShippingMethod>() on sh.ShippingMethodId equals m.Id// into withM
                         join ztt in unitOfWork.GetSet<ZipToTimezone>() on o.ShippingZip equals ztt.ZIP into withZip

                         join mail in unitOfWork.GetSet<MailLabelInfo>() on o.Id equals mail.OrderId into withMail
                         from mail in withMail.DefaultIfEmpty()

                         from zip in withZip.DefaultIfEmpty()

                         where !String.IsNullOrEmpty(sh.TrackingNumber)
                             && sh.IsActive
                             && (mail != null ? !mail.IsDelivered : !sh.IsDelivered)

                         orderby o.OrderDate descending

                         select new DTOOrder
                         {
                             Id = o.Id,
                             OrderId = o.AmazonIdentifier,
                             ShippingInfoId = mail != null ? mail.Id : sh.Id,
                             ShippingInfoType = mail != null ? 1 : 0,
                             Market = o.Market,
                             MarketplaceId = o.MarketplaceId,

                             AmazonEmail = o.AmazonEmail,
                             BuyerEmail = o.BuyerEmail,
                             OrderDate = o.OrderDate,
                             
                             PersonName = o.PersonName,
                             ShippingCountry = o.ShippingCountry,
                             ShipDate = mail != null ? mail.BuyDate : sh.ShippingDate,
                             ActualDeliveryDate = mail != null ? mail.ActualDeliveryDate : sh.ActualDeliveryDate,
                             IsFeedbackRequested = o.IsFeedbackRequested,
                             FeedbackRequestDate = o.FeedbackRequestDate,
                             Timezone = zip != null ? zip.Timezone : "",

                             ShippingService = m.Name,
                             Carrier = m.CarrierName,
                             TrackingNumber = mail != null ? mail.TrackingNumber : sh.TrackingNumber,
                             TrackingStateSource = mail != null ? mail.TrackingStateSource : sh.TrackingStateSource,
                             TrackingStateEvent = mail != null ? mail.TrackingStateEvent : sh.TrackingStateEvent,
                             TrackingStateDate = mail != null ? mail.TrackingStateDate : sh.TrackingStateDate,

                             NotDeliveredDismiss = mail != null ? mail.NotDeliveredDismiss : sh.NotDeliveredDismiss,
                             NotDeliveredSubmittedClaim = mail != null ? mail.NotDeliveredSubmittedClaim : sh.NotDeliveredSubmittedClaim,
                             NotDeliveredHighlight = mail != null ? mail.NotDeliveredHighlight : sh.NotDeliveredHighlight,
                         });

            return query;
        }

        public IQueryable<DTOOrder> GetListForFeedback()
        {
            var query = (from o in unitOfWork.GetSet<Order>()
                            join sh in unitOfWork.GetSet<OrderShippingInfo>() on o.Id equals sh.OrderId
                            
                            join bl in unitOfWork.GetSet<FeedbackBlackList>() on o.AmazonIdentifier equals bl.OrderId into withBlackList
                            from bl in withBlackList.DefaultIfEmpty()

                            join m in unitOfWork.GetSet<ShippingMethod>() on sh.ShippingMethodId equals m.Id// into withM
                            join ztt in unitOfWork.GetSet<ZipToTimezone>() on o.ShippingZip equals ztt.ZIP into withZip

                            join mail in unitOfWork.GetSet<MailLabelInfo>() on o.Id equals mail.OrderId into withMail
                            from mail in withMail.DefaultIfEmpty()

                            from zip in withZip.DefaultIfEmpty()
                          
                            where (sh.ShippingDate.HasValue || mail.BuyDate.HasValue)
                                && sh.IsActive
                                && String.IsNullOrEmpty(bl.OrderId)

                            orderby o.OrderDate descending
                          
                            select new DTOOrder
                            {
                                Id = o.Id,
                                OrderId = o.AmazonIdentifier,
                                Market = o.Market,
                                MarketplaceId = o.MarketplaceId,

                                AmazonEmail = o.AmazonEmail,
                                BuyerEmail = o.BuyerEmail,
                                OrderDate = o.OrderDate,
                                
                                PersonName = o.PersonName,
                                ShippingCountry = o.ShippingCountry,
                                ShipDate = mail != null ? mail.BuyDate : sh.ShippingDate,
                                ActualDeliveryDate = mail != null ? mail.ActualDeliveryDate : sh.ActualDeliveryDate,
                                IsFeedbackRequested = o.IsFeedbackRequested,
                                FeedbackRequestDate = o.FeedbackRequestDate,
                                Timezone = zip != null ? zip.Timezone : "",

                                ShippingService = m.Name,
                                Carrier = m.CarrierName,
                                TrackingNumber = mail != null ? mail.TrackingNumber : sh.TrackingNumber,
                                TrackingStateEvent = mail != null ? mail.TrackingStateEvent : sh.TrackingStateEvent,
                                TrackingStateDate = mail != null ? mail.TrackingStateDate : sh.TrackingStateDate
                            });

            return query;
        }

        public void UpdateRequestedFeedback(string orderId)
        {
            var order = unitOfWork.GetSet<Order>().FirstOrDefault(o => o.AmazonIdentifier == orderId);
            if (order != null)
            {
                order.FeedbackRequestDate = DateTime.UtcNow;
                order.IsFeedbackRequested = true;
                unitOfWork.Commit();
            }
        }

        public void UpdateRequestedAddressVerify(string orderId)
        {
            var order = unitOfWork.GetSet<Order>().FirstOrDefault(o => o.AmazonIdentifier == orderId);
            if (order != null)
            {
                order.AddressVerifyRequestDate = DateTime.UtcNow;
                unitOfWork.Commit();
            }
        }

        public IList<DTOOrder> GetUnshippedOrders()
        {
            var orders = GetAll().Where(o => o.OrderStatus == OrderStatusEnumEx.Unshipped
                                             || o.OrderStatus == OrderStatusEnumEx.PartiallyShipped)
                .Select(o => new DTOOrder()
                {
                    Market = o.Market,
                    MarketplaceId = o.MarketplaceId,
                    OrderId = o.AmazonIdentifier,
                    OrderDate = o.OrderDate,
                    InitialServiceType = o.InitialServiceType,
                    ShipmentProviderType = o.ShipmentProviderType,
                    LatestDeliveryDate = o.LatestDeliveryDate,
                    LatestShipDate = o.LatestShipDate,
                })
                .ToList();

            return orders;
        }

        public UnshippedInfoDTO GetUnshippedInfo()
        {
            var priceStrings = GetFiltered(o => o.OrderStatus == "Unshipped" || o.OrderStatus == "PartiallyShipped").Select(o => o.TotalPrice).ToList();
            var count = priceStrings.Count;
            var price = priceStrings.Sum();
            return new UnshippedInfoDTO
            {
                Count = count,
                Price = price
            };
        }

        public IList<DTOOrder> ExcludeExistOrderDtos(List<DTOOrder> orders, MarketType market, string marketplaceId)
        {
            var orderIds = orders.Select(o => o.OrderId).ToList();
            var existOrders = GetAll()
                .Where(i => orderIds.Contains(i.AmazonIdentifier)
                    && i.Market == (int)market
                    && (i.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))).Select(o => o.AmazonIdentifier)
                .ToList();
            return orders.Where(o => !existOrders.Contains(o.OrderId)).ToList();
        }

        public IList<Order> GetOrdersWithModifiedStatus(MarketType market,
            string marketplaceId)
        {
            if (market == MarketType.Amazon
                || market == MarketType.AmazonEU
                || market == MarketType.AmazonAU)
            {
                var from = DateTime.Now.AddDays(-4);
                return GetFiltered(o => o.Market == (int) market
                                        && (String.IsNullOrEmpty(marketplaceId) || o.MarketplaceId == marketplaceId)
                                        && o.OrderStatus != o.SourceOrderStatus
                                        && o.OrderDate > from).ToList(); //NOTE: added date filter, don't update old data
            }
            else
            {
                return new List<Order>();
            }
        } 

        public IQueryable<Order> GetOrdersByStatus(MarketType market, 
            string marketplaceId,
            string[] statusList)
        {
            return GetFiltered(o => o.Market == (int) market
                                    && (String.IsNullOrEmpty(marketplaceId) || o.MarketplaceId == marketplaceId)
                                    && statusList.Contains(o.OrderStatus));
        } 

        public Order CreateFromDto(DTOMarketOrder dto, DateTime? when)
        {
            var order = new Order
            {
                Market = dto.Market,
                SalesChannel = dto.SalesChannel,
                MarketplaceId = dto.MarketplaceId,
                FulfillmentChannel = dto.FulfillmentChannel,
                
                AmazonIdentifier = dto.OrderId,
                MarketOrderId = dto.MarketOrderId,
                CustomerOrderId = dto.CustomerOrderId,
                SalesRecordNumber = dto.SalesRecordNumber,

                SubOrderNumber = dto.SubOrderNumber,
                SubOrderAmountPercent = dto.SubOrderAmountPercent,

                DropShipperId = dto.DropShipperId,
                AutoDSSelection = dto.AutoDSSelection,

                PaymentMethod = dto.PaymentMethod,
                PaymentInfo = dto.PaymentInfo,

                DiscountAmount = dto.DiscountAmount,
                DiscountDesc = dto.DiscountDesc,
                TaxAmount = dto.TaxAmount,
                TaxRate = dto.TaxRate,
                DiscountTax = dto.DiscountTax,
                ShippingTaxAmount = dto.ShippingTaxAmount,

                OrderType = dto.OrderType,

                OrderStatus = dto.OrderStatus,
                OrderDate = dto.OrderDate,

                EarliestShipDate = dto.EarliestShipDate,
                LatestShipDate = dto.LatestShipDate,

                EstDeliveryDate = dto.EarliestDeliveryDate,
                LatestDeliveryDate = dto.LatestDeliveryDate,
                PersonName = dto.PersonName,

                TotalPaid = dto.TotalPaid,
                ShippingPaid = dto.ShippingPaid,

                TotalPrice = dto.TotalPrice,
                TotalPriceCurrency = dto.TotalPriceCurrency,

                ShippingPrice = dto.ShippingPrice,
                ShippingDiscountAmount = dto.ShippingDiscountAmount,

                Quantity = dto.Quantity,

                BuyerName = dto.BuyerName,
                BuyerEmail = dto.BuyerEmail,

                ShippingCountry = dto.ShippingCountry,
                ShippingAddress1 = dto.ShippingAddress1,
                ShippingAddress2 = dto.ShippingAddress2,
                ShippingCity = dto.ShippingCity,
                ShippingState = dto.ShippingState,
                ShippingZip = dto.ShippingZip,
                ShippingZipAddon = dto.ShippingZipAddon,
                ShippingPhone = dto.ShippingPhone,
                AddressValidationStatus = dto.AddressValidationStatus,

                ShippingCalculationStatus = 0,
                InitialServiceType = dto.InitialServiceType,
                ShipmentProviderType = dto.ShipmentProviderType,

                SourceOrderStatus = dto.SourceOrderStatus,
                SourceShippingService = dto.SourceShippingService,
                SourceShippedDate = dto.SourceShippedDate,
                PaidDate = dto.PaidDate,
                MarketLastUpdatedDate = dto.MarketLastUpdatedDate,

                IsSignConfirmation = dto.IsSignConfirmation,
                IsInsured = dto.IsInsured,

                OnHold = dto.OnHold,

                CreateDate = when,
                UpdateDate = when
            };
            Add(order);
            unitOfWork.Commit();
            return order;
        }


        public IList<OrderToTrackDTO> GetOrdersToTrack()
        {
            var batchShippingQuery = (from t in unitOfWork.TrackingOrders.GetAll()
                            join sh in unitOfWork.OrderShippingInfos.GetAll() on t.TrackingNumber equals sh.TrackingNumber
                            join o in GetAll() on sh.OrderId equals o.Id
                            where !t.Deleted && !String.IsNullOrEmpty(sh.TrackingNumber)
                            select new OrderToTrackDTO
                            {
                                OrderId = o.Id,
                                
                                TrackingId = t.Id,
                                ShipmentInfoId = sh.Id,

                                OrderNumber = o.AmazonIdentifier,
                                Market = o.Market,
                                MarketplaceId = o.MarketplaceId,

                                EstDeliveryDate = o.EstDeliveryDate,
                                OrderDate = o.OrderDate,
                                PersonName = o.PersonName,
                                BuyerName = o.BuyerName,
                                Comment = t.Comment,
                                BuyDate =sh.ShippingDate,

                                ShipmentProviderType = sh.ShipmentProviderType,
                                TrackingNumber = sh.TrackingNumber,
                                //StampsAccountType = sh.StampsAccountType,

                                ActualDeliveryDate = sh.ActualDeliveryDate,
                                TrackingStateDate = sh.TrackingStateDate,
                                //TrackingStateType = sh.TrackingStateType,
                                TrackingStateEvent = sh.TrackingStateEvent,
                                LastTrackingRequestDate = sh.LastTrackingRequestDate,
                            });

            var shippings = batchShippingQuery.ToList();

            var mailShippingQuery = (from t in unitOfWork.TrackingOrders.GetAll()
                                     join ml in unitOfWork.GetSet<MailLabelInfo>() on t.TrackingNumber equals ml.TrackingNumber
                                     join o in GetAll() on ml.OrderId equals o.Id into withOrder
                                     from o in withOrder.DefaultIfEmpty()
                                     where !t.Deleted && !String.IsNullOrEmpty(ml.TrackingNumber)
                                     select new OrderToTrackDTO
                                     {
                                         OrderId = o.Id,

                                         TrackingId = t.Id,
                                         MailInfoId = ml.Id,
                                         
                                         OrderNumber = o.AmazonIdentifier,
                                         Market = o.Market,
                                         MarketplaceId = o.MarketplaceId,

                                         EstDeliveryDate = o.EstDeliveryDate,
                                         OrderDate = o.OrderDate,
                                         PersonName = o.PersonName,
                                         BuyerName = o.BuyerName,
                                         Comment = t.Comment,
                                         BuyDate = ml.BuyDate,

                                         ShipmentProviderType = ml.ShipmentProviderType,
                                         TrackingNumber = ml.TrackingNumber,
                                         //StampsAccountType = ml.StampsAccountType,
                                         ActualDeliveryDate =  ml.ActualDeliveryDate,
                                         TrackingStateDate = ml.TrackingStateDate,
                                         //TrackingStateType = ml.TrackingStateType,
                                         TrackingStateEvent = ml.TrackingStateEvent,
                                         LastTrackingRequestDate = ml.LastTrackingRequestDate,
                                     });

            var mailShippings = mailShippingQuery.ToList();
            shippings.AddRange(mailShippings);

            shippings = shippings.OrderBy(o => o.OrderId).ThenByDescending(q => q.BuyDate)
                .ToList();

            return shippings;
        }
        
        public IEnumerable<SecondDayOrderDTO> GetSecondDayOrders(DateTime? from, DateTime? to)
        {
            var query = from o in GetAll()
                join sh in unitOfWork.OrderShippingInfos.GetAll() on o.Id equals sh.OrderId
                join mail in unitOfWork.MailLabelInfos.GetAll() on o.Id equals mail.OrderId into withM
                from mail in withM.DefaultIfEmpty()
                        where (o.InitialServiceType == ShippingUtils.SecondDayServiceName 
                            || o.InitialServiceType == ShippingUtils.NextDayServiceName)
                        && (sh == null || sh.IsActive) && !string.IsNullOrEmpty(o.PersonName)
                        && (o.OrderStatus != OrderStatusEnumEx.Shipped || !string.IsNullOrEmpty(mail != null ? mail.TrackingNumber : sh.TrackingNumber))
                select new SecondDayOrderDTO
                {
                    Id = o.Id,

                    OrderNumber = o.AmazonIdentifier,
                    Market = o.Market,
                    MarketplaceId = o.MarketplaceId,

                    TrackingNumber = mail != null ? mail.TrackingNumber : sh.TrackingNumber,
                    ShipmentProviderType = mail != null ? mail.ShipmentProviderType : sh.ShipmentProviderType,
                    EstDeliveryDate = o.EstDeliveryDate,
                    OrderDate = o.OrderDate,
                    ActualDeliveryDate = mail != null ? mail.ActualDeliveryDate : sh.ActualDeliveryDate,
                    PersonName = o.PersonName,
                    BuyerName = o.BuyerName
                };

            if (from.HasValue)
            {
                query = query.Where(q => q.OrderDate.HasValue && q.OrderDate >= from);
            }
            if (to.HasValue)
            {
                query = query.Where(q => q.OrderDate.HasValue && q.OrderDate <= to);
            }
            return query;
        }
    }
}
