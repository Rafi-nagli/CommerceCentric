using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Models;
using Amazon.Core.Models.Search;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.Core.Helpers;
using Amazon.DTO;
using Amazon.DTO.Listings;
using Amazon.DTO.Orders;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Users;

namespace Amazon.DAL.Repositories
{
    public class ItemOrderMappingRepository : Repository<ItemOrderMapping>, IItemOrderMappingRepository
    {
        public ItemOrderMappingRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        

        public void StoreShippingItemMappings(long shippingId, 
            IList<ListingOrderDTO> orderItems, 
            DateTime? when)
        {
            foreach (var item in orderItems)
            {
                unitOfWork.ItemOrderMappings.Add(new ItemOrderMapping
                {
                    ShippingInfoId = shippingId,
                    OrderItemId = item.OrderItemEntityId,
                    MappedQuantity = item.QuantityOrdered,

                    CreateDate = when,
                });
            }
            unitOfWork.Commit();
        }

        public void StorePartialShippingItemMappings(long shippingId, 
            IList<RateItemDTO> mappedItems, 
            IList<ListingOrderDTO> allOrderItems, 
            DateTime? when)
        {
            var shippingItems = new List<ListingOrderDTO>();
            foreach (var mappeditem in mappedItems)
            {
                var orderItem = allOrderItems.First(i => i.ItemOrderId == mappeditem.OrderItemId);                
                shippingItems.Add(new ListingOrderDTO()
                {
                    OrderItemEntityId = orderItem.OrderItemEntityId,
                    QuantityOrdered = mappeditem.Quantity,
                });
            }

            StoreShippingItemMappings(shippingId, shippingItems, when);
        }

        public IEnumerable<ItemDTO> GetLastShippedDateForItem()
        {
            var query = from o in unitOfWork.GetSet<Order>()
                        join oi in unitOfWork.GetSet<ViewOrderItem>() on o.Id equals oi.OrderId
                        join l in unitOfWork.GetSet<ViewListing>() on oi.ListingId equals l.Id
                        where o.OrderStatus == OrderStatusEnumEx.Shipped
                        group new { o, l, oi } by l.ItemId into byItem
                select new ItemDTO
                {
                    Id = byItem.Key,
                    ParentASIN = byItem.Max(i => i.l.ParentASIN),
                    ASIN = byItem.Max(i => i.l.ASIN),
                    StyleId = byItem.Max(i => i.oi.StyleId),
                    LastSoldDate = byItem.Max(i => i.o.OrderDate)
                };

            return query;
        }

        public IEnumerable<ItemDTO> GetLastAnyOrderDateForItem()
        {
            var query = from o in unitOfWork.GetSet<Order>()
                        join oi in unitOfWork.GetSet<ViewOrderItem>() on o.Id equals oi.OrderId
                        join l in unitOfWork.GetSet<ViewListing>() on oi.ListingId equals l.Id
                        group new { o, l, oi } by l.ItemId into byItem
                        select new ItemDTO
                        {
                            Id = byItem.Key,
                            LastSoldDate = byItem.Max(i => i.o.OrderDate)
                        };

            return query;
        }


        private IQueryable<DTOOrder> GetBaseQuery(bool unmaskReferenceStyle)
        {
            var listingsQuery = unitOfWork.Listings.GetViewListingsAsDto(unmaskReferenceStyle);

            var query = (
                from o in unitOfWork.GetSet<Order>()
                join oi in unitOfWork.GetSet<ViewOrderItem>() on o.Id equals oi.OrderId
                join s in unitOfWork.GetSet<OrderShippingInfo>() on o.Id equals s.OrderId
                join l in listingsQuery on oi.ListingId equals l.Id
                join b in unitOfWork.GetSet<OrderBatch>() on o.BatchId equals b.Id into withB
                from b in withB.DefaultIfEmpty()
                join sm in unitOfWork.GetSet<ShippingMethod>() on s.ShippingMethodId equals sm.Id
                join ds in unitOfWork.GetSet<DropShipper>() on o.DropShipperId equals ds.Id into withDS
                from ds in withDS.DefaultIfEmpty()
                join cm in unitOfWork.GetSet<ViewActualOrderComment>() on o.Id equals cm.OrderId into withComments
                from cm in withComments.DefaultIfEmpty()

                where oi.QuantityOrdered > 0
                    && s.IsVisible

                select new DTOOrder
                {
                    Id = o.Id,
                    Market = o.Market,
                    MarketplaceId = o.MarketplaceId,
                    FulfillmentChannel = o.FulfillmentChannel,
                    OrderType = o.OrderType,

                    OrderId = o.AmazonIdentifier,
                    MarketOrderId = o.MarketOrderId,
                    SalesRecordNumber = o.SalesRecordNumber,
                    CustomerOrderId = o.CustomerOrderId,

                    AutoDSSelection = o.AutoDSSelection,
                    DropShipperId = o.DropShipperId,
                    DropShipperName = ds != null ? ds.Name : "",
                    SubOrderNumber = o.SubOrderNumber,

                    OrderStatus = o.OrderStatus,
                    OrderDate = o.OrderDate,
                    EarliestDeliveryDate = o.EstDeliveryDate,
                    LatestDeliveryDate = o.LatestDeliveryDate,
                    EarliestShipDate = o.EarliestShipDate,
                    LatestShipDate = o.LatestShipDate,
                    Quantity = o.Quantity,
                    BuyerName = o.BuyerName,
                    BuyerEmail = o.BuyerEmail,
                    PersonName = o.PersonName,

                    ShippingCountry = o.ShippingCountry,
                    //NOTE: gets from isActive shipping
                    //ShippingService = sm.Name,
                    InitialServiceType = o.InitialServiceType,
                    SourceShippingService = o.SourceShippingService,
                    UpgradeLevel = o.UpgradeLevel,
                    ShipmentProviderType = o.ShipmentProviderType,
                    ShippingCalculationStatus = o.ShippingCalculationStatus,

                    OnHold = o.OnHold,
                    OnHoldUpdateDate = o.OnHoldUpdateDate,
                    TotalPaid = o.TotalPaid,
                    TotalPrice = o.TotalPrice,
                    TotalPriceCurrency = o.TotalPriceCurrency,
                    TaxAmount = o.TaxAmount,
                    ShippingPaid = o.ShippingPaid,
                    ShippingPrice = o.ShippingPrice,
                    ShippingTaxAmount = o.ShippingTaxAmount,
                    DiscountAmount = o.DiscountAmount,
                    DiscountTax = o.DiscountTax,
                    ShippingDiscountAmount = o.ShippingDiscountAmount,
                    
                    BatchId = o.BatchId,
                    BatchName = b != null ? b.Name : "",

                    
                    ShippingAddress1 = o.ShippingAddress1,
                    ShippingAddress2 = o.ShippingAddress2,
                    ShippingCity = o.ShippingCity,
                    ShippingState = o.ShippingState,
                    ShippingZip = o.ShippingZip,
                    ShippingZipAddon = o.ShippingZipAddon,
                    ShippingPhone = o.ShippingPhone,

                    AddressValidationStatus = o.AddressValidationStatus,
                    IsDismissAddressValidation = o.IsDismissAddressValidation,
                    DismissAddressValidationDate = o.DismissAddressValidationDate,
                    AddressVerifyRequestDate = o.AddressVerifyRequestDate,

                    AttachedToOrderId = o.AttachedToOrderId,
                    AttachedToOrderString = o.AttachedToOrderString,

                    ManuallyPersonName = o.ManuallyPersonName,
                    ManuallyShippingAddress1 = o.ManuallyShippingAddress1,
                    ManuallyShippingAddress2 = o.ManuallyShippingAddress2,
                    ManuallyShippingCity = o.ManuallyShippingCity,
                    ManuallyShippingState = o.ManuallyShippingState,
                    ManuallyShippingCountry = o.ManuallyShippingCountry,
                    ManuallyShippingZip = o.ManuallyShippingZip,
                    ManuallyShippingZipAddon = o.ManuallyShippingZipAddon,
                    ManuallyShippingPhone = o.ManuallyShippingPhone,

                    IsManuallyUpdated = o.IsManuallyUpdated,

                    InsuredValue = o.InsuredValue,
                    IsInsured = o.IsInsured,

                    IsSignConfirmation = o.IsSignConfirmation,
                    IsRefundLocked = o.IsRefundLocked,

                    LastCommentMessage = cm.Message,
                    LastCommentDate = cm.CreateDate,
                    LastCommentBy = cm.CreatedBy,
                    LastCommentByName = cm.CreatedByName,
                    LastCommentNumber = cm.CommentNumber,

                    ShippingInfos = new List<OrderShippingInfoDTO>
                    {
                        new OrderShippingInfoDTO
                        {
                            Id = s.Id,
                            IsActive = s.IsActive,
                            IsVisible = s.IsVisible,
                            ShippingGroupId = s.ShippingGroupId,

                            ShipmentProviderType = s.ShipmentProviderType,
                            LabelPath = s.LabelPath,
                            LabelPurchaseDate = s.LabelPurchaseDate,
                            LabelPurchaseBy = s.LabelPurchaseBy,
                            PrintLabelPackId = s.LabelPrintPackId,
                            LabelPurchaseResult = s.LabelPurchaseResult,
                            LabelPurchaseMessage = s.LabelPurchaseMessage,
                            TrackingNumber = s.TrackingNumber,
                            CustomCarrier = s.CustomCarrier,
                            CustomShippingMethodName = s.CustomShippingMethodName,

                            NumberInBatch = s.NumberInBatch,

                            CancelLabelRequested = s.CancelLabelRequested,
                            LabelCanceled = s.LabelCanceled,

                            DeliveryDays = s.DeliveryDays,
                            DeliveryDaysInfo = s.DeliveryDaysInfo,
                            DeliveredStatus = s.DeliveredStatus,
                            EstimatedDeliveryDate = s.EstimatedDeliveryDate,
                            ActualDeliveryDate = s.ActualDeliveryDate,
                            TrackingStateSource = s.TrackingStateSource,
                            TrackingStateEvent = s.TrackingStateEvent,
                            TrackingStateDate = s.TrackingStateDate,
                            LastTrackingRequestDate = s.LastTrackingRequestDate,

                            ShippingMethod = new ShippingMethodDTO()
                            {
                                Id = sm.Id,
                                ShipmentProviderType = sm.ShipmentProviderType,
                                Name = sm.Name,
                                ShortName = sm.ShortName ?? sm.Name,
                                RequiredPackageSize = sm.RequiredPackageSize,
                                CarrierName = sm.CarrierName,
                                ServiceIdentifier = sm.ServiceIdentifier,
                                StampsServiceEnumCode = sm.StampsServiceEnumCode,
                                StampsPackageEnumCode = sm.StampsPackageEnumCode,
                                AllowOverweight = sm.AllowOverweight,
                                IsInternational = sm.IsInternational,

                                CroppedLayout = sm.CroppedLayout,
                                IsCroppedLabel = sm.IsCroppedLabel,
                                IsFullPagePrint = sm.IsFullPagePrint
                            },

                            StampsShippingCost = s.StampsShippingCost,
                            InsuranceCost = s.InsuranceCost,
                            SignConfirmationCost = s.SignConfirmationCost,
                            UpChargeCost = s.UpChargeCost,

                            OrderAmazonId = o.AmazonIdentifier,
                            ShippingDate = s.ShippingDate,
                            StampsTxId = s.StampsTxId,

                            //NOTE: require for print label. Uses only on OrderShippingInfoRepository, TODO: change ShippingInfo dto
                            //IsInsured = o.IsInsured,
                            //IsSignConfirmation = o.IsSignConfirmation,
                            //OrderId = o.Id,
                        }
                    },

                    Items = new List<ListingOrderDTO> 
                    {
                        new ListingOrderDTO
                        {
                            OrderItemEntityId = oi.Id,
                            Id = l.Id,
                            ASIN = l.ASIN,
                            Market = l.Market,
                            MarketplaceId = l.MarketplaceId,
                            Rank = l.Rank,

                            ItemId = l.ItemId,
                            ParentASIN = l.ParentASIN,

                            SourceMarketId = l.SourceMarketId,
                            SourceMarketUrl = l.SourceMarketUrl,

                            //Price
                            ItemPaid = oi.ItemPaid,
                            ItemGrandPrice = oi.ItemGrandPrice,
                            ItemPrice = oi.ItemPrice,
                            ItemPriceCurrency = oi.ItemPriceCurrency,
                            ItemPriceInUSD = oi.ItemPriceInUSD,
                            ItemTax = oi.ItemTax,                            

                            PromotionDiscount = oi.PromotionDiscount,
                            PromotionDiscountCurrency = oi.PromotionDiscountCurrency,
                            PromotionDiscountInUSD = oi.PromotionDiscountInUSD,

                            ShippingPrice = oi.ShippingPrice,
                            ShippingPriceCurrency = oi.ShippingPriceCurrency,
                            ShippingPriceInUSD = oi.ShippingPriceInUSD,
                            ShippingPaid = oi.ShippingPaid,
                            ShippingTax = oi.ShippingTax,

                            ShippingDiscount = oi.ShippingDiscount,
                            ShippingDiscountCurrency = oi.ShippingDiscountCurrency,
                            ShippingDiscountInUSD = oi.ShippingDiscountInUSD,

                            QuantityOrdered = oi.QuantityOrdered,
                            QuantityShipped = oi.QuantityShipped,

                            ItemOrderId = oi.ItemOrderIdentifier,
                            SourceItemOrderIdentifier = oi.SourceItemOrderIdentifier,
                            RecordNumber = oi.RecordNumber,

                            ReplaceType = oi.ReplaceType,

                            SourceListingId = oi.SourceListingId,

                            SourceStyleString = oi.SourceStyleString,
                            SourceStyleItemId = oi.SourceStyleItemId,
                            SourceStyleSize = oi.SourceStyleSize,
                            SourceStyleColor = oi.SourceStyleColor,

                            StyleID = oi.StyleString,
                            StyleId = oi.StyleId,
                            StyleImage = oi.StyleImage,
                            StyleItemId = oi.StyleItemId,
                            StyleSize = oi.StyleSize,
                            StyleColor = oi.StyleColor,
                            UseStyleImage = l.UseStyleImage,
                            Weight = oi.Weight,                            
                            ItemStyle = oi.ItemStyle,
                            ShippingSize = oi.ShippingSize,
                            PackageLength = oi.PackageLength,
                            PackageWidth = oi.PackageWidth,
                            PackageHeight = oi.PackageHeight,
                            InternationalPackage = oi.InternationalPackage,
                            
                            IsPrime = l.IsPrime,

                            ListingId = l.ListingId,
                            SKU = l.SKU,
                            Size = l.Size,
                            Color = l.Color,
                            Barcode = l.Barcode,

                            Title = l.Title,
                            Picture = l.Picture,
                            ItemPicture = l.ItemPicture,
                        }
                    }
                });
            return query;
        }

        public DTOOrder GetOrderWithItems(IWeightService weightService, 
            string orderId, 
            bool unmaskReferenceStyle, 
            bool includeSourceItems)
        {
            if (String.IsNullOrEmpty(orderId))
                return null;

            var orders = GetFilteredOrdersWithItems(weightService,
                new OrderSearchFilter()
                {
                    Market = MarketType.None,
                    EqualOrderNumber = orderId,
                    IgnoreBatchFilter = true,
                    IncludeNotify = true,
                    UnmaskReferenceStyles = unmaskReferenceStyle,
                    IncludeSourceItems = includeSourceItems,
                });
            return orders.FirstOrDefault();
        }

        public DTOOrder GetOrderWithItems(IWeightService weightService, 
            long orderId, 
            bool withAllShippings, 
            bool includeNotify, 
            bool unmaskReferenceStyles)
        {
            var orders = GetFilteredOrdersWithItems(weightService,
                new OrderSearchFilter()
                {
                    Market = MarketType.None,
                    EqualOrderIds = new [] { orderId },
                    IgnoreBatchFilter = true,
                    IncludeNotify = includeNotify,
                    UnmaskReferenceStyles = unmaskReferenceStyles,
                });

            var order = orders.FirstOrDefault();
            if (order != null && withAllShippings)
            {
                order.ShippingInfos = unitOfWork.OrderShippingInfos.GetOrderInfoWithItems(weightService,
                    new[] { orderId },
                    SortMode.None,
                    false,
                    false,
                    false)
                    .Where(sh => sh.ShipmentProviderType == order.ShipmentProviderType).ToList();
            }
            return order;
        }

        public IEnumerable<DTOOrder> GetSelectedOrdersWithItems(IWeightService weightService, 
            long[] orderIdList,
            bool includeSourceItems)
        {
            return GetFilteredOrdersWithItems(weightService,
                new OrderSearchFilter()
                {
                    Market = MarketType.None,
                    EqualOrderIds = orderIdList,
                    IgnoreBatchFilter = true,
                    UnmaskReferenceStyles = false,
                    IncludeSourceItems = false,
                });
        }

        public IEnumerable<DTOOrder> GetOrdersWithItemsByStatus(IWeightService weightService, 
            string[] statusList, 
            MarketType market, 
            string marketplaceId)
        {
            return GetFilteredOrdersWithItems(weightService,
                new OrderSearchFilter()
                {
                    Market = market,
                    MarketplaceId = marketplaceId,
                    OrderStatus = statusList,
                    IgnoreBatchFilter = true
                });
        }

        public IEnumerable<DTOOrder> GetFilteredOrderInfos(IWeightService weightService, OrderSearchFilter filter)
        {
            var orderQuery = from o in unitOfWork.GetSet<Order>()
                join oi in unitOfWork.GetSet<ViewOrderItem>() on o.Id equals oi.OrderId
                join cm in unitOfWork.GetSet<ViewActualOrderComment>() on o.Id equals cm.OrderId into withComments
                from cm in withComments.DefaultIfEmpty()
                select new DTOOrder()
                {
                    Id = o.Id,
                    Market = o.Market,
                    MarketplaceId = o.MarketplaceId,
                    
                    OrderType = o.OrderType,

                    OrderId = o.AmazonIdentifier,
                    
                    OrderStatus = o.OrderStatus,
                    OrderDate = o.OrderDate,
                    EarliestDeliveryDate = o.EstDeliveryDate,
                    LatestDeliveryDate = o.LatestDeliveryDate,
                    EarliestShipDate = o.EarliestShipDate,
                    LatestShipDate = o.LatestShipDate,
                    
                    PersonName = o.PersonName,

                    ShippingCountry = o.ShippingCountry,
                    
                    InitialServiceType = o.InitialServiceType,
                    
                    ShipmentProviderType = o.ShipmentProviderType,

                    OnHold = o.OnHold,
                    
                    TotalPrice = o.TotalPrice,
                    
                    BatchId = o.BatchId,
                    
                    AddressValidationStatus = o.AddressValidationStatus,
                    IsDismissAddressValidation = o.IsDismissAddressValidation,
                    
                    ManuallyPersonName = o.ManuallyPersonName,
                    
                    IsManuallyUpdated = o.IsManuallyUpdated,

                    LastCommentMessage = cm.Message,
                    LastCommentDate = cm.CreateDate,
                    
                    Items = new List<ListingOrderDTO> 
                    {
                        new ListingOrderDTO
                        {
                            OrderItemEntityId = oi.Id,

                            //Price
                            ItemPrice = oi.ItemPrice,
                            ItemPriceInUSD = oi.ItemPriceInUSD,
                            
                            QuantityOrdered = oi.QuantityOrdered,
                            QuantityShipped = oi.QuantityShipped,

                            ItemOrderId = oi.ItemOrderIdentifier,
                            
                            Weight = oi.Weight
                        }
                    }
                };

            if (filter.BatchId.HasValue)
            {
                orderQuery = orderQuery.Where(o => o.BatchId == filter.BatchId.Value);
            }
            else
            {
                orderQuery = orderQuery.Where(o => !o.BatchId.HasValue);
            }

            if (filter.OrderStatus != null)
            {
                orderQuery = orderQuery.Where(o => filter.OrderStatus.Contains(o.OrderStatus));
            }

            var orderItems = orderQuery.ToList();
            var orders = orderItems.Distinct(new DTOOrderComparer()).ToList();

            var orderIdList = orders.Select(o => o.Id).ToList();

            //Include notifications
            IList<OrderNotifyDto> notifies = new List<OrderNotifyDto>();
            if (filter.IncludeNotify)
            {
                notifies = unitOfWork.OrderNotifies.GetForOrders(orders.Select(o => o.Id).ToList()).ToList();
            }

            IList<OrderShippingInfoDTO> shippingInfoes = new List<OrderShippingInfoDTO>();
            var shippingInfoQuery = from s in unitOfWork.GetSet<OrderShippingInfo>()
                join sm in unitOfWork.GetSet<ShippingMethod>() on s.ShippingMethodId equals sm.Id
                select new OrderShippingInfoDTO
                {
                    Id = s.Id,
                    OrderId = s.OrderId,
                    IsActive = s.IsActive,
                    IsVisible = s.IsVisible,
                    ShippingGroupId = s.ShippingGroupId,

                    StampsShippingCost = s.StampsShippingCost,

                    LabelPurchaseResult = s.LabelPurchaseResult,
                    LabelPurchaseMessage = s.LabelPurchaseMessage,
                    
                    ShippingMethod = new ShippingMethodDTO()
                    {
                        Id = sm.Id,
                        ShipmentProviderType = sm.ShipmentProviderType,
                        CarrierName = sm.CarrierName,
                        Name = sm.Name,
                        ShortName = sm.ShortName ?? sm.Name,
                        RequiredPackageSize = sm.RequiredPackageSize
                    },
                };
            shippingInfoes = shippingInfoQuery.Where(sh => orderIdList.Contains(sh.OrderId)).ToList();

            IList<OrderShippingInfoDTO> mailInfoes = new List<OrderShippingInfoDTO>();
            if (filter.IncludeMailInfos)
            {
                mailInfoes = unitOfWork.MailLabelInfos.GetByOrderIdsAsDto(orders.Select(o => o.Id).ToList()).ToList();
            }

            foreach (var order in orders)
            {
                var items = orderItems.Where(o => o.Id == order.Id)
                    .Select(i => i.Items.First()).ToList();

                order.Items = items;
                order.ShippingInfos = shippingInfoes.Where(sh => sh.OrderId == order.Id).ToList();
                order.MailInfos = mailInfoes.Where(sh => sh.OrderId == order.Id).ToList();

                var weight = items.All(i => i.Weight > 0) ? 
                    items.Sum(i => i.Weight * i.QuantityOrdered) : 0;
                if (weight > 0)
                {
                    weight = weightService == null ? weight.Value : weightService.AdjustWeight(weight.Value, items.Sum(i => i.QuantityOrdered));
                }

                order.WeightD = weight ?? 0;


                if (filter.IncludeNotify)
                    order.Notifies = notifies.Where(n => n.OrderId == order.Id).ToList();
                else
                    order.Notifies = new List<OrderNotifyDto>();

            }

            return orders;
        }

        public IEnumerable<DTOOrder> GetFilteredOrdersWithItems(IWeightService weightService, OrderSearchFilter filter)
        {
            var query = GetBaseQuery(filter.UnmaskReferenceStyles);

            query = ApplyOrderFilters(query, filter);

            var queryList = query.ToList();
            var orders = queryList.Distinct(new DTOOrderComparer()).ToList();
            var ordersByShippings = queryList.Distinct(new DTOShippingInfoComparer()).ToList();
            var ordersByItems = queryList.Distinct(new DTOOrderItemComparer()).ToList();

            ////Retrieve style list
            var styleIdList = new List<long>();
            foreach (var orderItem in ordersByItems)
            {
                styleIdList.AddRange(orderItem.ItemStyleIdList);
            }

            var locations = unitOfWork.StyleLocations.GetByStyleIdsAsDto(styleIdList.Distinct().ToList());
            foreach (var item in ordersByItems)
            {
                item.Items.First().Locations = locations.Where(l => l.StyleId == item.Items.First().StyleId)
                        .OrderByDescending(l => l.IsDefault).ToList();
            }
            
            //Include notifications
            IList<OrderNotifyDto> notifies = new List<OrderNotifyDto>();
            if (filter.IncludeNotify)
            {
                notifies = unitOfWork.OrderNotifies.GetForOrders(orders.Select(o => o.Id).ToList()).ToList();
            }

            //Include mail infoes
            IList<OrderShippingInfoDTO> mailInfoes = new List<OrderShippingInfoDTO>();
            if (filter.IncludeMailInfos)
            {
                mailInfoes = unitOfWork.MailLabelInfos.GetByOrderIdsAsDto(orders.Select(o => o.Id).ToList()).ToList();
            }

            IList<ListingOrderDTO> sourceOrderItems = new List<ListingOrderDTO>();
            if (filter.IncludeSourceItems)
            {
                sourceOrderItems = unitOfWork.OrderItemSources.GetByOrderIdsAsDto(orders.Select(o => o.Id).ToList()).ToList();
            }

            foreach (var order in orders)
            {
                //SHIPPINGS 
                var orderShippings = ordersByShippings.Where(o => o.Id == order.Id)
                    .Select(i => i.ShippingInfos.First()).ToList();
                
                order.ShippingInfos = orderShippings;

                var orderItems = ordersByItems.Where(o => o.Id == order.Id)
                    .Select(i => i.Items.First()).ToList();

                order.Items = orderItems;

                if (orderShippings.Count > 1 && orderShippings.Any(s => s.IsActive))
                {
                    order.ShippingService = orderShippings.First(s => s.IsActive).ShippingMethod.Name; 
                }

                var weight = orderItems.All(i => i.Weight > 0) ? orderItems.Sum(i => i.Weight * i.QuantityOrdered) : 0;
                if (weight > 0)
                {
                    weight = weightService == null ? weight.Value : weightService.AdjustWeight(weight.Value, orderItems.Sum(i => i.QuantityOrdered));
                }

                order.WeightD = weight ?? 0;

                //Sorting Items
                order.Items = OrderItemsByLocation(orderItems);

                //order.ItemsPrice = itemsPrice;
                //order.ShippingPrice = shippingPrice;
                //order.ShippingDiscount = discountPrice;

                if (filter.IncludeSourceItems)
                    order.SourceItems = sourceOrderItems.Where(i => i.OrderId == order.Id).ToList();
                else
                    order.SourceItems = new List<ListingOrderDTO>();

                if (filter.IncludeNotify)
                    order.Notifies = notifies.Where(n => n.OrderId == order.Id).ToList();
                else
                    order.Notifies = new List<OrderNotifyDto>();

                if (filter.IncludeMailInfos)
                    order.MailInfos = mailInfoes.Where(n => n.OrderId == order.Id).ToList();
                else
                    order.MailInfos = new List<OrderShippingInfoDTO>();
            }

            //POST Processing filters
            if (filter.ExcludeWithLabels)
            {
                orders = orders.Where(o => !o.MailInfos.Any() 
                    && o.ShippingInfos.All(sh => String.IsNullOrEmpty(sh.LabelPath))).ToList();
            }

            if (filter.Status == "Unshipped")
            {
                orders = orders.Where(q => q.OrderStatus == OrderStatusEnumEx.Unshipped
                    || q.OrderStatus == OrderStatusEnumEx.PartiallyShipped).ToList();

            }
            if (filter.Status == "AllPriority")
            {
                orders = orders.Where(o => o.ShippingInfos.All(i => !i.IsActive) || o.ShippingInfos.Any(i => i.IsActive && ShippingUtils.IsMethodNonStandard(i.ShippingMethod.Id))).ToList();
            }
            if (filter.Status == "AllStandard")
            {
                orders = orders.Where(o => o.ShippingInfos.All(i => !i.IsActive) || o.ShippingInfos.Any(i => i.IsActive && !ShippingUtils.IsMethodNonStandard(i.ShippingMethod.Id))).ToList();
            }
            if (filter.Status == "NoWeight")
            {
                orders = orders.Where(o => (o.Items.Any(i => !(i.Weight > 0)) || o.ShippingInfos.All(s => !s.IsActive))).ToList();
            }
            if (filter.Status == "Duplicate")
            {
                orders = orders.Where(o => o.Notifies.Any(n => n.Type == (int) OrderNotifyType.Duplicate)).ToList();
            }
            if (filter.Status == "WoPackage")
            {
                orders = orders.Where(o => !o.ShippingInfos.Any(sh => sh.IsActive)).ToList();
            }
            if (filter.Status == "WoStampsPrice")
            {
                orders = orders.Where(o => o.ShippingInfos.All(sh => !sh.IsActive) 
                    || o.ShippingInfos.All(sh => !sh.StampsShippingCost.HasValue || sh.StampsShippingCost == 0)).ToList();
            }
            if (filter.Status == "UpgradeCandidates")
            {
                var excludeStateList = unitOfWork.States.GetAll().Where(s => !s.IsBase).Select(s => s.StateCode).ToList();

                orders = orders.Where(q => q.ShippingInfos.Any(i => i.IsActive && ShippingUtils.IsMethodStandard(i.ShippingMethod.Id))
                    && q.ShippingCountry == "US"
                    && !excludeStateList.Contains(q.ShippingState)
                    && q.Items.Sum(i => i.ItemPrice) > 10).ToList(); //NOTE: always USD, only local shippings
            }

            return orders;
        }

        private IQueryable<DTOOrder> ApplyOrderFilters(IQueryable<DTOOrder> query,
            OrderSearchFilter filter)
        {
            var today = DateHelper.GetAppNowTime().Date;
            var todayNoon = DateHelper.GetAppNowTime().Date.AddHours(12);

            if (!String.IsNullOrEmpty(filter.EqualOrderNumber))
            {
                filter.OrderNumber = filter.EqualOrderNumber.ToLower();
                query = query.Where(q => q.OrderId.ToLower() == filter.OrderNumber || q.CustomerOrderId == filter.OrderNumber);

                return query;
            }

            if (filter.EqualOrderIds != null && filter.EqualOrderIds.Length > 0)
            {
                query = query.Where(q => filter.EqualOrderIds.Contains(q.Id));

                return query;
            }

            //Apply batch filter
            if (!filter.IgnoreBatchFilter)
            {
                if (filter.BatchId.HasValue)
                {
                    query = query.Where(o => o.BatchId == filter.BatchId.Value);
                }
            }

            if (!String.IsNullOrEmpty(filter.StyleId))
            {
                //query = query.Where(o => o.Items.Any(i => i.StyleID == filter.StyleId));

                //NOTE: w/o join show only one item in order with requested styleId
                query = from q in query
                        join oId in GetOrderIdsByStyleQuery(filter.StyleId) on q.Id equals oId
                        select q;

                return query;
            }

            if (!String.IsNullOrEmpty(filter.OrderNumber))
            {
                //orderNumber = orderNumber;
                var digitsOnlyNumber = new string(filter.OrderNumber.Where(char.IsDigit).ToArray());
                query = query.Where(
                        q => !String.IsNullOrEmpty(q.OrderId) && (q.OrderId.Contains(filter.OrderNumber)) || q.OrderId.Contains(digitsOnlyNumber));

                return query;
            }

            if (!String.IsNullOrEmpty(filter.BuyerName))
            {
                filter.BuyerName = filter.BuyerName.ToLower();
                query = query.Where(q => (!String.IsNullOrEmpty(q.PersonName) &&
                                            q.PersonName.ToLower().Contains(filter.BuyerName))
                                            || (!String.IsNullOrEmpty(q.BuyerName) &&
                                            q.BuyerName.ToLower().Contains(filter.BuyerName)));

                return query;
            }


            //Default no batch
            if (!filter.IgnoreBatchFilter)
            {
                if (!filter.BatchId.HasValue)
                {
                    query = query.Where(q => q.BatchId == null);
                }
            }

            if (filter.DropShipperId.HasValue)
            {
                query = query.Where(q => q.DropShipperId == filter.DropShipperId);
            }

            if (filter.From.HasValue)
            {
                query = query.Where(q => q.OrderDate.HasValue && q.OrderDate >= filter.From);
            }
            if (filter.To.HasValue)
            {
                query = query.Where(q => q.OrderDate.HasValue && q.OrderDate <= filter.To);
            }

            if (filter.OrderStatus != null && filter.OrderStatus.Any())
            {
                query = query.Where(q => filter.OrderStatus.Contains(q.OrderStatus));
            }

            if (filter.Market != MarketType.None)
            {
                query = query.Where(q => q.Market == (int)filter.Market);
            }

            if (!String.IsNullOrEmpty(filter.MarketplaceId))
            {
                query = query.Where(q => q.MarketplaceId == filter.MarketplaceId);
            }

            if (!String.IsNullOrEmpty(filter.FulfillmentChannel))
            {
                query = query.Where(q => q.FulfillmentChannel == filter.FulfillmentChannel);
            }

            if (filter.ExcludeOnHold)
            {
                query = query.Where(q => !q.OnHold);
            }

            if (filter.Status == "Unshipped")
            {
                query = query.Where(q => q.OrderStatus == "Unshipped");
            }
            if (filter.Status == "NotOnHold")
            {
                query = query.Where(q => !q.OnHold);
            }
            if (filter.Status == "OverdueShipDate")
            {
                query = query.Where(q => q.LatestShipDate.HasValue
                    && q.LatestShipDate < todayNoon);
            }
            if (filter.Status == "SameDay")
            {
                query = query.Where(q => q.InitialServiceType.ToLower() == "sameday");
            }
            if (filter.Status == "OnHold")
            {
                query = query.Where(q => q.OnHold);
            }
            if (filter.Status == "ExpeditedNotOnHold")
            {
                query = query.Where(q => !q.OnHold
                    && q.InitialServiceType.ToLower() == "expedited");
            }
            if (filter.Status == "PrioritiesNotHold")
            {
                query = query.Where(q => !q.OnHold
                    && !q.InitialServiceType.Contains("Standard"));
            }
            if (filter.Status == "NoAddressIssues")
            {
                query = query.Where(q => q.AddressValidationStatus < (int)AddressValidationStatus.Invalid
                                            || q.IsDismissAddressValidation);
            }
            if (filter.Status == "WithAddressIssues")
            {
                query = query.Where(q => q.AddressValidationStatus >= (int)AddressValidationStatus.Invalid
                                            && !q.IsDismissAddressValidation);
            }
            
            return query;
        }


        private IQueryable<OrderShippingInfoDTO> GetDisplayShippingQuery()
        {
            var shippingInfoQuery = from s in unitOfWork.GetSet<OrderShippingInfo>()
                join sm in unitOfWork.GetSet<ShippingMethod>() on s.ShippingMethodId equals sm.Id
                select new OrderShippingInfoDTO
                        {
                            Id = s.Id,
                            OrderId = s.OrderId,

                            IsActive = s.IsActive,
                            IsVisible = s.IsVisible,
                            ShippingGroupId = s.ShippingGroupId,
                            ShipmentProviderType = s.ShipmentProviderType,                            
                            LabelPath = s.LabelPath,
                            LabelPurchaseDate = s.LabelPurchaseDate,
                            PrintLabelPackId = s.LabelPrintPackId,
                            LabelPurchaseResult = s.LabelPurchaseResult,
                            LabelPurchaseMessage = s.LabelPurchaseMessage,
                            TrackingNumber = s.TrackingNumber,
                            CustomCarrier = s.CustomCarrier,
                            CustomShippingMethodName = s.CustomShippingMethodName,                            
                            NumberInBatch = s.NumberInBatch,

                            CancelLabelRequested = s.CancelLabelRequested,
                            LabelCanceled = s.LabelCanceled,

                            EstimatedDeliveryDate = s.EstimatedDeliveryDate,
                            DeliveryDays = s.DeliveryDays,
                            DeliveryDaysInfo = s.DeliveryDaysInfo,
                            DeliveredStatus = s.DeliveredStatus,
                            ActualDeliveryDate = s.ActualDeliveryDate,
                            TrackingStateSource = s.TrackingStateSource,
                            TrackingStateEvent = s.TrackingStateEvent,
                            TrackingStateDate = s.TrackingStateDate,

                            ShippingMethod = new ShippingMethodDTO()
                            {
                                Id = sm.Id,
                                ShipmentProviderType = sm.ShipmentProviderType,
                                Name = sm.Name,
                                ShortName = sm.ShortName ?? sm.Name,
                                RequiredPackageSize = sm.RequiredPackageSize,
                                CarrierName = sm.CarrierName,
                                ServiceIdentifier = sm.ServiceIdentifier,
                                StampsServiceEnumCode = sm.StampsServiceEnumCode,
                                StampsPackageEnumCode = sm.StampsPackageEnumCode,
                                AllowOverweight = sm.AllowOverweight,
                                IsInternational = sm.IsInternational,

                                CroppedLayout = sm.CroppedLayout,
                                IsCroppedLabel = sm.IsCroppedLabel,
                                IsFullPagePrint = sm.IsFullPagePrint
                            },

                            StampsShippingCost = s.StampsShippingCost,
                            InsuranceCost = s.InsuranceCost,
                            SignConfirmationCost = s.SignConfirmationCost,
                            UpChargeCost = s.UpChargeCost,

                            ShippingDate = s.ShippingDate,
                            StampsTxId = s.StampsTxId,
                        };

            return shippingInfoQuery;
        }


        private IQueryable<DTOOrder> GetDisplayOrderQuery(bool unmaskReferenceStyle)
        {
            var listingsQuery = unitOfWork.Listings.GetViewListingsAsDto(unmaskReferenceStyle);

            var query = (
                from o in unitOfWork.GetSet<Order>()
                join oi in unitOfWork.GetSet<ViewOrderItem>() on o.Id equals oi.OrderId
                join l in listingsQuery on oi.ListingId equals l.Id
                join b in unitOfWork.GetSet<OrderBatch>() on o.BatchId equals b.Id into withB
                from b in withB.DefaultIfEmpty()
                join ds in unitOfWork.GetSet<DropShipper>() on o.DropShipperId equals ds.Id into withDS
                from ds in withDS.DefaultIfEmpty()
                join cm in unitOfWork.GetSet<ViewActualOrderComment>() on o.Id equals cm.OrderId into withComments
                from cm in withComments.DefaultIfEmpty()

                select new DTOOrder
                {
                    Id = o.Id,
                    Market = o.Market,
                    MarketplaceId = o.MarketplaceId,
                    FulfillmentChannel = o.FulfillmentChannel,
                    OrderType = o.OrderType,

                    OrderId = o.AmazonIdentifier,
                    MarketOrderId = o.MarketOrderId,
                    SalesRecordNumber = o.SalesRecordNumber,
                    CustomerOrderId = o.CustomerOrderId,

                    AutoDSSelection = o.AutoDSSelection,
                    DropShipperId = o.DropShipperId,
                    DropShipperName = ds != null ? ds.Name : "",
                    SentToDropShipper = o.SentToDropShipper,

                    SubOrderNumber = o.SubOrderNumber,
                    SubOrderAmountPercent = o.SubOrderAmountPercent,

                    OrderStatus = o.OrderStatus,
                    OrderDate = o.OrderDate,
                    EarliestDeliveryDate = o.EstDeliveryDate,
                    LatestDeliveryDate = o.LatestDeliveryDate,
                    EarliestShipDate = o.EarliestShipDate,
                    LatestShipDate = o.LatestShipDate,
                    Quantity = o.Quantity,
                    BuyerName = o.BuyerName,
                    BuyerEmail = o.BuyerEmail,
                    PersonName = o.PersonName,

                    ShippingCountry = o.ShippingCountry,
                    //NOTE: gets from isActive shipping
                    //ShippingService = sm.Name,
                    InitialServiceType = o.InitialServiceType,
                    SourceShippingService = o.SourceShippingService,
                    UpgradeLevel = o.UpgradeLevel,
                    ShipmentProviderType = o.ShipmentProviderType,
                    ShippingCalculationStatus = o.ShippingCalculationStatus,

                    OnHold = o.OnHold,
                    OnHoldUpdateDate = o.OnHoldUpdateDate,
                    TotalPrice = o.TotalPrice,
                    TotalPriceCurrency = o.TotalPriceCurrency,
                    TotalPaid = o.TotalPaid,
                    TaxAmount = o.TaxAmount,
                    ShippingPaid = o.ShippingPaid,
                    ShippingPrice = o.ShippingPrice,
                    ShippingTaxAmount = o.ShippingTaxAmount,
                    DiscountAmount = o.DiscountAmount,
                    DiscountTax = o.DiscountTax,
                    ShippingDiscountAmount = o.ShippingDiscountAmount,

                    SignifydDesc = o.SignifydDesc,
                    SignifydStatus = o.SignifydStatus,

                    BatchId = o.BatchId,
                    BatchName = b != null ? b.Name : "",
                    IsForceVisible = o.IsForceVisible,
                    IsRefundLocked = o.IsRefundLocked,

                    ShippingAddress1 = o.ShippingAddress1,
                    ShippingAddress2 = o.ShippingAddress2,
                    ShippingCity = o.ShippingCity,
                    ShippingState = o.ShippingState,
                    ShippingZip = o.ShippingZip,
                    ShippingZipAddon = o.ShippingZipAddon,
                    ShippingPhone = o.ShippingPhone,
                    ShippingPhoneNumeric = o.ShippingPhoneNumeric,

                    AddressValidationStatus = o.AddressValidationStatus,
                    IsDismissAddressValidation = o.IsDismissAddressValidation,
                    DismissAddressValidationDate = o.DismissAddressValidationDate,
                    AddressVerifyRequestDate = o.AddressVerifyRequestDate,

                    AttachedToOrderId = o.AttachedToOrderId,
                    AttachedToOrderString = o.AttachedToOrderString,

                    ManuallyPersonName = o.ManuallyPersonName,
                    ManuallyShippingAddress1 = o.ManuallyShippingAddress1,
                    ManuallyShippingAddress2 = o.ManuallyShippingAddress2,
                    ManuallyShippingCity = o.ManuallyShippingCity,
                    ManuallyShippingState = o.ManuallyShippingState,
                    ManuallyShippingCountry = o.ManuallyShippingCountry,
                    ManuallyShippingZip = o.ManuallyShippingZip,
                    ManuallyShippingZipAddon = o.ManuallyShippingZipAddon,
                    ManuallyShippingPhone = o.ManuallyShippingPhone,
                    ManuallyShippingPhoneNumeric = o.ManuallyShippingPhoneNumeric,

                    IsManuallyUpdated = o.IsManuallyUpdated,

                    InsuredValue = o.InsuredValue,
                    IsInsured = o.IsInsured,

                    IsSignConfirmation = o.IsSignConfirmation,

                    LastCommentMessage = cm.Message,
                    LastCommentDate = cm.CreateDate,
                    LastCommentBy = cm.CreatedBy,
                    LastCommentByName = cm.CreatedByName,
                    LastCommentNumber = cm.CommentNumber,
                    
                    Items = new List<ListingOrderDTO> 
                    {
                        new ListingOrderDTO
                        {
                            OrderItemEntityId = oi.Id,
                            Id = l.Id,
                            ASIN = l.ASIN,
                            Market = l.Market,
                            MarketplaceId = l.MarketplaceId,
                            Rank = l.Rank,

                            ItemId = l.ItemId,
                            ParentASIN = l.ParentASIN,
                            ListingCreateDate = l.CreateDate,

                            SourceMarketId = l.SourceMarketId,
                            SourceMarketUrl = l.SourceMarketUrl,

                            ItemGrandPrice = oi.ItemGrandPrice,
                            ItemPaid = oi.ItemPaid,
                            ItemTax = oi.ItemTax,
                            ItemPrice = oi.ItemPrice,
                            ItemPriceCurrency = oi.ItemPriceCurrency,
                            ItemPriceInUSD = oi.ItemPriceInUSD,
                            
                            RefundItemPrice = oi.RefundItemPrice,
                            RefundItemPriceInUSD = oi.RefundItemPriceInUSD,

                            PromotionDiscount = oi.PromotionDiscount,
                            PromotionDiscountCurrency = oi.PromotionDiscountCurrency,
                            PromotionDiscountInUSD = oi.PromotionDiscountInUSD,

                            ShippingPrice = oi.ShippingPrice,
                            ShippingPriceCurrency = oi.ShippingPriceCurrency,
                            ShippingPriceInUSD = oi.ShippingPriceInUSD,

                            RefundShippingPrice = oi.RefundShippingPrice,
                            RefundShippingPriceInUSD = oi.RefundShippingPriceInUSD,

                            ShippingDiscount = oi.ShippingDiscount,
                            ShippingDiscountCurrency = oi.ShippingDiscountCurrency,
                            ShippingDiscountInUSD = oi.ShippingDiscountInUSD,

                            QuantityOrdered = oi.QuantityOrdered,
                            QuantityShipped = oi.QuantityShipped,

                            ItemOrderId = oi.ItemOrderIdentifier,
                            RecordNumber = oi.RecordNumber,

                            SourceListingId = oi.SourceListingId,
                            SourceStyleString = oi.SourceStyleString,
                            SourceStyleItemId = oi.SourceStyleItemId,
                            SourceStyleSize = oi.SourceStyleSize,
                            SourceStyleColor = oi.SourceStyleColor,

                            StyleID = oi.StyleString,
                            StyleId = oi.StyleId,
                            StyleImage = oi.StyleImage,
                            StyleItemId = oi.StyleItemId,
                            StyleSize = oi.StyleSize,
                            StyleColor = oi.StyleColor,
                            UseStyleImage = l.UseStyleImage,
                            ReplaceType = oi.ReplaceType,

                            Weight = oi.Weight,
                            ShippingSize = oi.ShippingSize,
                            PackageLength = oi.PackageLength,
                            PackageWidth = oi.PackageWidth,
                            PackageHeight = oi.PackageHeight,
                            ExcessiveShipmentThreshold = oi.ExcessiveShipment,

                            ListingId = l.ListingId,
                            SKU = l.SKU,
                            Size = l.Size,
                            Color = l.Color,

                            Title = l.Title,
                            Picture = l.Picture,
                            ItemPicture = l.ItemPicture,
                        }
                    }
                });
            return query;
        }

        public GridResponse<DTOOrder> GetDisplayOrdersWithItems(IWeightService weightService, OrderSearchFilter filter)
        {
            return GetDisplayOrdersWithItems(null, weightService, filter);
        }

        private void WriteToLog(ILogService log, string message)
        {
            if (log != null)
                log.Info(message);
        }

        public GridResponse<DTOOrder> GetDisplayOrdersWithItems(ILogService log, IWeightService weightService, OrderSearchFilter filter)
        {
            var orderQuery = GetDisplayOrderQuery(filter.UnmaskReferenceStyles);
            var totalCount = 0;

            orderQuery = ApplyDisplayOrderFilters(orderQuery, filter);
                        
            if (filter.StartIndex.HasValue && filter.LimitCount.HasValue && filter.LimitCount > 0)
            {
                totalCount = orderQuery.Select(o => o.Id).Distinct().Count();

                if (!String.IsNullOrEmpty(filter.SortField))
                {
                    switch (filter.SortField)
                    {
                        case "OrderDate":
                            if (filter.SortMode == 0)
                                orderQuery = orderQuery.OrderBy(o => o.OrderDate);
                            else
                                orderQuery = orderQuery.OrderByDescending(o => o.OrderDate);
                            break;
                        case "AlignedExpectedShipDate":
                            if (filter.SortMode == 0)
                                orderQuery = orderQuery.OrderBy(o => o.LatestShipDate);
                            else
                                orderQuery = orderQuery.OrderByDescending(o => o.LatestShipDate);
                            break;
                        case "StampsShippingCost":
                            if (filter.SortMode == 0)
                                orderQuery = orderQuery.OrderBy(o => o.ShippingPaid);
                            else
                                orderQuery = orderQuery.OrderByDescending(o => o.ShippingPaid);
                            break;
                        case "Weight":
                            if (filter.SortMode == 0)
                                orderQuery = orderQuery.OrderBy(o => o.Items.FirstOrDefault().Weight);
                            else
                                orderQuery = orderQuery.OrderByDescending(o => o.Items.FirstOrDefault().Weight);
                            break;
                        case "ItemPrice":
                            if (filter.SortMode == 0)
                                orderQuery = orderQuery.OrderBy(o => o.TotalPrice);
                            else
                                orderQuery = orderQuery.OrderByDescending(o => o.TotalPrice);
                            break;
                        case "ShippingCountry":
                            if (filter.SortMode == 0)
                                orderQuery = orderQuery.OrderBy(o => o.ShippingCountry);
                            else
                                orderQuery = orderQuery.OrderByDescending(o => o.ShippingCountry);
                            break;
                        case "ShippingMethodId":
                            if (filter.SortMode == 0)
                                orderQuery = orderQuery.OrderBy(o => o.InitialServiceType);
                            else
                                orderQuery = orderQuery.OrderByDescending(o => o.InitialServiceType);
                            break;
                        case "Quantity":
                            if (filter.SortMode == 0)
                                orderQuery = orderQuery.OrderBy(o => o.Quantity);
                            else
                                orderQuery = orderQuery.OrderByDescending(o => o.Quantity);
                            break;
                        case "PersonName":
                            if (filter.SortMode == 0)
                                orderQuery = orderQuery.OrderBy(o => o.PersonName);
                            else
                                orderQuery = orderQuery.OrderByDescending(o => o.PersonName);
                            break;
                        default:
                            orderQuery = orderQuery.OrderByDescending(o => o.OrderDate);
                            break;
                    }
                }
                else
                {
                    orderQuery = orderQuery.OrderByDescending(o => o.OrderDate);
                }

                orderQuery = orderQuery.Skip(filter.StartIndex.Value);
                orderQuery = orderQuery.Take(filter.LimitCount.Value);
            }

            WriteToLog(log, "Before orderQuery.ToList");
            var orderList = orderQuery.ToList();
            WriteToLog(log, "End orderQuery.ToList");
            var orders = orderList.Distinct(new DTOOrderComparer()).ToList();
            var ordersByItems = orderList.Distinct(new DTOOrderItemComparer()).ToList();

            //Retrieve style list
            var styleIdList = new List<long>();
            foreach (var order in orders)
            {
                styleIdList.AddRange(order.ItemStyleIdList);
            }

            var orderIdList = orderList.Select(o => o.Id).ToList();

            WriteToLog(log, "Before GetDisplayShippingQuery");
            var shippingInfoQuery = GetDisplayShippingQuery();
            shippingInfoQuery = shippingInfoQuery.Where(sh => (sh.IsVisible || filter.IncludeAllShippings) && orderIdList.Contains(sh.OrderId));
            var shippingInfoList = shippingInfoQuery.ToList();
            WriteToLog(log, "End GetDisplayShippingQuery");

            var locations = unitOfWork.StyleLocations.GetByStyleIdsAsDto(styleIdList.Distinct().ToList());
            foreach (var item in ordersByItems)
            {
                item.Items.First().Locations = locations.Where(l => l.StyleId == item.Items.First().StyleId)
                        .OrderByDescending(l => l.IsDefault).ToList();
            }

            //Include notifications
            IList<OrderNotifyDto> notifies = new List<OrderNotifyDto>();
            if (filter.IncludeNotify)
            {
                WriteToLog(log, "Before IncludeNotify");
                notifies = unitOfWork.OrderNotifies.GetForOrders(orders.Select(o => o.Id).ToList()).ToList();
                WriteToLog(log, "End IncludeNotify");
            }

            //Include mail infoes
            IList<OrderShippingInfoDTO> mailInfoes = new List<OrderShippingInfoDTO>();
            if (filter.IncludeMailInfos)
            {
                WriteToLog(log, "Before IncludeMailInfos");
                mailInfoes = unitOfWork.MailLabelInfos.GetByOrderIdsAsDto(orders.Select(o => o.Id).ToList()).ToList();
                WriteToLog(log, "End IncludeMailInfos");
            }

            foreach (var order in orders)
            {
                //SHIPPINGS 
                var orderShippings = shippingInfoList.Where(sh => sh.OrderId == order.Id).ToList();
                order.ShippingInfos = orderShippings;

                var orderItems = ordersByItems.Where(o => o.Id == order.Id)
                    .Select(i => i.Items.First()).ToList();
                order.Items = orderItems;

                if (orderShippings.Count > 1 && orderShippings.Any(s => s.IsActive))
                {
                    order.ShippingService = orderShippings.First(s => s.IsActive).ShippingMethod.Name;
                }

                var weight = orderItems.All(i => i.Weight > 0) ? orderItems.Sum(i => i.Weight * i.QuantityOrdered) : 0;
                if (weight > 0)
                {
                    weight = weightService == null ? weight : weightService.AdjustWeight(weight.Value, orderItems.Sum(i => i.QuantityOrdered));
                }

                order.WeightD = weight ?? 0;

                //Sorting Items
                order.Items = OrderItemsByLocation(orderItems);


                if (filter.IncludeNotify)
                    order.Notifies = notifies.Where(n => n.OrderId == order.Id).ToList();
                else
                    order.Notifies = new List<OrderNotifyDto>();

                if (filter.IncludeMailInfos)
                    order.MailInfos = mailInfoes.Where(n => n.OrderId == order.Id).ToList();
                else
                    order.MailInfos = new List<OrderShippingInfoDTO>();
            }

            //POST Processing filters
            if (filter.ExcludeWithLabels && !filter.HasGlobalSearchParams())
            {
                orders = orders.Where(o => (!o.MailInfos.Any()
                    && o.ShippingInfos.All(sh => String.IsNullOrEmpty(sh.LabelPath)))
                    || (filter.IncludeForceVisible && o.IsForceVisible == true)).ToList();
            }
            
            return new GridResponse<DTOOrder>(orders, totalCount);
        }

        private IQueryable<DTOOrder> ApplyDisplayOrderFilters(IQueryable<DTOOrder> query,
            OrderSearchFilter filter)
        {
            int? takeLimit = null;

            if (!String.IsNullOrEmpty(filter.EqualOrderNumber))
            {
                filter.OrderNumber = filter.EqualOrderNumber.ToLower();
                query = query.Where(q => q.OrderId.ToLower() == filter.OrderNumber 
                    || q.MarketOrderId == filter.OrderNumber
                    || q.CustomerOrderId == filter.OrderNumber);

                return query;
            }

            if (filter.EqualOrderIds != null && filter.EqualOrderIds.Length > 0)
            {
                query = query.Where(q => filter.EqualOrderIds.Contains(q.Id));

                return query;
            }

            //Apply batch filter
            if (!filter.IgnoreBatchFilter)
            {
                if (filter.BatchId.HasValue)
                {
                    query = query.Where(o => o.BatchId == filter.BatchId.Value);
                }
            }

            if (filter.StyleItemId.HasValue)
            {
                //NOTE: w/o join show only one item in order with requested styleId
                query = from q in query
                        join oId in GetOrderIdsByStyleItemIdQuery(filter.StyleItemId.Value) on q.Id equals oId
                        select q;
            }

            if (!String.IsNullOrEmpty(filter.StyleId))
            {
                //query = query.Where(o => o.Items.Any(i => i.StyleID == filter.StyleId));

                //NOTE: w/o join show only one item in order with requested styleId
                query = from q in query
                        join oId in GetOrderIdsByStyleQuery(filter.StyleId) on q.Id equals oId
                        select q;
            }

            if (filter.Status == "Unshipped")
            {
                query = query.Where(q => q.OrderStatus == OrderStatusEnumEx.Unshipped
                    || q.OrderStatus == OrderStatusEnumEx.PartiallyShipped);
            }

            if (!String.IsNullOrEmpty(filter.BuyerName))
            {
                filter.BuyerName = filter.BuyerName.Trim().ToLower();
                var phoneNumber = StringHelper.GetAllDigitSequences(filter.BuyerName);

                var orderIdsWithTrackingNumber = (from sh in unitOfWork.GetSet<OrderShippingInfo>()
                                                 where !String.IsNullOrEmpty(sh.TrackingNumber)
                                                    && sh.TrackingNumber == filter.BuyerName
                                                 select sh.OrderId).ToList();

                Expression<Func<DTOOrder, bool>> buyerExpression = (DTOOrder q) =>
                    //Check person name
                    ((!String.IsNullOrEmpty(q.PersonName) &&
                        q.PersonName.ToLower().Contains(filter.BuyerName))
                        || (!String.IsNullOrEmpty(q.BuyerName) &&
                            q.BuyerName.ToLower().Contains(filter.BuyerName)))
                            //Check phone number
                            || (!String.IsNullOrEmpty(phoneNumber)
                                && ((!String.IsNullOrEmpty(q.ShippingPhoneNumeric)
                                              && q.ShippingPhoneNumeric.Contains(phoneNumber))
                                             || (!String.IsNullOrEmpty(q.ManuallyShippingPhoneNumeric)
                                                 && q.ManuallyShippingPhoneNumeric.Contains(phoneNumber))))
                                || orderIdsWithTrackingNumber.Contains(q.Id);
                query = query.Where(buyerExpression).OrderByDescending(o => o.OrderDate);
                takeLimit = 100;

                //query = query.OrderByDescending(o => o.OrderDate).Take(100);
            }



            //if (!String.IsNullOrEmpty(filter.BuyerName)
            //    || !String.IsNullOrEmpty(filter.StyleId)
            //    || filter.StyleItemId.HasValue)
            //    return query;

            if (filter.Market != MarketType.None)
            {
                query = query.Where(q => q.Market == (int)filter.Market);
            }

            if (!String.IsNullOrEmpty(filter.MarketplaceId))
            {
                query = query.Where(q => q.MarketplaceId == filter.MarketplaceId);
            }

            if (!String.IsNullOrEmpty(filter.OrderNumber))
            {
                //orderNumber = orderNumber;
                var digitsOnlyNumber = new string(filter.OrderNumber.Where(char.IsDigit).ToArray());
                if (digitsOnlyNumber.Length < filter.OrderNumber.Length / 2)
                {
                    query = query.Where(
                            q => (!String.IsNullOrEmpty(q.OrderId) && (q.OrderId.Contains(filter.OrderNumber)))
                            || (!String.IsNullOrEmpty(q.CustomerOrderId) && (q.CustomerOrderId.Contains(filter.OrderNumber))));
                }
                else
                {
                    query = query.Where(
                            q => (!String.IsNullOrEmpty(q.OrderId) && (q.OrderId.Contains(filter.OrderNumber) || q.OrderId.Contains(digitsOnlyNumber)))
                            || (!String.IsNullOrEmpty(q.CustomerOrderId) && (q.CustomerOrderId.Contains(filter.OrderNumber) || q.CustomerOrderId.Contains(digitsOnlyNumber))));
                }
                query = query.Take(150);

                return query;
            }

            if (filter.DropShipperId.HasValue)
            {
                query = query.Where(q => q.DropShipperId == filter.DropShipperId);
            }

            //Default no batch
            if (!filter.IgnoreBatchFilter)
            {
                if (!filter.BatchId.HasValue && !filter.HasGlobalSearchParams())
                {
                    query = query.Where(q => q.BatchId == null);
                }
            }

            if (filter.From.HasValue)
            {
                query = query.Where(q => q.OrderDate.HasValue && q.OrderDate >= filter.From);
            }
            if (filter.To.HasValue)
            {
                query = query.Where(q => q.OrderDate.HasValue && q.OrderDate <= filter.To);
            }

            if (filter.OrderStatus != null && filter.OrderStatus.Any() && !filter.HasGlobalSearchParams())
            {
                query = query.Where(q => filter.OrderStatus.Contains(q.OrderStatus)
                    || (filter.IncludeForceVisible && q.IsForceVisible == true));
            }

            if (!String.IsNullOrEmpty(filter.FulfillmentChannel))
            {
                query = query.Where(q => q.FulfillmentChannel == filter.FulfillmentChannel);
            }

            if (filter.ExcludeOnHold)
            {
                query = query.Where(q => !q.OnHold);
            }

            if (filter.HasGlobalSearchParams())
            {
                if (!takeLimit.HasValue)
                    takeLimit = 8000;
            }
            else
            {
                //var fromDate = DateTime.Now.AddMonths(-3);
                //query = query.Where(q => q.OrderDate > fromDate);
            }

            if (takeLimit.HasValue)
            {
                query = query.OrderByDescending(o => o.OrderDate).Take(takeLimit.Value);
            }

            return query;
        }

        private IList<ListingOrderDTO> OrderItemsByLocation(IEnumerable<ListingOrderDTO> items)
        {
            return items.OrderBy(o => o.SortIsle)
                .ThenBy(o => o.SortSection)
                .ThenBy(o => o.SortShelf)
                .ThenBy(o => o.StyleID)
                .ThenBy(o => SizeHelper.GetSizeIndex(o.Size))
                .ThenBy(o => o.Title).ToList();
        }
       
        private IQueryable<long> GetOrderIdsByStyleQuery(string styleId)
        {
            var styleOrderIds = from o in unitOfWork.GetSet<Order>()
                join oi in unitOfWork.GetSet<ViewOrderItem>() on o.Id equals oi.OrderId
                join l in unitOfWork.GetSet<Listing>() on oi.ListingId equals l.Id
                where oi.StyleString.Contains(styleId)
                    || l.SKU == styleId
                orderby o.OrderDate descending 
                select o.Id;

            //styleOrderIds = styleOrderIds.Take(500); 
            //TODO: remove after add paging
            return styleOrderIds;
        }

        private IQueryable<long> GetOrderIdsByStyleItemIdQuery(long styleItemId)
        {
            var styleOrderIds = from o in unitOfWork.GetSet<Order>()
                                join oi in unitOfWork.GetSet<ViewOrderItem>() on o.Id equals oi.OrderId
                                where oi.StyleItemId == styleItemId
                                orderby o.OrderDate descending
                                select o.Id;

            styleOrderIds = styleOrderIds.Take(500);
            //TODO: remove after add paging
            return styleOrderIds;
        }


        public IList<SoldSizeInfo> GetPendingAndOtherUnshippedOrderItemQtyes(long? excludedBatchId)
        {
            var query = from o in unitOfWork.GetSet<Order>()
                        join oi in unitOfWork.GetSet<ViewOrderItem>() on o.Id equals oi.OrderId
                                where o.OrderStatus == OrderStatusEnumEx.Pending
                                    || (o.OrderStatus == OrderStatusEnumEx.Unshipped
                            //NOTE: on Order page excludeBatchId is empty (exclude all orders with NOT emtpy batchId)
                            //on Batch page excludeBatchId is not empty (exclude all orders with not equal batchId)
                            && (
                                (excludedBatchId.HasValue && o.BatchId.Value != excludedBatchId)
                                || (!excludedBatchId.HasValue && o.BatchId.HasValue)
                               )
                              )
                            group oi by oi.StyleItemId into byStyleItem
                            select new SoldSizeInfo
                            {
                                StyleItemId = byStyleItem.Key,
                                SoldQuantity = byStyleItem.Sum(i => i.QuantityOrdered)
                            };

            return query.ToList();
        }

        public GridResponse<DTOOrder> GetDisplayOrdersShort(IWeightService weightService, OrderSearchFilter filter)
        {
            throw new NotImplementedException();
        }
    }
}
