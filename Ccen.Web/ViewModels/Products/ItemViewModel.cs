using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Amazon.Api;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Listings;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DTO;
using Amazon.DTO.Listings;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Feeds;

namespace Amazon.Web.ViewModels.Products
{
    public class ItemViewModel
    {
        public int ItemId { get; set; }

        public int? Market { get; set; }
        public string MarketplaceId { get; set; }

        public long? ListingEntityId { get; set; }
        public string ParentASIN { get; set; }
        public string SKU { get; set; }

        public string ListingType { get; set; }

        public bool IsFBA { get; set; }
        public decimal? EstimatedOrderHandlingFeePerOrder { get; set; }
        public decimal? EstimatedPickPackFeePerUnit { get; set; }
        public decimal? EstimatedWeightHandlingFeePerUnit { get; set; }

        public decimal? FBAFee
        {
            get { return EstimatedOrderHandlingFeePerOrder + EstimatedPickPackFeePerUnit + EstimatedWeightHandlingFeePerUnit; }
        }

        public string PictureUrl { get; set; }
        public string LargePictureUrl { get; set; }

        public int? DisplayQuantity { get; set; }
        public int RealQuantity { get; set; }
        
        public DateTime? AutoQuantityUpdateDate { get; set; }
        public string AutoQuantityUpdateDateAgoDateFormatted
        {
            get
            {
                return DateHelper.ConvertToReadableStringAgo(AutoQuantityUpdateDate, true);
            }
        }
        public bool OnHold { get; set; }
        public bool StyleOnHold { get; set; }
        public bool UseStyleImage { get; set; }
        public DateTime? RestockDate { get; set; }

        public int SoldByAmazon { get; set; }
        public int SoldByInventory { get; set; }
        public int SoldByFBA { get; set; }
        public int SoldBySpecialCase { get; set; }

        public int TotalSoldByAmazon { get; set; }
        public int TotalSoldByInventory { get; set; }
        public int TotalSoldByFBA { get; set; }
        public int TotalSoldBySpecialCase { get; set; }
        
        public int? TotalQuantity { get; set; }

        public int? RemainingQuantity { get; set; }

        public string Size { get; set; }
        public string ColorVariation { get; set; }

        public string StyleString { get; set; }
        public long? StyleId { get; set; }
        public long? StyleItemId { get; set; }
        public string StyleSize { get; set; }
        public string StyleColor { get; set; }
        public bool StyleItemOnHold { get; set; }

        public string Color { get; set; }
        public string ASIN { get; set; }
        public string Title { get; set; }


        public string Currency
        {
            get
            {
                return PriceHelper.GetCurrencySymbol((MarketType)(Market ?? (int)MarketType.Amazon), MarketplaceId);
            }
        }

        public decimal ItemCurrentPrice { get; set; }
        public string ItemCurrentPriceCurrency { get; set; }
        public decimal? BusinessPrice { get; set; }

        public bool IsPrime { get; set; }

        //Sale
        public decimal? SalePrice { get; set; }
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
        public int? MaxPiecesOnSale { get; set; }
        public int? PiecesSoldOnSale { get; set; }
        public long? SaleId { get; set; }
        

        public decimal? LowestPrice { get; set; }
        public BuyBoxStatusCode BuyBoxStatus { get; set; }


        public string ShippingPrice { get; set; }
        public string Weight { get; set; }
        [RegularExpression(@"^([0-9]*)$", ErrorMessage = "Invalid count")]
        public string PiecesInWarehouse { get; set; }

        [Display(Name = "UPC")]
        public string Barcode { get; set; }

        public int PublishedStatus { get; set; }
        public string PublishedStatusReason { get; set; }

        public string FormattedPublishedStatus
        {
            get { return PublishedStatusesHelper.GetName((PublishedStatuses)PublishedStatus); }
        }

        public bool? IsExistOnAmazon { get; set; }
        public bool? IsAmazonParentASIN { get; set; }
        public DateTime? OpenDate { get; set; }
        public int? AmazonRealQuantity { get; set; }
        public decimal? AmazonCurrentPrice { get; set; }

        public IList<ItemMarketInfoViewModel> RelatedMarkets { get; set; }

        public List<MessageString> ItemMessages { get; set; }

        public IList<ListingDefectViewModel> ListingDefects { get; set; }

        public bool HasItemMessages
        {
            get { return ItemMessages != null && ItemMessages.Any(); }
        }
        
        public bool HasListingDefects
        {
            get { return ListingDefects != null && ListingDefects.Any(); }
        }

        public float MarketIndex
        {
            get { return MarketHelper.GetMarketIndex((MarketType)(Market ?? (int)MarketType.None), MarketplaceId); }
        }

        public string MarketShortName
        {
            get { return MarketHelper.GetShortName(Market ?? (int)MarketType.None, MarketplaceId); }
        }

        public bool HasImage
        {
            get { return !String.IsNullOrEmpty(PictureUrl); }
        }

        public string Thumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(PictureUrl, 
                    75, 
                    75, 
                    false, 
                    ImageHelper.NO_IMAGE_URL,
                    convertInDomainUrlToThumbnail: true);
            }
        }

        public string LargeThumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(LargePictureUrl, 
                    75, 
                    75, 
                    false, 
                    ImageHelper.NO_IMAGE_URL,
                    convertInDomainUrlToThumbnail:true);
            }
        }


        public string StyleUrl
        {
            get
            {
                return UrlHelper.GetStyleUrl(StyleString);
            }
        }

        public float SizeIndex
        {
            get
            {
                return SizeHelper.GetSizeIndex(Size);
            }
        }

        public string SourceMarketId { get; set; }
        public string MarketUrl
        {
            get
            {
                return UrlHelper.GetMarketUrl(ASIN, SourceMarketId, (MarketType)Market, MarketplaceId);
            }
        }

        public string SellerMarketUrl
        {
            get
            {
                if (Market == (int)MarketType.Magento 
                    || Market == (int)MarketType.Walmart
                    || Market == (int)MarketType.WalmartCA
                    || Market == (int)MarketType.Jet 
                    || Market == (int)MarketType.eBay)
                    return UrlHelper.GetSellarCentralInventoryUrl(SourceMarketId, (MarketType)Market, MarketplaceId);
                return UrlHelper.GetSellarCentralInventoryUrl(ASIN, (MarketType)Market, MarketplaceId);
            }
        }
        
        public override string ToString()
        {
            return "ItemId=" + ItemId 
                + ", ListingId=" + ListingEntityId 
                + ", ParentASIN=" + ParentASIN 
                + ", Barcode=" + Barcode 
                + ", DisplayQuantity=" + DisplayQuantity 
                + ", RealQuantity=" + RealQuantity 
                + ", TotalQuantity=" + TotalQuantity

                + ", OnHold=" + OnHold
                + ", UseStyleImage=" + UseStyleImage
                + ", RestockDate=" + RestockDate

                + ", SalePrice=" + SalePrice
                + ", SaleStartDate=" + SaleStartDate
                + ", SaleEndDate=" + SaleEndDate
                + ", MaxPiecesOnSale=" + MaxPiecesOnSale
                
                + ", StyleId=" + StyleId
                + ", StyleItemId=" + StyleItemId
                
                + ", PublishedStatus=" + PublishedStatus;
        }
        
        public ItemViewModel()
        {
        }

        public ItemViewModel(ItemDTO item)
        {
            ItemId = item.Id;
            Market = item.Market;
            MarketplaceId = item.MarketplaceId;
            IsPrime = item.IsPrime;

            if (item.PublishedStatus != item.PublishedStatusFromMarket
                && item.PublishedStatusFromMarket.HasValue
                && !PublishedStatusesHelper.InProgressStatus((PublishedStatuses)item.PublishedStatus))
            {
                PublishedStatus = item.PublishedStatusFromMarket.Value;
            }
            else
            {
                PublishedStatus = item.PublishedStatus;
            }

            //NOTE: need to review (in fact need to hide errors listings in out-of-stock)
            if (PublishedStatus == (int)PublishedStatuses.PublishingErrors
                && item.RealQuantity == 0)
            {
                PublishedStatus = (int)PublishedStatuses.PublishedInactive;
            }

            PublishedStatusReason = item.PublishedStatusReason;

            ListingEntityId = item.ListingEntityId;
            ParentASIN = item.ParentASIN;
            SKU = item.SKU;

            SourceMarketId = item.SourceMarketId;

            PictureUrl = item.ImageUrl;
            LargePictureUrl = item.LargeImageUrl;

            ListingType = AmazonTemplateHelper.GetListingType(item);

            IsFBA = item.IsFBA;
            EstimatedOrderHandlingFeePerOrder = item.EstimatedOrderHandlingFeePerOrder;
            EstimatedPickPackFeePerUnit = item.EstimatedPickPackFeePerUnit;
            EstimatedWeightHandlingFeePerUnit = item.EstimatedWeightHandlingFeePerUnit;

            StyleId = item.StyleId;
            StyleItemId = item.StyleItemId;
            StyleString = item.StyleString;
            StyleOnHold = item.StyleOnHold ?? false;
            ASIN = item.ASIN;

            Size = item.Size;
            ColorVariation = item.ColorVariation;

            StyleSize = item.StyleSize;
            StyleColor = item.StyleColor;
            StyleItemOnHold = item.StyleItemOnHold ?? false;

            Color = item.Color;
            DisplayQuantity = item.DisplayQuantity;
            RealQuantity = item.RealQuantity;

            AutoQuantityUpdateDate = item.AutoQuantityUpdateDate;
            OnHold = item.OnHold;
            UseStyleImage = item.UseStyleImage;
            RestockDate = item.RestockDate;

            ItemCurrentPrice = item.CurrentPrice;
            ItemCurrentPriceCurrency = PriceHelper.GetCurrencySymbol((MarketType)item.Market, item.MarketplaceId);
            BusinessPrice = item.BusinessPrice;

            //Sale
            SalePrice = item.SalePrice;
            SaleStartDate = item.SaleStartDate;
            SaleEndDate = item.SaleEndDate;
            MaxPiecesOnSale = item.MaxPiecesOnSale;
            PiecesSoldOnSale = item.PiecesSoldOnSale;
            SaleId = item.SaleId;
            
            Weight = item.Weight != null ? item.Weight.ToString() : "";
            Barcode = item.Barcode;

            LowestPrice = item.LowestPrice;
            BuyBoxStatus = item.BuyBoxStatus ?? BuyBoxStatusCode.None;

            TotalQuantity = item.TotalQuantity;

            SoldByAmazon = item.SoldByAmazon ?? 0;
            SoldByInventory = item.SoldByInventory ?? 0;
            SoldByFBA = item.SoldByFBA ?? 0;
            SoldBySpecialCase = item.SoldBySpecialCase ?? 0;

            TotalSoldByAmazon = item.TotalSoldByAmazon ?? 0;
            TotalSoldByInventory = item.TotalSoldByInventory ?? 0;
            TotalSoldByFBA = item.TotalSoldByFBA ?? 0;
            TotalSoldBySpecialCase = item.TotalSoldBySpecialCase ?? 0;

            RemainingQuantity = item.RemainingQuantity.HasValue ? Math.Max(item.RemainingQuantity.Value, 0) : (int?)null;
            
            IsAmazonParentASIN = item.IsAmazonParentASIN;
            IsExistOnAmazon = item.IsExistOnAmazon;
            OpenDate = item.OpenDate ?? item.CreateDate;
            AmazonRealQuantity = item.AmazonRealQuantity;
            AmazonCurrentPrice = item.AmazonCurrentPrice;
        }

        public static FeedViewModel GetItemFeedInfo(IUnitOfWork db, long itemId)
        {
            var feedDto = (from fi in db.FeedItems.GetAllAsDto()
                          join f in db.Feeds.GetAllAsDto() on fi.FeedId equals f.Id
                          where fi.ItemId == itemId
                          orderby f.SubmitDate descending
                          select f).FirstOrDefault();
            if (feedDto == null)
                return null;

            var model = new FeedViewModel(feedDto);
            return model;
        }

        public static IList<FeedMessageViewModel> GetItemErrors(IUnitOfWork db, long itemId)
        {
            var results = new List<FeedMessageViewModel>();

            var item = db.Items.GetAll().FirstOrDefault(i => i.Id == itemId);
            var listing = db.Listings.GetAll().FirstOrDefault(l => l.ItemId == itemId && !l.IsRemoved);
            
            //Fill Item Messages
            var itemMessagesQuery = db.ItemAdditions.GetAllAsDTO().Where(i => i.ItemId == itemId
                && (i.Field == ItemAdditionFields.PublishError
                    || i.Field == ItemAdditionFields.PrePublishError
                    || i.Field == ItemAdditionFields.UpdateImageError)
                    //|| i.Field == ItemAdditionFields.UpdateRelationshipError
                    ).ToList();
            var itemMessages = itemMessagesQuery.ToList();

            results.AddRange(itemMessages
                .Select(d => new FeedMessageViewModel()
                {
                    FeedId = StringHelper.TryGetInt(d.Source),
                    Message = d.Value,                    
                    Status = MessageStatus.Error,
                    CreateDate = d.CreateDate
                })
                .ToList());

            if (!String.IsNullOrEmpty(item.ItemPublishedStatusReason)
                && item.ItemPublishedStatus != (int)PublishedStatuses.Published)
            {
                results.Add(new FeedMessageViewModel()
                {
                    Message = item.ItemPublishedStatusReason,
                    Status = MessageStatus.Error,
                    CreateDate = item.ItemPublishedStatusDate
                });
            }

            //Defects
            //Update Listing Defects
            if (listing != null)
            {
                var listingsDefectsQuery = db.ListingDefects.GetAllAsDto().Where(d => d.SKU == listing.SKU
                    && d.MarketType == (int)listing.Market
                    && d.MarketplaceId == listing.MarketplaceId);
                var listingDefects = listingsDefectsQuery.ToList();

                results.AddRange(listingDefects
                    .Select(d => new FeedMessageViewModel()
                    {
                        Message = d.FieldName + " - " + d.AlertType + " - " + d.Explanation,
                        Status = d.AlertName == "Suppressed" ? MessageStatus.Error : MessageStatus.Warning,
                        CreateDate = d.LastUpdated
                    })
                    .ToList());
            }

            return results;
        }
        
        


        /// <summary>
        /// Get items for grid
        /// </summary>
        /// <param name="db"></param>
        /// <param name="parentASIN"></param>
        /// <returns></returns>
        public static IEnumerable<ItemViewModel> GetAll(IUnitOfWork db, 
            MarketType market,
            string marketplaceId,
            string parentASIN, 
            ListingsModeType listingMode)
        {
            var query = db.Items.GetAllActualWithSold()
                .Where(i => i.ParentASIN == parentASIN);
 
            if (market != MarketType.None)
                query = query.Where(i => i.Market == (int)market);

            if (!String.IsNullOrEmpty(marketplaceId))
                query = query.Where(i => i.MarketplaceId == marketplaceId);

            switch (listingMode)
            {
                case ListingsModeType.OnlyFBA:
                    query = query.Where(l => l.IsFBA);
                    break;
                case ListingsModeType.WithoutFBA:
                    query = query.Where(l => !l.IsFBA);
                    break;
            }
            
            var items = query
                .ToList()
                .Select(i => new ItemViewModel(i))
                .ToList();

            var skuList = items.Select(i => i.SKU);
            var itemIds = items.Select(i => i.ItemId);

            //Fill Item Messages
            var itemMessagesQuery = db.ItemAdditions.GetAllAsDTO().Where(i => itemIds.Contains(i.ItemId)
                && (i.Field == ItemAdditionFields.PublishError
                    || i.Field == ItemAdditionFields.PrePublishError
                    || i.Field == ItemAdditionFields.UpdateImageError
                    //|| i.Field == ItemAdditionFields.UpdateRelationshipError
                    )).ToList();
            var itemMessages = itemMessagesQuery.ToList();

            foreach (var item in items)
            {
                if (item.RealQuantity > 0)
                {
                    item.ItemMessages = itemMessages
                        .Where(m => m.ItemId == item.ItemId)
                        .Select(d => new MessageString()
                        {
                            //Message = d.Value,
                            Status = MessageStatus.Error
                        })
                        .ToList();
                }
                else
                {
                    item.ItemMessages = new List<MessageString>();
                }


                if (!String.IsNullOrEmpty(item.PublishedStatusReason)
                    && item.PublishedStatus != (int)PublishedStatuses.Published)
                {
                    item.ItemMessages.Add(new MessageString()
                    {
                        //Message = item.PublishedStatusReason,
                        Status = MessageStatus.Error
                    });
                }
            }

            //Update Listing Defects
            var listingsDefectsQuery = db.ListingDefects.GetAllAsDto().Where(d => skuList.Contains(d.SKU)
                && d.MarketType == (int)market
                && d.MarketplaceId == marketplaceId);
            var listingDefects = listingsDefectsQuery.ToList();

            foreach (var item in items)
            {
                item.ItemMessages.AddRange(listingDefects
                    .Where(d => d.SKU == item.SKU)
                    .Select(d => new MessageString()
                    {
                        Status = d.AlertName == "Suppressed" ? MessageStatus.Error : MessageStatus.Warning
                    })
                    .ToList());
            }


            //Update Related Markets
            var allMarketItems = db.Items.GetAllViewActual()
                .Where(i => skuList.Contains(i.SKU))
                .Select(i => new ItemMarketInfoDTO()
                {
                    SKU = i.SKU,
                    ASIN = i.ASIN,
                    ParentASIN = i.ParentASIN,
                    Market = i.Market,
                    MarketplaceId = i.MarketplaceId
                }).ToList();

            foreach (var item in items)
            {
                item.RelatedMarkets = allMarketItems
                    .Where(i => i.SKU == item.SKU 
                        && !(i.MarketplaceId == item.MarketplaceId
                            && i.Market == item.Market))
                    .Select(i => new ItemMarketInfoViewModel(i))
                    .ToList()
                    .OrderBy(i => i.MarketIndex)
                    .ToList();
            }

            return items;
        }

        public void Update(IUnitOfWork db, 
            ILogService log,
            IPriceManager priceManager,
            IStyleHistoryService styleHistoryService,
            DateTime when,
            long? by)
        {
            log.Info("ItemViewModel.Update begin");
            var item = db.Items.GetAll().FirstOrDefault(i => i.Id == ItemId);
            var listing = db.Listings.GetFiltered(l => l.Id == ListingEntityId).FirstOrDefault();

            if (item != null)
            {
                if (listing != null && !listing.IsFBA)
                {
                    //NOTE: only when we have in DB empty Parent ASIN and came not empty update them. Then create Parent ASIN if not exist
                    if (String.IsNullOrEmpty(item.ParentASIN) && !String.IsNullOrEmpty(ParentASIN))
                    {
                        item.ParentASIN = ParentASIN;

                        db.ParentItems.FindOrCreateForItem(item, when);
                    }

                    log.Info("Item Before: " + item);
                    item.Barcode = Barcode;

                    var style = db.Styles.GetActiveByStyleIdAsDto(StyleString);
                    var newStyleId = style != null ? style.Id : (long?) null;
                    if (item.StyleId != newStyleId)
                    {
                        if (newStyleId.HasValue)
                        {
                            styleHistoryService.AddRecord(newStyleId.Value,
                                StyleHistoryHelper.AttachListingKey,
                                item.StyleItemId,
                                item.Market + ":" + item.MarketplaceId,
                                StyleItemId,
                                item.Id.ToString() + ":" + item.ASIN,
                                null);
                        }

                        if (item.StyleId.HasValue)
                        {
                            styleHistoryService.AddRecord(item.StyleId.Value,
                                StyleHistoryHelper.DetachListingKey,
                                item.StyleItemId,
                                item.Market + ":" + item.MarketplaceId,
                                StyleItemId,
                                item.Id.ToString(),
                                null);
                        }
                    }

                    item.StyleString = StyleString;
                    item.StyleId = newStyleId;
                    item.StyleItemId = StyleItemId;

                    item.UseStyleImage = UseStyleImage;

                    item.UpdateDate = when;
                    item.UpdatedBy = by;
                    log.Info("Item After: " + item);
                }
            }
            
            if (listing != null)
            {
                log.Info("Listing Before: " + listing);

                listing.IsPrime = IsPrime;

                if (listing.CurrentPrice != ItemCurrentPrice)
                {
                    var oldPrice = listing.CurrentPrice;

                    listing.PriceUpdateRequested = true;
                    listing.CurrentPrice = ItemCurrentPrice;
                    listing.CurrentPriceInUSD = PriceHelper.RougeConvertToUSD(listing.CurrentPriceCurrency, ItemCurrentPrice);                    

                    if (listing.IsFBA)
                    {
                        listing.AutoAdjustedPrice = null;
                    }

                    if (listing.BusinessPrice.HasValue)
                    {
                        listing.BusinessPrice = listing.CurrentPrice;
                    }

                    priceManager.LogListingPrice(db, 
                        PriceChangeSourceType.EnterNewPrice, 
                        listing.Id,
                        listing.SKU,
                        listing.CurrentPrice,
                        oldPrice,
                        when,
                        by);
                }
                               

                //NOTE: Always force send updates to Amazon
                listing.PriceUpdateRequested = true;

                if (!listing.IsFBA)
                {
                    if (DisplayQuantity != listing.DisplayQuantity)
                    {
                        listing.QuantityUpdateRequested = true;
                        listing.QuantityUpdateRequestedDate = when;
                        listing.DisplayQuantity = DisplayQuantity;
                    }

                    if (listing.OnHold != OnHold)
                    {
                        listing.QuantityUpdateRequested = true;
                        listing.QuantityUpdateRequestedDate = when;
                        listing.OnHold = OnHold;
                    }

                    //NOTE: Always force send updates to Amazon
                    listing.QuantityUpdateRequested = true;
                    listing.QuantityUpdateRequestedDate = when;
                }
                    
                listing.UpdateDate = when;
                listing.UpdatedBy = by;

                log.Info("Listing After: " + listing);
            }
            db.Commit();
            log.Info("ItemViewModel.Update end");
        }
    }
}