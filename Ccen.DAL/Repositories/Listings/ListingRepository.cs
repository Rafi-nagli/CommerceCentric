using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.DTO;
using Amazon.DTO.Listings;
using System.Linq.Expressions;

namespace Amazon.DAL.Repositories
{
    public class ListingRepository : Repository<Listing>, IListingRepository
    {
        public ListingRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IList<Listing> GetByListingId(string listingId, MarketType market, string marketplaceId)
        {
            if (!ArgumentHelper.CheckMarket(market))
                throw new ArgumentNullException("market");
            if (!ArgumentHelper.CheckMarketplaceId(market, marketplaceId))
                throw new ArgumentNullException("marketplaceId");

            return GetAll().Where(l => l.ListingId == listingId
                && !l.IsRemoved
                && l.Market == (int)market
                && (String.IsNullOrEmpty(marketplaceId) || l.MarketplaceId == marketplaceId))
                .ToList();
        }

        public Listing GetBySKU(string sku, MarketType market, string marketplaceId)
        {
            if (!ArgumentHelper.CheckMarket(market))
                throw new ArgumentNullException("market");
            if (!ArgumentHelper.CheckMarketplaceId(market, marketplaceId))
                throw new ArgumentNullException("marketplaceId");

            return GetAll().FirstOrDefault(l => l.SKU == sku
                && !l.IsRemoved
                && l.Market == (int)market
                && (String.IsNullOrEmpty(marketplaceId) || l.MarketplaceId == marketplaceId));
        }

        public IList<Listing> GetByListingIdIncludeRemoved(string listingId, MarketType market, string marketplaceId)
        {
            if (!ArgumentHelper.CheckMarket(market))
                throw new ArgumentNullException("market");
            if (!ArgumentHelper.CheckMarketplaceId(market, marketplaceId))
                throw new ArgumentNullException("marketplaceId");

            return GetAll().Where(l => l.ListingId == listingId
                && l.Market == (int)market
                && (String.IsNullOrEmpty(marketplaceId) || l.MarketplaceId == marketplaceId))
                .ToList();
        }

        public IList<StyleItemDTO> GetStyleItemIdListFromListingsBySKU(string sku)
        {
            var query = from l in GetAll()
                join i in unitOfWork.GetSet<Item>() on l.ItemId equals i.Id
                where l.SKU == sku
                      && i.StyleItemId.HasValue
                      && i.MarketplaceId != MarketplaceKeeper.AmazonMxMarketplaceId //NOTE: Exclude Mexico market, a lot of listings has incorrect sizes/mappings
                        orderby l.IsRemoved ascending
                select new StyleItemDTO()
                {
                    StyleId = i.StyleId ?? 0,
                    StyleItemId = i.StyleItemId.Value,
                };

            return query.ToList();
        }

        public void MarkAsRemoved(IList<long> listingIds)
        {
            listingIds = listingIds.Distinct().ToList();
            foreach (var listingId in listingIds)
            {
                var item = new Listing()
                {
                    Id = listingId,
                    IsRemoved = true,
                };

                TrackItem(item, new List<Expression<Func<Listing, object>>>()
                {
                    l => l.IsRemoved
                });
            }
            unitOfWork.Commit();
        }

        public IEnumerable<long> MarkNotExistingAsRemoved(IEnumerable<long> existListingIds, MarketType market, string marketplaceId)
        {
            var results = new List<long>();
            var toRemove = GetFiltered(l => !l.IsRemoved
                && l.Market == (int)market
                && l.MarketplaceId == marketplaceId
                && !existListingIds.Contains(l.Id));

            foreach (var listing in toRemove)
            {
                listing.IsRemoved = true;
                results.Add(listing.Id);
            }
            unitOfWork.Commit();

            return results;
        }

        public IEnumerable<string> MarkNotExistingAsRemoved(IEnumerable<string> existListingIds, 
            MarketType market, 
            string marketplaceId)
        {
            var results = new List<string>();
            var allListingIds = GetFiltered(l => !l.IsRemoved
                && l.Market == (int)market
                && l.MarketplaceId == marketplaceId)
                .ToList();
            var toRemove = allListingIds.Where(l => !existListingIds.Contains(l.ListingId)).ToList();


            //var toRemove = GetFiltered(l => !l.IsRemoved 
            //    && l.Market == (int)market 
            //    && l.MarketplaceId == marketplaceId
            //    && !existListingIds.Contains(l.ListingId));

            foreach (var listing in toRemove)
            {
                listing.IsRemoved = true;
                results.Add(listing.ListingId);
            }
            unitOfWork.Commit();

            return results;
        }

        public IList<Listing> GetPriceUpdateRequiredList(MarketType market, string marketplaceId)
        {
            var queryUpdateRequested = GetFiltered(l => !l.IsRemoved 
                //&& !l.IsFBA //FBA Available
                && l.PriceUpdateRequested
                && l.Market == (int)market);

            if (market == MarketType.Amazon || market == MarketType.AmazonEU || market == MarketType.AmazonAU)
                queryUpdateRequested = queryUpdateRequested.Where(l => !String.IsNullOrEmpty(l.SKU));

            if (!String.IsNullOrEmpty(marketplaceId))
                queryUpdateRequested = queryUpdateRequested.Where(l => l.MarketplaceId == marketplaceId);

            return queryUpdateRequested.ToList();
        }

        public IList<Listing> GetQuantityUpdateRequiredList(MarketType market, string marketplaceId)
        {
            var queryUpdateRequested = GetFiltered(l => !l.IsRemoved
                                         && !l.IsFBA
                                         && l.QuantityUpdateRequested
                                         && l.Market == (int) market);

            if (market == MarketType.Amazon || market == MarketType.AmazonEU || market == MarketType.AmazonAU)
                queryUpdateRequested = queryUpdateRequested.Where(l => !String.IsNullOrEmpty(l.SKU));

            if (!String.IsNullOrEmpty(marketplaceId))
                queryUpdateRequested = queryUpdateRequested.Where(l => l.MarketplaceId == marketplaceId);

            return queryUpdateRequested.ToList();
        }

        public IQueryable<ViewListing> GetViewListings()
        {
            return unitOfWork.GetSet<ViewListing>();
        }

        public IQueryable<ViewUnmaskedListing> GetViewUnmaskedListings()
        {
            return unitOfWork.GetSet<ViewUnmaskedListing>();
        }

        public IQueryable<ViewListingDTO> GetViewListingsAsDto(bool unmaskReferenceStyle)
        {
            IQueryable<ViewListingDTO> listingsQuery;

            listingsQuery = from l in unitOfWork.GetSet<ViewListing>()
                select new ViewListingDTO
                {
                    Id = l.Id,
                    ASIN = l.ASIN,
                    Market = l.Market,
                    MarketplaceId = l.MarketplaceId,

                    ItemId = l.ItemId,
                    ParentASIN = l.ParentASIN,
                    CreateDate = l.CreateDate,

                    StyleString = l.StyleString,
                    StyleId = l.StyleId,
                    StyleItemId = l.StyleItemId,
                    StyleSize = l.StyleSize,
                    StyleColor = l.StyleColor,
                    StyleImage = l.StyleImage,
                    UseStyleImage = l.UseStyleImage,

                    SourceMarketId = l.SourceMarketId,
                    SourceMarketUrl = l.SourceMarketUrl,

                    CurrentPrice = l.CurrentPrice,

                    Picture = l.Picture,
                    ItemPicture = l.ItemPicture,

                    ItemStyle = l.ItemStyle,
                    ShippingSize = l.ShippingSize,
                    InternationalPackage = l.InternationalPackage,
                    ListingId = l.ListingId,
                    SKU = l.SKU,
                    Size = l.Size,
                    Color = l.Color,
                    Title = l.Title,
                    Weight = l.Weight ?? 0,
                    Barcode = l.Barcode,

                    Quantity = l.Quantity,
                    Rank = l.Rank,

                    IsFBA = l.IsFBA,
                    IsPrime = l.IsPrime,
                    IsRemoved = l.IsRemoved,
                };
            
            return listingsQuery;
        }

        public IQueryable<ListingDTO> GetListingsAsListingDto()
        {
            var query = from l in unitOfWork.Listings.GetAll()
                        join lv in unitOfWork.GetSet<ViewListing>() on l.Id equals lv.Id
                        join bBox in unitOfWork.GetSet<BuyBoxStatus>() on new { lv.ASIN, lv.Market, lv.MarketplaceId }
                                    equals new { bBox.ASIN, bBox.Market, bBox.MarketplaceId } into withBBox
                        from bBox in withBBox.DefaultIfEmpty()
                        where l.IsRemoved == false
                        select new ListingDTO()
                        {
                            Id = l.Id,
                            Market = l.Market,
                            MarketplaceId = l.MarketplaceId,
                            ListingId = l.ListingId,
                            OnHold = l.OnHold,
                            RestockDate = l.RestockDate,

                            ParentASIN = lv.ParentASIN,
                            ASIN = lv.ASIN,
                            SourceMarketId = lv.SourceMarketId,
                            IsFBA = l.IsFBA,
                            IsPrime = l.IsPrime,
                            Weight = lv.Weight,
                            
                            SKU = l.SKU,
                            ItemId = l.ItemId,

                            StyleId = lv.StyleId,
                            StyleItemId = lv.StyleItemId,
                            StyleSize = lv.StyleSize,
                            StyleColor = lv.StyleColor,

                            ListingSize = lv.Size,
                            ListingColor = lv.Color,

                            Price = l.CurrentPrice,
                            Quantity = l.RealQuantity,
                            DisplayQuantity = l.DisplayQuantity,

                            Rank = lv.Rank,

                            ItemPicture = lv.ItemPicture,

                            LowestPrice = bBox.WinnerPrice,
                            LowestPriceUpdateDate = bBox.CheckedDate,
                        };

            return query;
        }


        public IList<ListingOrderDTO> GetOrderItems(long orderId)
        {
            return GetAllOrderItemsWithListingInfo().Where(l => l.OrderId == orderId).ToList();
        }

        public IList<ListingOrderDTO> GetOrderItems(string orderNumber)
        {
            return GetAllOrderItemsWithListingInfo().Where(l => l.OrderNumber == orderNumber).ToList();
        }

        public IQueryable<ListingOrderDTO> GetAllOrderItemsWithListingInfo()
        {
            var query = from l in unitOfWork.GetSet<ViewListing>()
                        join i in unitOfWork.GetSet<Item>() on l.ItemId equals i.Id
                        join oi in unitOfWork.GetSet<ViewOrderItem>() on l.Id equals oi.ListingId
                        join o in unitOfWork.Orders.GetAll() on oi.OrderId equals o.Id
                        select new ListingOrderDTO
                        {
                            ASIN = l.ASIN,
                            Market = l.Market,
                            MarketplaceId = l.MarketplaceId,

                            QuantityOrdered = oi.QuantityOrdered,
                            QuantityShipped = oi.QuantityShipped,

                            ItemPrice = oi.ItemPrice,
                            ItemPriceCurrency = oi.ItemPriceCurrency,
                            ItemPriceInUSD = oi.ItemPriceInUSD,

                            PromotionDiscount = oi.PromotionDiscount,
                            PromotionDiscountCurrency = oi.PromotionDiscountCurrency,
                            PromotionDiscountInUSD = oi.PromotionDiscountInUSD,
                            
                            ShippingPrice = oi.ShippingPrice,
                            ShippingPriceCurrency = oi.ShippingPriceCurrency,
                            ShippingPriceInUSD = oi.ShippingPriceInUSD,

                            ShippingDiscount = oi.ShippingDiscount,
                            ShippingDiscountCurrency = oi.ShippingDiscountCurrency,
                            ShippingDiscountInUSD = oi.ShippingDiscountInUSD,

                            OrderNumber = o.AmazonIdentifier,
                            OrderId = o.Id,

                            OrderItemEntityId = oi.Id,

                            ItemOrderId = oi.ItemOrderIdentifier,
                            SourceListingId = oi.SourceListingId,

                            SourceMarketId = l.SourceMarketId,
                            ParentSourceMarketId = l.ParentSourceMarketId,

                            StyleItemId = oi.StyleItemId,
                            StyleId = oi.StyleId,
                            StyleID = oi.StyleString,
                            StyleImage = l.StyleImage,

                            SourceStyleString = oi.SourceStyleString,
                            SourceStyleSize = oi.SourceStyleSize,
                            SourceStyleColor = oi.SourceStyleColor,
                            SourceStyleItemId = oi.SourceStyleItemId,

                            Weight = oi.Weight,
                            ShippingSize = oi.ShippingSize,
                            PackageLength = oi.PackageLength,
                            PackageWidth = oi.PackageWidth,
                            PackageHeight = oi.PackageHeight,

                            InternationalPackage = oi.InternationalPackage,
                            ItemStyle = oi.ItemStyle,
                            RestockDate = oi.RestockDate,
                            OnMarketTemplateName = i.OnMarketTemplateName,

                            SKU = l.SKU,
                            Id = l.Id,
                            ListingId = l.ListingId,
                            Size = l.Size,
                            ParentASIN = l.ParentASIN,
                            Picture = l.Picture,
                            Title = l.Title,

                            RealQuantity = l.Quantity
                        };

            return query;
        }

        public IList<ListingOrderDTO> GetOrderItemSources(long orderId)
        {
            return GetAllOrderItemSourcesWithListingInfo().Where(l => l.OrderId == orderId).ToList();
        }

        public IList<ListingOrderDTO> GetOrderItemSources(string orderNumber)
        {
            return GetAllOrderItemSourcesWithListingInfo().Where(l => l.OrderNumber == orderNumber).ToList();
        }

        /// <summary>
        /// TODO: get style info from SourceOrderItem, not from Listing
        /// </summary>
        /// <returns></returns>
        private IQueryable<ListingOrderDTO> GetAllOrderItemSourcesWithListingInfo()
        {
            var query = from l in unitOfWork.GetSet<ViewListing>()
                        join oi in unitOfWork.OrderItemSources.GetAll() on l.Id equals oi.ListingId
                        join o in unitOfWork.Orders.GetAll() on oi.OrderId equals o.Id
                        select new ListingOrderDTO
                        {
                            ASIN = l.ASIN,
                            Market = l.Market,
                            MarketplaceId = l.MarketplaceId,

                            QuantityOrdered = oi.QuantityOrdered,
                            QuantityShipped = oi.QuantityShipped,

                            ItemPrice = oi.ItemPrice,
                            ItemPriceCurrency = oi.ItemPriceCurrency,
                            ItemPriceInUSD = oi.ItemPriceInUSD,
                            
                            PromotionDiscount = oi.PromotionDiscount,
                            PromotionDiscountCurrency = oi.PromotionDiscountCurrency,
                            PromotionDiscountInUSD = oi.PromotionDiscountInUSD,

                            ShippingPrice = oi.ShippingPrice,
                            ShippingPriceCurrency = oi.ShippingPriceCurrency,
                            ShippingPriceInUSD = oi.ShippingPriceInUSD,

                            ShippingDiscount = oi.ShippingDiscount,
                            ShippingDiscountCurrency = oi.ShippingDiscountCurrency,
                            ShippingDiscountInUSD = oi.ShippingDiscountInUSD,

                            OrderItemEntityId = oi.Id,
                            OrderNumber = o.AmazonIdentifier,
                            OrderId = o.Id,

                            ItemOrderId = oi.ItemOrderIdentifier,
                            
                            StyleItemId = l.StyleItemId,
                            StyleId = l.StyleId,
                            StyleID = l.StyleString,
                            Weight = l.Weight,
                            ShippingSize = l.ShippingSize,
                            PackageLength = l.PackageLength,
                            PackageWidth = l.PackageWidth,
                            PackageHeight = l.PackageHeight,
                            InternationalPackage = l.InternationalPackage,
                            RestockDate = l.RestockDate,

                            SKU = l.SKU,
                            Id = l.Id,
                            ListingId = l.ListingId,
                            Title = l.Title,
                            Size = l.Size,
                            ParentASIN = l.ParentASIN,
                            Picture = l.Picture,
                            RealQuantity = l.Quantity
                        };

            return query;
        }


        /// <summary>
        /// Update Order Items if hasn't listring for one item return completely empty list
        /// </summary>
        /// <param name="orderItems"></param>
        /// <returns></returns>
        public bool FillOrderItemsBySKU(IList<ListingOrderDTO> orderItems, 
            MarketType market, 
            string marketplaceId)
        {
            if (!ArgumentHelper.CheckMarket(market))
                throw new ArgumentNullException("market");
            if (!ArgumentHelper.CheckMarketplaceId(market, marketplaceId))
                throw new ArgumentNullException("marketplaceId");

            var result = true;
            foreach (var dto in orderItems)
            {
                var query = from vl in unitOfWork.GetSet<ViewListing>()
                            join i in unitOfWork.GetSet<Item>() on vl.ItemId equals i.Id
                            where vl.SKU == dto.SKU
                                && vl.Market == (int)market
                                && vl.StyleId != null
                                && vl.StyleItemId != null
                            select new { vl, i };
                
                var view = query.ToList();
                var listing = view.FirstOrDefault(v => String.IsNullOrEmpty(marketplaceId) || v.vl.MarketplaceId == marketplaceId);

                if (market == MarketType.Amazon || market == MarketType.AmazonEU || market == MarketType.AmazonAU)
                {
                    var listingsList = view.Where(l => l.vl.MarketplaceId == marketplaceId
                        && !l.vl.IsRemoved).OrderBy(l => l.vl.CreateDate).ToList();

                    if (listingsList.Count > 0)
                    {
                        listing = listingsList.FirstOrDefault(l => l.vl.Quantity > 0);
                        if (listing == null)
                            listing = listingsList.LastOrDefault();
                    }

                    if (listing == null)
                        listing = view.FirstOrDefault(l => l.vl.MarketplaceId == marketplaceId); //Include removed

                    if (listing == null && market == MarketType.AmazonEU)
                        listing = view.FirstOrDefault(); //Ignore marketplaceId, temporary enable for EU
                }
                else
                {
                    //For other market
                    listing = view.FirstOrDefault();
                }

                if (listing != null 
                    && listing.vl.StyleItemId.HasValue) //NOTE: only if listing has linked styleItemId, otherwise we skipped that listing!!! No needed listing w/o style item
                {
                    dto.Id = listing.vl.Id;
                    dto.SKU = listing.vl.SKU;
                    dto.ListingId = listing.vl.ListingId;
                    dto.DropShipperId = listing.vl.DropShipperId;

                    dto.Weight = listing.vl.Weight;
                    dto.ShippingSize = listing.vl.ShippingSize;
                    dto.PackageLength = listing.vl.PackageLength;
                    dto.PackageWidth = listing.vl.PackageWidth;
                    dto.PackageHeight = listing.vl.PackageHeight;
                    dto.InternationalPackage = listing.vl.InternationalPackage;
                    dto.ItemStyle = listing.vl.ItemStyle;
                    dto.RestockDate = listing.vl.RestockDate;
                    dto.Size = listing.vl.Size;
                    dto.ParentASIN = listing.vl.ParentASIN;
                    dto.Picture = listing.vl.Picture;

                    dto.StyleItemId = listing.vl.StyleItemId;
                    dto.StyleId = listing.vl.StyleId;
                    dto.StyleID = listing.vl.StyleString;
                    dto.StyleSize = listing.vl.StyleSize;
                    dto.StyleColor = listing.vl.StyleColor;

                    dto.RealQuantity = listing.vl.Quantity;
                    dto.OnMarketTemplateName = listing.i.OnMarketTemplateName;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }

        public bool FillOrderItemsBySKUOrBarcode(IList<ListingOrderDTO> orderItems,
            MarketType market,
            string marketplaceId)
        {
            if (!ArgumentHelper.CheckMarket(market))
                throw new ArgumentNullException("market");
            if (!ArgumentHelper.CheckMarketplaceId(market, marketplaceId))
                throw new ArgumentNullException("marketplaceId");

            var result = true;
            foreach (var dto in orderItems)
            {
                var query = from vl in unitOfWork.GetSet<ViewListing>()
                            join i in unitOfWork.GetSet<Item>() on vl.ItemId equals i.Id
                            where vl.SKU == dto.SKU
                                && vl.Market == (int)market
                                && vl.StyleId != null
                                && vl.StyleItemId != null
                            select new { vl, i };

                var view = query.ToList();
                
                //NOTE: Try by barcode
                if (view.Count == 0)
                {
                    view = (from vl in unitOfWork.GetSet<ViewListing>()
                                join i in unitOfWork.GetSet<Item>() on vl.ItemId equals i.Id
                                where i.Barcode == dto.Barcode
                                    && vl.Market == (int)market
                                    && vl.StyleId != null
                                    && vl.StyleItemId != null
                                select new { vl, i }).ToList();
                    
                }
                               
                //NOTE: Enable filtering by marketplace (only when we have there listings
                if (view.Count(v => v.i.MarketplaceId == marketplaceId) > 0)
                {
                    view = view.Where(v => v.i.MarketplaceId == marketplaceId).ToList();
                }

                var listing = view.FirstOrDefault();

                if (market == MarketType.Amazon || market == MarketType.AmazonEU || market == MarketType.AmazonAU)
                {
                    var listingsList = view.Where(l => l.vl.MarketplaceId == marketplaceId
                        && !l.vl.IsRemoved).OrderBy(l => l.vl.CreateDate).ToList();

                    if (listingsList.Count > 0)
                    {
                        listing = listingsList.FirstOrDefault(l => l.vl.Quantity > 0);
                        if (listing == null)
                            listing = listingsList.LastOrDefault();
                    }

                    if (listing == null)
                        listing = view.FirstOrDefault(l => l.vl.MarketplaceId == marketplaceId); //Include removed

                    if (listing == null && market == MarketType.AmazonEU)
                        listing = view.FirstOrDefault(); //Ignore marketplaceId, temporary enable for EU
                }
                else
                {
                    //For other market
                    listing = view.FirstOrDefault();
                }

                if (listing != null
                    && listing.vl.StyleItemId.HasValue) //NOTE: only if listing has linked styleItemId, otherwise we skipped that listing!!! No needed listing w/o style item
                {
                    dto.Id = listing.vl.Id;
                    dto.SKU = listing.vl.SKU;
                    dto.ListingId = listing.vl.ListingId;
                    dto.DropShipperId = listing.vl.DropShipperId;

                    dto.Weight = listing.vl.Weight;
                    dto.ShippingSize = listing.vl.ShippingSize;
                    dto.PackageLength = listing.vl.PackageLength;
                    dto.PackageWidth = listing.vl.PackageWidth;
                    dto.PackageHeight = listing.vl.PackageHeight;
                    dto.InternationalPackage = listing.vl.InternationalPackage;
                    dto.ItemStyle = listing.vl.ItemStyle;
                    dto.RestockDate = listing.vl.RestockDate;
                    dto.Size = listing.vl.Size;
                    dto.ParentASIN = listing.vl.ParentASIN;
                    dto.Picture = listing.vl.Picture;

                    dto.StyleItemId = listing.vl.StyleItemId;
                    dto.StyleId = listing.vl.StyleId;
                    dto.StyleID = listing.vl.StyleString;
                    dto.StyleSize = listing.vl.StyleSize;
                    dto.StyleColor = listing.vl.StyleColor;

                    dto.RealQuantity = listing.vl.Quantity;
                    dto.OnMarketTemplateName = listing.i.OnMarketTemplateName;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }

        public bool FillOrderItemsByStyleAndSize(IList<ListingOrderDTO> orderItems,
            MarketType market,
            string marketplaceId)
        {
            if (!ArgumentHelper.CheckMarket(market))
                throw new ArgumentNullException("market");
            if (!ArgumentHelper.CheckMarketplaceId(market, marketplaceId))
                throw new ArgumentNullException("marketplaceId");

            var result = false;
            foreach (var dto in orderItems)
            {
                long? styleId = unitOfWork.GetSet<Style>()
                    .Where(s => s.StyleID == dto.StyleID
                        && !s.Deleted)
                    .Select(s => s.Id)
                    .FirstOrDefault();

                StyleItem styleItem = null;
                if (styleId.HasValue)
                {
                    var styleItems = unitOfWork.GetSet<StyleItem>()
                        .Where(si => si.StyleId == styleId.Value)
                        .ToList();

                    var candidates = styleItems;
                    if (candidates.Count > 1)
                    {
                        if (!String.IsNullOrEmpty(dto.StyleSize))
                        {
                            candidates = styleItems.Where(si => si.Size == dto.StyleSize).ToList();
                            if (candidates.Count == 0)
                            {
                                var preparedSize = dto.StyleSize.Replace("Y", "").Replace("T", ""); //NOTE: for 5Y, 4T sizes
                                candidates = styleItems.Where(si => si.Size.Replace("Y", "").Replace("T", "") == preparedSize).ToList();
                            }
                        }
                    
                        if (candidates.Count > 1)
                        {
                            if (!String.IsNullOrEmpty(dto.StyleColor))
                                candidates = styleItems.Where(si => si.Color == dto.StyleColor).ToList();
                        }
                    }

                    if (candidates.Count == 1)
                    {
                        styleItem = candidates.FirstOrDefault();
                    }
                }

                if (styleItem != null)
                {
                    var listingQuery = from vl in unitOfWork.GetSet<ViewListing>()
                                join i in unitOfWork.GetSet<Item>() on vl.ItemId equals i.Id
                                where vl.Market == (int)market
                                    && (vl.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))
                                    && vl.StyleId == styleItem.StyleId
                                    && vl.StyleItemId == styleItem.Id
                                orderby vl.IsRemoved descending
                                select new { vl, i };

                    var listing = listingQuery.FirstOrDefault();

                    if (listing != null
                        && listing.vl.StyleItemId.HasValue) //NOTE: only if listing has linked styleItemId, otherwise we skipped that listing!!! No needed listing w/o style item
                    {
                        dto.Id = listing.vl.Id;
                        dto.SKU = listing.vl.SKU;
                        dto.ListingId = listing.vl.ListingId;
                        dto.DropShipperId = listing.vl.DropShipperId;

                        dto.Weight = listing.vl.Weight;
                        dto.ShippingSize = listing.vl.ShippingSize;
                        dto.PackageLength = listing.vl.PackageLength;
                        dto.PackageWidth = listing.vl.PackageWidth;
                        dto.PackageHeight = listing.vl.PackageHeight;
                        dto.InternationalPackage = listing.vl.InternationalPackage;
                        dto.ItemStyle = listing.vl.ItemStyle;
                        dto.RestockDate = listing.vl.RestockDate;
                        dto.Size = listing.vl.Size;
                        dto.ParentASIN = listing.vl.ParentASIN;
                        dto.Picture = listing.vl.Picture;

                        dto.StyleItemId = listing.vl.StyleItemId;
                        dto.StyleId = listing.vl.StyleId;
                        dto.StyleID = listing.vl.StyleString;
                        dto.StyleSize = listing.vl.StyleSize;
                        dto.StyleColor = listing.vl.StyleColor;

                        dto.RealQuantity = listing.vl.Quantity;
                        dto.OnMarketTemplateName = listing.i.OnMarketTemplateName;

                        result = true;
                    }
                }
            }
            return result;
        }

        public bool FillOrderItemsBySourceMarketId(IList<ListingOrderDTO> orderItems, MarketType market, string marketplaceId)
        {
            if (!ArgumentHelper.CheckMarket(market))
                throw new ArgumentNullException("market");
            if (!ArgumentHelper.CheckMarketplaceId(market, marketplaceId))
                throw new ArgumentNullException("marketplaceId");

            var result = true;
            foreach (var dto in orderItems)
            {
                var listingList = (from vi in unitOfWork.GetSet<ViewListing>()
                    join pi in unitOfWork.GetSet<ParentItem>() on new {vi.ParentASIN, vi.Market, vi.MarketplaceId}
                        equals new {ParentASIN = pi.ASIN, pi.Market, pi.MarketplaceId}
                    where (pi.SourceMarketId == dto.ParentSourceMarketId
                            || vi.IsRemoved == true) //NOTE: Parent Ids should be same or listing marked as removed (in that case skip checking parent Ids, it had value from new listing)
                          && vi.SourceMarketId == dto.SourceMarketId
                          && vi.Market == (int) market
                          && (String.IsNullOrEmpty(marketplaceId) || vi.MarketplaceId == marketplaceId)
                    orderby vi.CreateDate descending
                    select vi).ToList();


                //TODO: I'm not sure is safely to take by ASIN (m.b. better to wait while appear the listing)
                //?? unitOfWork.GetSet<ViewListing>().FirstOrDefault(v => v.ASIN == dto.ASIN);
                ViewListing listing = null;
                if (listingList.Count > 0)
                {
                    listing = listingList.FirstOrDefault(l => l.Quantity > 0);
                    if (listing == null)
                        listing = listingList.LastOrDefault();
                }

                if (listing == null
                    && !String.IsNullOrEmpty(dto.SKU))
                {
                    var candidateQuery = from vi in unitOfWork.GetSet<ViewListing>()
                                         where vi.SKU == dto.SKU
                                           && vi.Market == (int)market
                                           && (String.IsNullOrEmpty(marketplaceId) || vi.MarketplaceId == marketplaceId)
                                         select vi;
                    var listingCandidates = candidateQuery.Count();

                    if (listingCandidates > 1)
                    {
                        var withNotRemovedCandidateQuery = candidateQuery.Where(c => !c.IsRemoved);
                        listingCandidates = withNotRemovedCandidateQuery.Count();
                        if (listingCandidates == 0) //All removed we get any of them
                            listingCandidates = 1;
                        else
                            candidateQuery = withNotRemovedCandidateQuery;
                    }

                    if (listingCandidates == 1)
                    {
                        listing = candidateQuery.FirstOrDefault();
                    }
                }

                if (listing != null
                    && listing.StyleItemId.HasValue) //NOTE: only if listing has linked styleItemId, otherwise we skipped that listing!!! No needed listing w/o style item
                {
                    dto.Id = listing.Id;
                    dto.SKU = listing.SKU;
                    dto.ASIN = listing.ASIN;
                    dto.ListingId = listing.ListingId;
                    dto.DropShipperId = listing.DropShipperId;

                    dto.Weight = listing.Weight;
                    dto.ShippingSize = listing.ShippingSize;
                    dto.PackageLength = listing.PackageLength;
                    dto.PackageWidth = listing.PackageWidth;
                    dto.PackageHeight = listing.PackageHeight;
                    dto.InternationalPackage = listing.InternationalPackage;
                    dto.ItemStyle = listing.ItemStyle;
                    dto.RestockDate = listing.RestockDate;
                    dto.FulfillDate = listing.FulfillDate;
                    dto.PreOrderExpReceiptDate = listing.PreOrderExpReceiptDate;

                    dto.Size = listing.Size;
                    dto.Color = listing.Color;
                    dto.ParentASIN = listing.ParentASIN;
                    dto.Picture = listing.Picture;

                    dto.StyleItemId = listing.StyleItemId;
                    dto.StyleId = listing.StyleId;
                    dto.StyleID = listing.StyleString;
                    dto.StyleSize = listing.StyleSize;
                    dto.StyleColor = listing.StyleColor;

                    dto.RealQuantity = listing.Quantity;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }

        public bool FillOrderItemsByListingId(IList<ListingOrderDTO> orderItems, MarketType market, string marketplaceId)
        {
            if (!ArgumentHelper.CheckMarket(market))
                throw new ArgumentNullException("market");
            if (!ArgumentHelper.CheckMarketplaceId(market, marketplaceId))
                throw new ArgumentNullException("marketplaceId");

            var result = true;
            foreach (var dto in orderItems)
            {
                var listingList = (from vi in unitOfWork.GetSet<ViewListing>()
                                  join i in unitOfWork.GetSet<Item>() on vi.ItemId equals i.Id
                                  where (vi.ListingId == dto.ListingId
                                        || vi.ASIN == dto.ListingId
                                        || i.Barcode == dto.ListingId)
                            && vi.Market == (int)market
                            && (String.IsNullOrEmpty(marketplaceId) || vi.MarketplaceId == marketplaceId)                            
                            orderby vi.CreateDate
                            select vi)
                    .ToList();

                //TODO: Not sure is it safely to take by ASIN (m.b. better to wait while appear the listing)
                //?? unitOfWork.GetSet<ViewListing>().FirstOrDefault(v => v.ASIN == dto.ASIN);
                ViewListing listing = null;
                if (listingList.Count > 0)
                {
                    listing = listingList.OrderByDescending(l => l.StyleItemId.HasValue ? 1 : 0)
                        .ThenByDescending(l => l.CreateDate)
                        .FirstOrDefault(l => l.Quantity > 0);
                    if (listing == null)
                        listing = listingList.OrderByDescending(l => l.StyleItemId.HasValue ? 1 : 0)
                            .ThenByDescending(l => l.CreateDate).FirstOrDefault();
                }

                if (listing == null
                    && !String.IsNullOrEmpty(dto.SKU))
                {
                    listing = (from vi in unitOfWork.GetSet<ViewListing>()
                               where vi.SKU == dto.SKU
                                 && vi.Market == (int)market
                                 && (String.IsNullOrEmpty(marketplaceId) || vi.MarketplaceId == marketplaceId)
                               select vi).FirstOrDefault();
                }

                if (listing == null
                        && !String.IsNullOrEmpty(dto.ListingId))
                {
                    listing = (from vi in unitOfWork.GetSet<ViewListing>()
                               where vi.ListingId == dto.ListingId
                                 && vi.Market == (int)market
                                 && (String.IsNullOrEmpty(marketplaceId) || vi.MarketplaceId == marketplaceId)
                               select vi).FirstOrDefault();
                }

                if (listing != null
                    && listing.StyleItemId.HasValue) //NOTE: only if listing has linked styleItemId, otherwise we skipped that listing!!! No needed listing w/o style item
                {
                    dto.Id = listing.Id;
                    dto.SKU = listing.SKU;
                    dto.ASIN = listing.ASIN;
                    dto.ListingId = listing.ListingId;
                    dto.DropShipperId = listing.DropShipperId;

                    dto.Weight = listing.Weight;
                    dto.ShippingSize = listing.ShippingSize;
                    dto.PackageLength = listing.PackageLength;
                    dto.PackageWidth = listing.PackageWidth;
                    dto.PackageHeight = listing.PackageHeight;
                    dto.InternationalPackage = listing.InternationalPackage;
                    dto.ItemStyle = listing.ItemStyle;
                    dto.RestockDate = listing.RestockDate;
                    dto.Size = listing.Size;
                    dto.ParentASIN = listing.ParentASIN;
                    dto.Picture = listing.Picture;

                    dto.StyleItemId = listing.StyleItemId;
                    dto.StyleId = listing.StyleId;
                    dto.StyleID = listing.StyleString;
                    dto.StyleSize = listing.StyleSize;
                    dto.StyleColor = listing.StyleColor;

                    dto.RealQuantity = listing.Quantity;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }

        public IQueryable<ListingQuantityDTO> GetAllAsQuantityDto()
        {
            var query = from lv in unitOfWork.GetSet<ViewListing>()
                        join l in unitOfWork.GetSet<Listing>() on lv.Id equals l.Id
                        join i in unitOfWork.GetSet<Item>() on l.ItemId equals i.Id
                        select new ListingQuantityDTO()
                {
                    Id = l.Id,
                    ListingId = l.ListingId,
                    SKU = l.SKU,
                    IsFBA = l.IsFBA,
                    IsRemoved = l.IsRemoved,
                    
                    Market = l.Market,
                    MarketplaceId = l.MarketplaceId,

                    //AutoQuantity = l.AutoQuantity,
                    ItemPublishedStatus = i.ItemPublishedStatus,
                    AutoQuantityUpdateDate = l.AutoQuantityUpdateDate,
                    RealQuantity = l.RealQuantity,
                    DisplayQuantity = l.DisplayQuantity,
                    QuantityUpdateRequested = l.QuantityUpdateRequested,
                    OnHold = l.OnHold,
                    OnHoldParent = lv.OnHoldParent,
                    RestockDate = l.RestockDate,

                    Rank = lv.Rank,

                    StyleSize = lv.StyleSize,
                    Size = lv.Size,

                    StyleId = lv.StyleId,
                    StyleItemId = lv.StyleItemId,
                };

            return query;
        }

        public bool CheckForExistenceSKU(string sku, MarketType market, string marketplaceId)
        {
            var query = GetFiltered(l => !l.IsRemoved
                && l.Market == (int)market
                && l.SKU == sku);

            if (!String.IsNullOrEmpty(marketplaceId))
                query = query.Where(l => l.MarketplaceId == marketplaceId);

            return query.FirstOrDefault() != null;
        }

        public IList<ItemDTO> CheckForExistence(IList<ItemDTO> result, MarketType market, string marketplaceId)
        {
            var existListingId = GetFiltered(l => !l.IsRemoved
                && l.Market == (int)market
                && l.MarketplaceId == marketplaceId)
                .Select(l => l.ListingId)
                .ToList();
            return result.Where(r => !existListingId.Contains(r.ListingId)).ToList();
        }

        /// <summary>
        /// Create new listing (and checking for existing in case )
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="item"></param>
        public Listing StoreOrUpdate(ItemDTO dto, 
            Item item, 
            MarketType market, 
            string marketplaceId, 
            DateTime when)
        {
            if (!ArgumentHelper.CheckMarket(market))
                throw new ArgumentNullException("market");
            if (!ArgumentHelper.CheckMarketplaceId(market, marketplaceId))
                throw new ArgumentNullException("marketplaceId");

            var listing = GetFiltered(l => l.ListingId == dto.ListingId
                && l.Market == (int)market
                && l.MarketplaceId == marketplaceId).FirstOrDefault();

            var priceChangeType = PriceChangeSourceType.None;
            decimal? oldPrice = null;
            if (listing == null)
            {
                priceChangeType = PriceChangeSourceType.Initial;

                listing = new Listing
                {
                    ListingId = dto.ListingId,
                    Market = (int)market,
                    MarketplaceId = marketplaceId,
                    SKU = dto.SKU,
                    ASIN = dto.ASIN,
                    IsFBA = dto.IsFBA,
                    CurrentPrice = dto.CurrentPrice,
                    CurrentPriceCurrency = dto.CurrentPriceCurrency,
                    CurrentPriceInUSD = dto.CurrentPriceInUSD,

                    BusinessPrice = dto.BusinessPrice,

                    RealQuantity = dto.RealQuantity,
                    DisplayQuantity = dto.DisplayQuantity,

                    RestockDate = dto.RestockDate,

                    AmazonCurrentPrice = dto.CurrentPrice,
                    AmazonRealQuantity = dto.RealQuantity,

                    LowestPrice = dto.LowestPrice,

                    OpenDate = dto.OpenDate,
                    CreateDate = when,
                    UpdateDate = when,
                    IsRemoved = false,

                    ItemId = item.Id,
                };
                
                Add(listing);
                //NOTE: Commit to get db ID for the PriceHistories
                unitOfWork.Commit();

                unitOfWork.QuantityHistories.Add(new QuantityHistory()
                {
                    ListingId = listing.Id,
                    Type = (int) QuantityChangeSourceType.Initial,
                    QuantityChanged = listing.RealQuantity,
                    SKU = listing.SKU,
                    StyleString = item.StyleString,
                    Size = item.Size,
                    CreateDate = when,
                    CreatedBy = null
                });
            }
            else
            {
                //NOTE: Enter here if listing has IsRemoved flag

                if (listing.CurrentPrice != dto.CurrentPrice)
                {
                    priceChangeType = PriceChangeSourceType.Initial;
                    oldPrice = listing.CurrentPrice;
                }

                listing.SKU = dto.SKU;
                listing.ASIN = dto.ASIN;
                listing.Market = (int)market;
                listing.MarketplaceId = marketplaceId;

                listing.IsFBA = dto.IsFBA;

                listing.CurrentPrice = dto.CurrentPrice;
                listing.CurrentPriceCurrency = dto.CurrentPriceCurrency;
                listing.CurrentPriceInUSD = dto.CurrentPriceInUSD;

                listing.BusinessPrice = dto.BusinessPrice;

                //NOTE: Not updating auto sale price change info
                
                listing.RealQuantity = dto.RealQuantity;
                listing.DisplayQuantity = dto.DisplayQuantity;

                listing.RestockDate = dto.RestockDate;

                listing.AmazonCurrentPrice = dto.CurrentPrice;
                listing.AmazonRealQuantity = dto.RealQuantity;

                listing.LowestPrice = dto.LowestPrice;

                listing.UpdateDate = when;
                listing.IsRemoved = false;

                //Item can be changed as well
                listing.ItemId = item.Id;
            }

            unitOfWork.Commit();

            if (priceChangeType != PriceChangeSourceType.None)
            {
                unitOfWork.PriceHistories.Add(new PriceHistory
                {
                    ListingId = listing.Id,
                    SKU = listing.SKU,
                    Type = (int)priceChangeType,
                    Price = dto.CurrentPrice,
                    OldPrice = oldPrice,
                    ChangeDate = when,
                });
                unitOfWork.Commit();
            }

            return listing;
        }
    }
}
