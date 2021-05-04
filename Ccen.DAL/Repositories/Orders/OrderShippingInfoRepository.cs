using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Features;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Helpers;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using Amazon.DTO.Listings;

namespace Amazon.DAL.Repositories
{
    public class OrderShippingInfoRepository : Repository<OrderShippingInfo>, IOrderShippingInfoRepository
    {
        public OrderShippingInfoRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IEnumerable<ShippingDTO> GetOrdersToFulfillAsDTO(MarketType market, string marketplaceId)
        {
            var query = from sh in GetAll()
                join o in unitOfWork.Orders.GetAll() on sh.OrderId equals o.Id
                join method in unitOfWork.ShippingMethods.GetAll() on sh.ShippingMethodId equals method.Id
                where !string.IsNullOrEmpty(sh.TrackingNumber)
                      && sh.IsFulfilled == false
                      && !sh.LabelCanceled
                      && sh.ShippingMethodId != ShippingUtils.DynamexPTPSameShippingMethodId
                      && sh.ShippingMethodId != ShippingUtils.AmazonDhlExpressMXShippingMethodId //NOTE: Exclude DHLMX, Amazon return error when submit it!
                      && o.OrderStatus != OrderStatusEnumEx.Canceled //In case if was cancelled after label purchase
                      && (o.Market != (int)MarketType.Amazon || o.OrderStatus == OrderStatusEnumEx.Unshipped)
                select new ShippingDTO()
                {
                    Id = sh.Id,
                    Market = o.Market,
                    MarketplaceId = o.MarketplaceId,
                    IsFromMailPage = false,
                    AmazonIdentifier = o.AmazonIdentifier,
                    MarketOrderId = o.MarketOrderId,
                    OrderId = o.Id,
                    TrackingNumber = sh.TrackingNumber,
                    CustomCurrier = sh.CustomCarrier,
                    CustomShippingMethodName = sh.CustomShippingMethodName,
                    ShippingMethodId = sh.ShippingMethodId ?? 0,
                    OrderDate = o.OrderDate,
                    OrderStatus = o.OrderStatus,
                    ShippingDate = sh.ShippingDate ?? sh.LabelPurchaseDate ?? DateTime.Now,
                    ShippingService = string.Empty,

                    ShippingMethod = new ShippingMethodDTO()
                    {
                        Id = method.Id,
                        Name = method.Name,
                        ShortName = method.ShortName ?? method.Name,
                        RequiredPackageSize = method.RequiredPackageSize,
                        CarrierName = method.CarrierName,
                        IsInternational = method.IsInternational,
                        ShipmentProviderType = method.ShipmentProviderType,
                    }
                };

            if (market != MarketType.None)
                query = query.Where(sh => sh.Market == (int)market);
            if (!String.IsNullOrEmpty(marketplaceId))
                query = query.Where(sh => sh.MarketplaceId == marketplaceId);

            return query;
        }

        public IQueryable<OrderShippingInfoDTO> GetAllAsDto()
        {
            var query = from o in unitOfWork.GetSet<Order>()
                        join s in unitOfWork.GetSet<OrderShippingInfo>() on o.Id equals s.OrderId
                        join sm in unitOfWork.GetSet<ShippingMethod>() on s.ShippingMethodId equals sm.Id

                        select new OrderShippingInfoDTO
                        {
                            Id = s.Id,
                            OrderId = o.Id,
                            OrderAmazonId = o.AmazonIdentifier,
                            Market = o.Market,
                            MarketplaceId = o.MarketplaceId,
                            OrderType = o.OrderType,

                            LabelFromType = (int)LabelFromType.Batch,
                            LabelPath = s.LabelPath,
                            StampsTxId = s.StampsTxId,

                            ScanFormId = s.ScanFormId,

                            LabelPurchaseDate = s.LabelPurchaseDate,
                            LabelPurchaseBy = s.LabelPurchaseBy,
                            LabelPurchaseResult = s.LabelPurchaseResult,
                            LabelPurchaseMessage = s.LabelPurchaseMessage,

                            OnHold = o.OnHold,
                            PrintLabelPackId = s.LabelPrintPackId,
                            
                            BatchId = o.BatchId,
                            NumberInBatch = s.NumberInBatch,
                            CustomLabelSortOrder = s.CustomLabelSortOrder,

                            PersonName = o.PersonName,
                            BuyerName = o.BuyerName,

                            DeliveryDays = s.DeliveryDays,
                            DeliveredStatus = s.DeliveredStatus,
                            ActualDeliveryDate = s.ActualDeliveryDate,
                            DeliveryDaysInfo = s.DeliveryDaysInfo,
                            EstimatedDeliveryDate = s.EstimatedDeliveryDate,

                            StampsShippingCost = s.StampsShippingCost,

                            TotalPrice = o.TotalPrice,
                            TotalPriceCurrency = o.TotalPriceCurrency,

                            IsInsured = o.IsInsured,
                            InsuranceCost = s.InsuranceCost,

                            IsSignConfirmation = o.IsSignConfirmation,
                            SignConfirmationCost = s.SignConfirmationCost,
                            UpChargeCost = s.UpChargeCost,

                            ShipmentProviderType = s.ShipmentProviderType,
                            ShipmentOfferId = s.ShipmentOfferId,

                            CancelLabelRequested = s.CancelLabelRequested,
                            LabelCanceled = s.LabelCanceled,

                            TrackingNumber = s.TrackingNumber,
                            TrackingStateDate = s.TrackingStateDate,
                            TrackingStateEvent = s.TrackingStateEvent,

                            ShippingDate = s.ShippingDate,
                            IsActive = s.IsActive,
                            IsVisible = s.IsVisible,
                            ShippingGroupId = s.ShippingGroupId,
                            ShippingNumber = s.ShippingNumber,

                            WeightD = s.UsedWeight ?? 0,

                            PackageLength = s.PackageLength,
                            PackageWidth = s.PackageWidth,
                            PackageHeight = s.PackageHeight,

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
                                RotationAngle = sm.RotationAngle,
                                IsCroppedLabel = sm.IsCroppedLabel,
                                IsFullPagePrint = sm.IsFullPagePrint,

                                IsSupportReturnToPOBox = sm.IsSupportReturnToPOBox,
                                PackageNameOnLabel = sm.PackageNameOnLabel,
                            },

                            ToAddress = new AddressDTO()
                            {
                                Address1 = o.ShippingAddress1,
                                Address2 = o.ShippingAddress2,
                                City = o.ShippingCity,
                                State = o.ShippingState,
                                Zip = o.ShippingZip,
                                ZipAddon = o.ShippingZipAddon,
                                Country = o.ShippingCountry,

                                Phone = o.ShippingPhone,
                                FullName = o.PersonName,

                                IsResidential = o.ShippingAddressIsResidential,
                                IsManuallyUpdated = o.IsManuallyUpdated,

                                ManuallyAddress1 = o.ManuallyShippingAddress1,
                                ManuallyAddress2 = o.ManuallyShippingAddress2,
                                ManuallyCity = o.ManuallyShippingCity,
                                ManuallyState = o.ManuallyShippingState,
                                ManuallyZip = o.ManuallyShippingZip,
                                ManuallyZipAddon = o.ManuallyShippingZipAddon,
                                ManuallyCountry = o.ManuallyShippingCountry,

                                ManuallyPhone = o.ManuallyShippingPhone,
                                ManuallyFullName = o.ManuallyPersonName,
                            },
                        };

            return query;
        }

        /// <summary>
        /// For print labels / label
        /// </summary>
        /// <param name="orderIds"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public IList<OrderShippingInfoDTO> GetOrderInfoWithItems(IWeightService weightService, 
            IList<long> orderIds, 
            SortMode sort, 
            bool unmaskReferenceStyle, 
            bool includeSourceItems,
            bool onlyIsActive = true)
        {
            var query = GetAllAsDto().Where(sh => orderIds.Contains(sh.OrderId));
            if (onlyIsActive)
                query = query.Where(sh => sh.IsActive);

            var shippingList = query.ToList();
            var shippingIdList = shippingList.Select(sh => sh.Id).ToList();

            var items = GetShippingItemsIncludeLocation(shippingIdList, unmaskReferenceStyle);
            foreach (var shippingInfo in shippingList)
            {
                var shippingItems = items.Where(i => i.ShippingInfoId == shippingInfo.Id)
                    .Select(i => new DTOOrderItem()
                    {
                        ASIN = i.ASIN,
                        ParentASIN = i.ParentASIN,
                        SKU = i.SKU,

                        ItemOrderId = i.ItemOrderId,
                        SourceItemOrderId = i.SourceItemOrderIdentifier,

                        Weight = i.Weight ?? 0,
                        PackageLength = i.PackageLength,
                        PackageWidth = i.PackageWidth,
                        PackageHeight = i.PackageHeight,

                        Quantity = i.QuantityOrdered,

                        ItemPrice = i.ItemPrice,
                        ItemPriceCurrency = i.ItemPriceCurrency,
                        Title = i.Title,
                        Size = i.Size,
                        Color = i.Color,

                        StyleId = i.StyleID,
                        StyleEntityId = i.StyleId,
                        StyleItemId = i.StyleItemId,
                        StyleSize = i.StyleSize,
                        StyleColor = i.StyleColor,

                        ReplaceType = i.ReplaceType,

                        Locations = i.Locations
                    })
                    .ToList();

                shippingInfo.Items = OrderItemsByLocation(shippingItems);
            }

            foreach (var shippingInfo in shippingList)
            {
                var weight = shippingInfo.Items.Sum(i => i.Weight * i.Quantity);
                shippingInfo.WeightD = weightService == null ? weight : weightService.AdjustWeight(weight, shippingInfo.Items.Sum(i => i.Quantity));

                var packageSize = weightService == null ? new ItemPackageDTO() : weightService.ComposePackageSize(shippingInfo.Items);

                //If no maually entered values, set to auto calculated
                if (!shippingInfo.PackageHeight.HasValue
                    || !shippingInfo.PackageLength.HasValue
                    || !shippingInfo.PackageWidth.HasValue
                    || shippingInfo.PackageHeight.Value == 0
                    || shippingInfo.PackageLength.Value == 0
                    || shippingInfo.PackageWidth.Value == 0)
                {
                    shippingInfo.PackageLength = packageSize.PackageLength;
                    shippingInfo.PackageWidth = packageSize.PackageWidth;
                    shippingInfo.PackageHeight = packageSize.PackageHeight;
                }

                shippingInfo.TotalQuantity = shippingInfo.Items.Sum(i => i.Quantity);
            }

            return shippingList;
        }

        /// <summary>
        /// For print Packing Slip
        /// </summary>
        /// <param name="orderIds"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public IEnumerable<PackingListDTO> GetPackingSlipOrders(long[] orderIds, SortMode sort, bool unmaskReferenceStyle)
        {
            var query = from o in unitOfWork.GetSet<Order>()
                join s in unitOfWork.GetSet<OrderShippingInfo>() on o.Id equals s.OrderId
                join sm in unitOfWork.GetSet<ShippingMethod>() on s.ShippingMethodId equals sm.Id
                join ml in unitOfWork.GetSet<MailLabelInfo>() on o.Id equals ml.OrderId into withMl
                from ml in withMl.DefaultIfEmpty()
                join mlm in unitOfWork.GetSet<ShippingMethod>() on ml.ShippingMethodId equals mlm.Id into withMlm
                from mlm in withMlm.DefaultIfEmpty()

                where orderIds.Contains(o.Id) 
                    && s.IsActive 
                    && s.IsVisible
                select new PackingListDTO
                {
                    Id = o.Id,

                    Market = o.Market,
                    MarketplaceId = o.MarketplaceId,

                    OrderEntityId = o.Id,
                    OrderId = o.AmazonIdentifier,
                    SalesRecordNumber = o.SalesRecordNumber,
                    OrderStatus = o.OrderStatus,

                    OnHold = o.OnHold,

                    Quantity = o.Quantity,
                    BuyerName = o.BuyerName,
                    PersonName = o.PersonName,

                    ShippingMethodName = mlm != null ? mlm.Name : sm.Name,
                    ShippingMethodId = mlm != null ? mlm.Id : sm.Id,

                    Carrier = mlm != null ? mlm.CarrierName : sm.CarrierName,
                    StampsShippingCost = ml != null ? ml.StampsShippingCost : s.StampsShippingCost,
                    InitialServiceType = o.InitialServiceType,

                    ShippingInfoId = s.Id,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,

                    BatchId = o.BatchId,
                    NumberInBatch = s.NumberInBatch,

                    ShippingCountry = o.ShippingCountry,
                    ShippingPhone = ml != null ? ml.Phone : o.ShippingPhone,
                    ShippingAddress1 = ml != null ? ml.Address1 : o.ShippingAddress1,
                    ShippingAddress2 = ml != null ? ml.Address2 : o.ShippingAddress2,
                    ShippingCity = ml != null ? ml.City : o.ShippingCity,
                    ShippingState = ml != null ? ml.State : o.ShippingState,
                    ShippingZip = ml != null ? ml.Zip : o.ShippingZip,
                    ShippingZipAddon = ml != null ? ml.ZipAddon : o.ShippingZipAddon,

                    IsManuallyUpdated = o.IsManuallyUpdated,

                    ManuallyPersonName = o.ManuallyPersonName,
                    ManuallyShippingCountry = o.ManuallyShippingCountry,
                    ManuallyShippingAddress1 = o.ManuallyShippingAddress1,
                    ManuallyShippingAddress2 = o.ManuallyShippingAddress2,
                    ManuallyShippingCity = o.ManuallyShippingCity,
                    ManuallyShippingState = o.ManuallyShippingState,
                    ManuallyShippingZip = o.ManuallyShippingZip,
                    ManuallyShippingZipAddon = o.ManuallyShippingZipAddon,
                    ManuallyShippingPhone = o.ManuallyShippingPhone,
                };

            var shippingList = query.ToList();
            var shippingIdList = shippingList.Select(sh => sh.ShippingInfoId).ToList();

            var orderIdList = shippingList.Select(sh => sh.OrderEntityId).Distinct().ToList();
            var notifiesList = unitOfWork.OrderNotifies.GetAllAsDto().Where(n => orderIdList.Contains(n.OrderId)
                    && n.Type == (int)OrderNotifyType.MarketComment)
                .ToList();

            var items = GetShippingItemsIncludeLocation(shippingIdList, unmaskReferenceStyle);// query.Select(s => s.ShippingInfoId));
            foreach (var shipping in shippingList)
            {
                shipping.Items = OrderItemsByLocation(items.Where(i => i.ShippingInfoId == shipping.ShippingInfoId).ToList());
                shipping.Notifies = notifiesList.Where(n => n.OrderId == shipping.OrderEntityId).ToList();
            }

            return shippingList;
        }

        private IList<ListingOrderDTO> GetShippingItemsIncludeLocation(IList<long> shippingInfoIdList,
            bool unmaskReferenceStyle)
        {
            var listingsQuery = unitOfWork.Listings.GetViewListingsAsDto(unmaskReferenceStyle);

            var itemQuery = (from l in listingsQuery
                             join oi in unitOfWork.GetSet<ViewOrderItem>() on l.Id equals oi.ListingId
                             join m in unitOfWork.GetSet<ItemOrderMapping>() on oi.Id equals m.OrderItemId
                             join st in unitOfWork.GetSet<Style>() on oi.StyleId equals st.Id
                             //join sh in shippingInfoIdList on m.ShippingInfoId equals sh
                             join sl in unitOfWork.StyleLocations.GetAll() on oi.StyleId equals sl.StyleId into withLocation
                             from sl in withLocation.DefaultIfEmpty()
                             where shippingInfoIdList.Contains(m.ShippingInfoId) &&
                                oi.QuantityOrdered > 0
                             select new ListingOrderDTO
                             {
                                 ShippingInfoId = m.ShippingInfoId,

                                 ASIN = l.ASIN,
                                 Market = l.Market,
                                 MarketplaceId = l.MarketplaceId,

                                 Size = l.Size,
                                 Color = l.Color,

                                 ItemId = l.ItemId,
                                 ParentASIN = l.ParentASIN,

                                 StyleId = oi.StyleId,
                                 StyleID = oi.StyleString,
                                 StyleItemId = oi.StyleItemId,
                                 StyleImage = oi.StyleImage,
                                 StyleSize = oi.StyleSize,
                                 StyleColor = oi.StyleColor,
                                 UseStyleImage = l.UseStyleImage,
                                 
                                 Weight = oi.Weight ?? 0,
                                 ShippingSize = oi.ShippingSize,
                                 PackageLength = oi.PackageLength,
                                 PackageWidth = oi.PackageWidth,
                                 PackageHeight = oi.PackageHeight,

                                 ReplaceType = oi.ReplaceType,

                                 //Prices
                                 ShippingPrice = oi.ShippingPrice,
                                 ShippingPriceCurrency = oi.ShippingPriceCurrency,
                                 ShippingDiscount = oi.ShippingDiscount,
                                 ShippingDiscountCurrency = oi.ShippingDiscountCurrency,

                                 ItemPaid = oi.ItemPaid,
                                 ItemPrice = oi.ItemPrice,
                                 ItemPriceCurrency = oi.ItemPriceCurrency,
                                 PromotionDiscount = oi.PromotionDiscount,
                                 PromotionDiscountCurrency = oi.PromotionDiscountCurrency,

                                 QuantityOrdered = m.MappedQuantity,
                                 QuantityShipped = oi.QuantityShipped,

                                 SourceStyleString = oi.SourceStyleString,
                                 SourceStyleItemId = oi.SourceStyleItemId,

                                 RecordNumber = oi.RecordNumber,
                                 ItemOrderId = oi.ItemOrderIdentifier,
                                 SourceItemOrderIdentifier = oi.SourceItemOrderIdentifier,

                                 ListingId = l.ListingId,
                                 SKU = l.SKU,
                                 Title = st.Name,// l.Title,
                                 Picture = l.Picture,
                                 ItemPicture = l.ItemPicture,

                                 Locations = new List<StyleLocationDTO>
                                 {
                                    new StyleLocationDTO
                                    {
                                        StyleId = l.StyleId ?? 0,
                                        SortIsle = sl != null ? sl.SortIsle : int.MaxValue,
                                        SortSection = sl != null ? sl.SortSection : int.MaxValue,
                                        SortShelf = sl != null ? sl.SortShelf : int.MaxValue,

                                        Isle = sl.Isle,
                                        Section = sl.Section,
                                        Shelf = sl.Shelf,

                                        IsDefault = sl != null && sl.IsDefault
                                    }
                                }
                             }).ToList();

            var items = itemQuery.Distinct(new DTOListingOrderComparer()).ToList();
            var locations = itemQuery.ToList();//.Distinct(new DTOListingOrderLocationComparer()).ToList();

            foreach (var item in items)
            {
                var itemLocations = locations.Where(l => l.ItemOrderId == item.ItemOrderId)
                        .OrderByDescending(i => i.Locations.First().IsDefault)
                        .Select(i => i.Locations.First())
                        .ToList();

                item.Locations = itemLocations;
            }

            return items;
        }

        private IList<ListingOrderDTO> OrderItemsByLocation(IEnumerable<ListingOrderDTO> items)
        {
            return items.OrderBy(o => o.SortIsle)
                .ThenBy(o => o.SortSection)
                .ThenBy(o => o.SortShelf)
                .ThenBy(o => o.StyleID)
                .ThenBy(o => SizeHelper.GetSizeIndex(o.StyleSize))
                .ThenBy(o => o.StyleColor)
                .ThenBy(o => o.Title)
                .ToList();
        }

        private IList<DTOOrderItem> OrderItemsByLocation(IEnumerable<DTOOrderItem> items)
        {
            return items.OrderBy(o => o.SortIsle)
                .ThenBy(o => o.SortSection)
                .ThenBy(o => o.SortShelf)
                .ThenBy(o => o.StyleId)
                .ThenBy(o => SizeHelper.GetSizeIndex(o.StyleSize))
                .ThenBy(o => o.StyleColor)
                .ThenBy(o => o.Title)
                .ToList();
        }
        
        public OrderShippingInfo CreateShippingInfo(RateDTO rate, 
            long orderId, 
            int shippingNumber, 
            int methodId)
        {
            var now = DateHelper.GetAppNowTime();
            var shippingInfo = new OrderShippingInfo
            {
                OrderId = orderId,
                ShippingMethodId = methodId,
                ShippingNumber = shippingNumber,
                ShippingGroupId = rate.GroupId,

                ShipmentOfferId = rate.OfferId,
                ShipmentProviderType = rate.ProviderType,

                StampsShippingCost = rate.Amount,
                InsuranceCost = rate.InsuranceCost,
                SignConfirmationCost = rate.SignConfirmationCost,
                UpChargeCost = rate.UpChargeCost,

                PackageLength = rate.PackageLength,
                PackageWidth = rate.PackageWidth,
                PackageHeight = rate.PackageHeight,
                
                ShippingDate = DateHelper.FitTOSQLDateTime(rate.ShipDate),
                EstimatedDeliveryDate = DateHelper.FitTOSQLDateTime(rate.DeliveryDate),
                EarliestDeliveryDate = DateHelper.FitTOSQLDateTime(rate.EarliestDeliveryDate),
                DeliveryDaysInfo = StringHelper.Substring(rate.DeliveryDaysInfo, 50),
                DeliveryDays = rate.DeliveryDays,

                LabelPath = null,
                LabelPrintPackId = null,
                NumberInBatch = rate.NumberInBatch,
                FeedId = null,
                IsActive = rate.IsDefault,
                IsVisible = rate.IsVisible,
                IsFeedSubmitted = false,
                IsFulfilled = false,
                MessageIdentifier = 0,
                TrackingNumber = null,
                CreateDate = now,
                UpdateDate = now
            };
            Add(shippingInfo);
            unitOfWork.Commit();

            return shippingInfo;
        }

        public IEnumerable<OrderShippingInfo> GetByOrderId(long id)
        {
            return GetFiltered(i => i.OrderId == id);
        }

        public IEnumerable<OrderShippingInfo> GetByOrderId(IList<long> idList)
        {
            return GetFiltered(i => idList.Contains(i.OrderId));
        }

        public IList<OrderShippingInfoDTO> GetByOrderIdAsDto(long orderId)
        {
            return GetAllAsDto().Where(sh => sh.OrderId == orderId).ToList();
        }

        public IEnumerable<ShippingDTO> GetOrdersAsShippingDTO(MarketType market, string marketplaceId)
        {
            throw new NotImplementedException();
        }
    }
}
