using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;
using Amazon.Model.Models;
using Amazon.Core.Contracts.Quantities;

namespace Amazon.Model.General.Services
{
    public enum DistributeMode
    {
        None = 0,
        //DropShipper = 1,
        //DropShipperMagento = 2,
        UsingOnHandQty = 10,
    }

    public enum MarketDistributeModes
    {
        Default = 0,
        DirectFromRemaining = 10,
    }

    public class MarketDistributeInfo
    {
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }
        public string LinkedToMarketplaceId { get; set; }
        public int MarketQty { get; set; }
        public List<ListingQuantityDTO> Listings { get; set; }
        public int Multiplier { get; set; }
        public bool BlockPreorderInventory { get; set; }
        /// <summary>
        /// Max qty allocated to listing
        /// </summary>
        public int MaxQuantity { get; set; }
        /// <summary>
        /// When qty less then low it will be redistributed, othewise keeped as is
        /// </summary>
        public int LowQuantity { get; set; }

        public bool SupportRestockDate { get; set; }
        //public List<long> ExcludeDS { get; set; }

        public MarketDistributeModes Mode { get; set; }

        public int DirectDistributionThreashold { get; set; }

        public IList<IQuantityDistributionRule> Rules { get; set; }
    }


    public class QuantityDistributionService
    {
        private IDbFactory _dbFactory;
        private IQuantityManager _quantityManager;
        private ILogService _log;
        private ITime _time;
        private IList<MarketDistributeInfo> _marketList;
        private DistributeMode _mode;

        public QuantityDistributionService(IDbFactory dbFactory,
            IQuantityManager quantityManager,
            ILogService log,
            ITime time,
            IList<MarketDistributeInfo> marketList,
            DistributeMode mode)
        {
            _dbFactory = dbFactory;
            _quantityManager = quantityManager;
            _log = log;
            _time = time;
            _marketList = marketList;
            _mode = mode;
        }

        public static int GetMarketMaxQty(int currentQty, MarketType market, string marketplaceId)
        {
            if (market == MarketType.eBay)
                return Math.Min(currentQty, 2);
            return currentQty;
        }

        public IList<ListingQuantityDTO> Redistribute(IUnitOfWork db)
        {
            _log.Info("Redistribute, mode=" + _mode);
            IList<ListingQuantityDTO> listings = new List<ListingQuantityDTO>();

            listings = db.Listings.GetAllAsQuantityDto()
                .Where(l => l.IsRemoved == false
                            && !l.IsFBA)
                .ToList();

            var styles = db.Styles.GetAllAsDtoLite().Where(s => !s.Deleted).ToList();
            var styleItems = db.StyleItems.GetAllAsDto().ToList();
            var styleReferences = db.StyleReferences.GetAllAsDto().ToList();
            var styleItemReferences = db.StyleItemReferences.GetAllAsDto().ToList();

            var marketsQuantity = db.Items.GetMarketsSoldQuantityIncludePreOrderByStyleItem().ToList();
            var sentInStoreQuantity = db.Scanned.GetSentInStoreQuantities().ToList();
            var sentToFBAQuantity = db.Scanned.GetSentToFBAQuantities().ToList();
            var specialCaseQuantity = db.QuantityOperations.GetSpecialCaseQuantities().ToList();
            var inventoryQuantity = _mode == DistributeMode.UsingOnHandQty ? db.StyleItems.GetInventoryOnHandQuantities().ToList() 
                : db.StyleItems.GetInventoryQuantities().ToList();
            var saleEventQuantity = db.SaleEventSizeHoldInfoes.GetSoldQuantitiesByStyleItem().ToList();
            saleEventQuantity.AddRange(db.SaleEventSizeHoldInfoes.GetHoldedQuantitiesByStyleItem().ToList());

            var sentToPhotoshootQuantity = RetryHelper.TryOrDefault(() => db.PhotoshootPickListEntries.GetHoldedQuantitiesByStyleItem().ToList(),
                new List<SoldSizeInfo>());
            
            return RedistributeForStylesInner(db, 
                _log,
                _time,
                styles,
                styleReferences,
                styleItems,
                styleItemReferences,
                listings,
                marketsQuantity,
                sentInStoreQuantity,
                sentToFBAQuantity,
                specialCaseQuantity,
                saleEventQuantity,
                sentToPhotoshootQuantity,
                inventoryQuantity);
        }

        public IList<ListingQuantityDTO> RedistributeForStyle(IUnitOfWork db,
            long styleId)
        {
            return RedistributeForStyle(db, new List<long>() {styleId});
        }

        public IList<ListingQuantityDTO> RedistributeForStyle(IUnitOfWork db,
            IList<long> styleIds)
        {
            var listings = db.Listings.GetAllAsQuantityDto()
                .Where(l => l.StyleId.HasValue
                    && styleIds.Contains(l.StyleId.Value)
                    && l.IsRemoved == false
                    && !l.IsFBA)
                .ToList();

            var styleReferences = db.StyleReferences.GetAllAsDto().Where(sr => styleIds.Contains(sr.StyleId)).ToList();

            var styleIdList = styleIds.ToList();
            styleIdList.AddRange(styleReferences.Select(s => s.LinkedStyleId).ToList());

            var styles = db.Styles.GetAllAsDtoLite().Where(s => styleIdList.Contains(s.Id) && !s.Deleted).ToList();
            var styleItems = db.StyleItems.GetAllAsDto().Where(si => styleIdList.Contains(si.StyleId)).ToList();

            var styleItemReferences = db.StyleItemReferences.GetAllAsDto().Where(si => styleIdList.Contains(si.StyleId)).ToList();

            var marketsQuantity = db.Items.GetMarketsSoldQuantityIncludePreOrderByStyleItem()
                .Where(s => s.StyleId.HasValue && styleIdList.Contains(s.StyleId.Value))
                .ToList();
            var sentInStoreQuantity = db.Scanned.GetSentInStoreQuantities()
                .Where(s => s.StyleId.HasValue && styleIdList.Contains(s.StyleId.Value))
                .ToList();
            var sentToFBAQuantity = db.Scanned.GetSentToFBAQuantities()
                .Where(s => s.StyleId.HasValue && styleIdList.Contains(s.StyleId.Value))
                .ToList();
            var specialCaseQuantity = db.QuantityOperations.GetSpecialCaseQuantities()
                .Where(s => s.StyleId.HasValue && styleIdList.Contains(s.StyleId.Value))
                .ToList();
            var saleEventQuantity = db.SaleEventSizeHoldInfoes.GetSoldQuantitiesByStyleItem()
                .Where(s => s.StyleId.HasValue && styleIdList.Contains(s.StyleId.Value))
                .ToList();
            saleEventQuantity.AddRange(db.SaleEventSizeHoldInfoes.GetHoldedQuantitiesByStyleItem()
                .Where(s => s.StyleId.HasValue && styleIdList.Contains(s.StyleId.Value))
                .ToList());

            var sentToPhotoshootQuantity = RetryHelper.TryOrDefault(() => db.PhotoshootPickListEntries.GetHoldedQuantitiesByStyleItem()
                .Where(s => s.StyleId.HasValue && styleIdList.Contains(s.StyleId.Value))
                .ToList(), new List<SoldSizeInfo>());

            var inventoryQuantity = _mode == DistributeMode.UsingOnHandQty ?
                db.StyleItems.GetInventoryOnHandQuantities()
                .Where(s => s.StyleId.HasValue && styleIdList.Contains(s.StyleId.Value))
                .ToList() :
                db.StyleItems.GetInventoryQuantities()
                .Where(s => s.StyleId.HasValue && styleIdList.Contains(s.StyleId.Value))
                .ToList();

            return RedistributeForStylesInner(db,
                _log,
                _time,
                styles,
                styleReferences,
                styleItems,
                styleItemReferences,
                listings,
                marketsQuantity,
                sentInStoreQuantity,
                sentToFBAQuantity,
                specialCaseQuantity,
                saleEventQuantity,
                sentToPhotoshootQuantity,
                inventoryQuantity);
        }

        private IList<ListingQuantityDTO> RedistributeForStylesInner(IUnitOfWork db, 
            ILogService log, 
            ITime time,
            IList<StyleEntireDto> styles,
            IList<StyleReferenceDTO> styleReferences,
            IList<StyleItemDTO> styleItems,
            IList<StyleItemReferenceDTO> styleItemReferences,
            IList<ListingQuantityDTO> listings,
            IList<SoldSizeInfo> marketsQuantity,
            IList<SoldSizeInfo> sentInStoreQuantity,
            IList<SoldSizeInfo> sentToFBAQuantity,
            IList<SoldSizeInfo> specialCaseQuantity,
            IList<SoldSizeInfo> saleEventQuantity,
            IList<SoldSizeInfo> sentToPhotoshootQuantity,
            IList<SoldSizeInfo> inventoryQuantity)
        {
            log.Info("begin Redistribute");
            var watch = Stopwatch.StartNew();
            
            var index = 0;

            CalculateRemainingQuantities(styles, 
                styleItems,
                marketsQuantity,
                sentInStoreQuantity,
                sentToFBAQuantity,
                specialCaseQuantity,
                saleEventQuantity,
                sentToPhotoshootQuantity,
                inventoryQuantity);
            
            PrepareOnHoldForListings(styles,
                styleItems,
                listings);

            PrepareReferenceStyleItemsRemaingQuantities(db,
               log,
               styles,
               styleReferences,
               styleItemReferences,
               styleItems,
               listings,
               time.GetAppNowTime());
            

            //STEP 1.1 Redestribute qty (each market has self part of global remaining qty). 
            //UPDATE ONLY US + UK MARKETS
            foreach (var style in styles)
            {
                var currentStyleItems = styleItems.Where(si => si.StyleId == style.Id).ToList();
                foreach (var styleItem in currentStyleItems)
                {
                    var quantityRemaining = styleItem.RemainingQuantity ?? 0;
                    if (quantityRemaining < 0)
                        quantityRemaining = 0;
                    
                    foreach (var marketInfo in _marketList)
                    {
                        var queryByStyle = listings
                            .Where(l => l.StyleId == style.Id
                                        && l.StyleItemId == styleItem.StyleItemId
                                        && !l.OnHold
                                        && l.ItemPublishedStatus != (int)PublishedStatuses.Unpublished);

                        var marketQuery = queryByStyle;
                        
                        if (!String.IsNullOrEmpty(marketInfo.LinkedToMarketplaceId))
                        {
                            var linkedMarketplaceSKUList = new List<string>();
                            linkedMarketplaceSKUList = queryByStyle
                                .Where(l => l.MarketplaceId == marketInfo.LinkedToMarketplaceId)
                                .Select(l => l.SKU)
                                .ToList();

                            marketQuery = marketQuery.Where(l => !linkedMarketplaceSKUList.Contains(l.SKU));
                        }

                        if (String.IsNullOrEmpty(marketInfo.MarketplaceId))
                            marketQuery = marketQuery.Where(l => l.Market == (int) marketInfo.Market);
                        else
                            marketQuery = marketQuery.Where(l => l.Market == (int) marketInfo.Market 
                                && l.MarketplaceId == marketInfo.MarketplaceId);
                        
                        marketInfo.Listings = marketQuery.OrderBy(l => l.SKU).ToList();
                    }
                    
                    var notDistributedListingsCount = _marketList.Where(m => m.Mode != MarketDistributeModes.DirectFromRemaining).Sum(m => m.Listings.Count*m.Multiplier);
                    var leftQty = quantityRemaining;
                    if (notDistributedListingsCount > 0)
                    {
                        for (int i = 0; i < _marketList.Count; i++)
                        {
                            var marketInfo = _marketList[i];

                            if (marketInfo.Mode == MarketDistributeModes.DirectFromRemaining)
                            {
                                marketInfo.MarketQty = Math.Max(0, quantityRemaining - marketInfo.DirectDistributionThreashold);
                            }

                            if (marketInfo.Mode == MarketDistributeModes.Default)
                            {
                                var marketListingsCount = marketInfo.Listings.Count * marketInfo.Multiplier;

                                if (i == _marketList.Count - 1)
                                {
                                    //NOTE: for last one market set all left qty
                                    marketInfo.MarketQty = leftQty;
                                }
                                else
                                {
                                    //NOTE: always get max when rounding value, but not greater then available
                                    if (notDistributedListingsCount != 0)
                                        marketInfo.MarketQty = (int)Math.Min(leftQty, Math.Ceiling(leftQty * marketListingsCount / (float)(notDistributedListingsCount)));
                                    else
                                        marketInfo.MarketQty = 0;
                                }

                                //TODO: move to rules
                                //NOTE: when we have Restock quantity reset market quantity for not support restock market
                                if (styleItem.RestockDate.HasValue 
                                    && styleItem.RestockDate > time.GetAppNowTime()
                                    && !marketInfo.SupportRestockDate)
                                {
                                    marketInfo.MarketQty = 0;
                                }

                                notDistributedListingsCount -= marketListingsCount;
                            }

                            if (marketInfo.Rules != null)
                            {
                                foreach (var rule in marketInfo.Rules)
                                {
                                    marketInfo.MarketQty = rule.Apply(style,
                                        marketInfo.Market,
                                        marketInfo.MarketplaceId,
                                        marketInfo.Listings,
                                        quantityRemaining,
                                        marketInfo.MarketQty);
                                }
                            }

                            leftQty -= marketInfo.MarketQty;
                        }
                    }

                    //Correction qty for borderline cases (move all qty to US market / main market)
                    //if (_marketList[0].MarketQty <= _marketList[0].Listings.Count
                    //    && _marketList[0].Listings != null
                    //    && _marketList[0].Listings.Any())
                    //{
                    //    _marketList[0].MarketQty = quantityRemaining;
                    //    for (int i = 1; i < _marketList.Count; i++)
                    //    {
                    //        _marketList[i].MarketQty = 0;
                    //    }
                    //}
                    
                    //TODO: =leftQty, need to complex testing
                    var notUsedQuantity = 0;

                    foreach (var marketInfo in _marketList)
                    {
                        if (marketInfo.Mode == MarketDistributeModes.DirectFromRemaining)
                        {
                            if (marketInfo.Listings.Any())
                            {
                                ReDistributeQuantityBetweenStyleListings(marketInfo.Listings,
                                    marketInfo.MarketQty,
                                    Int32.MaxValue,
                                    Int32.MaxValue);
                            }
                        }

                        if (marketInfo.Mode == MarketDistributeModes.Default)
                        {
                            if (marketInfo.Listings.Any())
                            {
                                var toRedistribute = marketInfo.MarketQty;
                                if (marketInfo.MarketQty > 0) //NOTE: prevent to set qty after rules restriction
                                    toRedistribute += notUsedQuantity;

                                ReDistributeQuantityBetweenStyleListings(marketInfo.Listings,
                                    toRedistribute,
                                    marketInfo.MaxQuantity,
                                    marketInfo.LowQuantity);
                            }
                            notUsedQuantity = marketInfo.MarketQty + notUsedQuantity - marketInfo.Listings.Sum(l => l.RealQuantity);
                        }
                    }
                }
                index++;
            }

            //STEP 1.1 Update restock date, TO ALL LISTINGS (include UK, may exclude: eBay, CA)
            //NOTE: NOW NO LINKS BY SKU (each market independ destirubte qty). For now no need: "Should copy RestockDate to linked by SKU listings (to keep integrity) 
            //(by default all linked SKU have the same StyleId)"
            var allListings = listings;
            foreach (var style in styles)
            {
                var currentStyleItems = styleItems.Where(si => si.StyleId == style.Id).ToList();
                foreach (var styleItem in currentStyleItems)
                {
                    var listingsToUpdate = allListings
                        .Where(l => l.StyleId == style.Id
                            && l.StyleItemId == styleItem.StyleItemId)
                        .ToList();
                    UpdateRestockDate(listingsToUpdate, styleItem.RestockDate);
                }
            }

            //STEP 1.2 On hold listings. TO ALL LISTINGS (When distirubte we should use only not onHold, otherwise we spend qty to death listings).
            //(onHold independed, each market distibute quantity independed (no linking by SKU))
            var onHoldListings = listings.Where(l => l.OnHold).ToList();
            ResetOnHoldListings(onHoldListings);
            //NOTE: Added only listings there are not in change list (changes to other listing reflected to that list, used the same object)

            //STEP 1.3 Unpublish listings. TO ALL UNPUBLISH 
            var unpublishListings = listings.Where(l => l.ItemPublishedStatus == (int)PublishedStatuses.Unpublished).ToList();
            ResetUnpublishedListings(unpublishListings);

            //SETP 1.4 Reset qty to unlinked listings (w/o linked style)
            var listingsWoStyles = listings.Where(l => !l.StyleItemId.HasValue && l.RealQuantity > 0).ToList();
            ResetUnlinkedListings(listingsWoStyles);

            //SETP 1.5 Copy changes from US to CA and MX listings, include onHold changes
            var usListings = listings.Where(l => l.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId).ToList();

            var caListings = listings.Where(l => l.MarketplaceId == MarketplaceKeeper.AmazonCaMarketplaceId).ToList();
            CopyToLinkedListings(usListings, caListings);

            var mxListings = listings.Where(l => l.MarketplaceId == MarketplaceKeeper.AmazonMxMarketplaceId).ToList();
            CopyToLinkedListings(usListings, mxListings);
            
            //STEP 2. UPDATE LISTINGS INTO DB
            var changedListings = allListings.Where(r => r.DirtyStatus != (int)DirtyStatusEnum.None).ToList();
            if (changedListings.Any())
            {
                log.Info("before db Update, duration=" + watch.Elapsed.TotalMilliseconds + " ms");

                var listingIds = changedListings.Select(l => l.Id).ToList();
                var listingsToUpdate = db.Listings
                    .GetFiltered(l => listingIds.Contains(l.Id))
                    .OrderBy(l => l.SKU)
                    .ThenBy(l => l.Market)
                    .ThenBy(l => l.MarketplaceId) //NOTE: order listings for relieve logs reading
                    .ToList();

                foreach (var listing in listingsToUpdate)
                {
                    var changeListing = changedListings.FirstOrDefault(l => l.Id == listing.Id);

                    if (changeListing != null)
                    {
                        log.Info("listing, id=" + listing.Id 
                            + ", market=" + listing.Market
                            + ", marketplaceId=" + listing.MarketplaceId
                            + ", sku=" + listing.SKU
                            + ", styleId=" + changeListing.StyleId
                            + " (" + changeListing.Size + "), realQuantity=" + changeListing.RealQuantity + " (from=" + listing.RealQuantity + ")"
                            + ", restockDate=" + changeListing.RestockDate + " (from=" + listing.RestockDate + ")"
                            + ", onHold=" + changeListing.OnHold + " (from=" + listing.OnHold + ")");

                        changeListing.OldQuantity = listing.RealQuantity;

                        _quantityManager.UpdateListingQuantity(db,
                            QuantityChangeSourceType.SetByAutoQuantity,
                            listing,
                            changeListing.RealQuantity,
                            changeListing.Size,
                            null,
                            null,
                            null,
                            time.GetAppNowTime(),
                            null);

                        //NOTE: forbid updates, we can't change status of listings, RealQuantity for that listings already set to 0
                        //listing.OnHold = changeListing.OnHold;
                        listing.RestockDate = changeListing.RestockDate;

                        if (!changeListing.IsLinked)
                        {
                            listing.QuantityUpdateRequested = true;
                            listing.QuantityUpdateRequestedDate = time.GetAppNowTime();
                        }
                    }
                    else
                    {
                        log.Warn("listing was disappeared, id=" + listing.Id);
                    }
                }

                log.Info("before Commit, duration=" + watch.Elapsed.TotalMilliseconds + " ms");

                db.Commit();
            }
            
            watch.Stop();
            log.Info("end Redistribute, duration=" + watch.Elapsed.TotalMilliseconds + " ms");

            return changedListings;
        }

        private void CalculateRemainingQuantities(IList<StyleEntireDto> allStyles, 
            IList<StyleItemDTO> allStyleItems,
            IList<SoldSizeInfo> marketsQuantity,
            IList<SoldSizeInfo> sentInStoreQuantity,
            IList<SoldSizeInfo> sentToFBAQuantity,
            IList<SoldSizeInfo> specialCaseQuantity,
            IList<SoldSizeInfo> saleEventQuantity,
            IList<SoldSizeInfo> sentToPhotoshootQuantity,
            IList<SoldSizeInfo> inventoryQuantity)
        {
            foreach (var style in allStyles)
            {
                var currentStyleItems = allStyleItems.Where(si => si.StyleId == style.Id).ToList();
                foreach (var styleItem in currentStyleItems)
                {
                    //CALCULATE REMAINING QUANTITY 
                    //NOTE: Indeped of QuantityMode, view takes into account it
                    var invQty = inventoryQuantity.Where(i => i.StyleItemId == styleItem.StyleItemId).Sum(i => i.TotalQuantity ?? 0); //NOTE: equal FirstOrDefault
                    var marketSoldQty = marketsQuantity.Where(i => i.StyleItemId == styleItem.StyleItemId).Sum(i => i.SoldQuantity ?? 0); //NOTE: may have multiple listings per market + multiple markets
                    var sentInStoreQty = sentInStoreQuantity.Where(i => i.StyleItemId == styleItem.StyleItemId).Sum(i => i.SoldQuantity ?? 0); //NOTE: equal FirstOrDefault
                    var sentToFBAQty = sentToFBAQuantity.Where(i => i.StyleItemId == styleItem.StyleItemId).Sum(i => i.SoldQuantity ?? 0); //NOTE: equal FirstOrDefault
                    var specialCaseQty = specialCaseQuantity.Where(i => i.StyleItemId == styleItem.StyleItemId).Sum(i => i.SoldQuantity ?? 0);
                    var saleEventQty = saleEventQuantity.Where(i => i.StyleItemId == styleItem.StyleItemId).Sum(i => i.SoldQuantity ?? 0);
                    var sentToPhotoshootQty = sentToPhotoshootQuantity.Where(i => i.StyleItemId == styleItem.StyleItemId).Sum(i => i.SoldQuantity ?? 0);

                    styleItem.RemainingQuantity = invQty 
                        - marketSoldQty 
                        - sentInStoreQty 
                        - sentToFBAQty 
                        - specialCaseQty 
                        - saleEventQty 
                        - sentToPhotoshootQty;

                    //NOTE: [Fix] sometimes quantity can be negative
                    if (styleItem.RemainingQuantity < 0)
                        styleItem.RemainingQuantity = 0;
                }
            }
        }
        
        private void PrepareOnHoldForListings(IList<StyleEntireDto> styles,
            IList<StyleItemDTO> styleItems,
            IList<ListingQuantityDTO> listings)
        {
            foreach (var style in styles)
            {
                if (style.OnHold || style.SystemOnHold)
                {
                    var styleListings = listings.Where(l => l.StyleId == style.Id).ToList();
                    foreach (var listing in styleListings)
                    {
                        listing.OnHold = true;
                    }
                }
            }

            foreach (var styleItem in styleItems)
            {
                if (styleItem.OnHold)
                {
                    var styleItemListings = listings.Where(l => l.StyleItemId == styleItem.StyleItemId).ToList();
                    foreach (var listing in styleItemListings)
                    {
                        listing.OnHold = true;
                    }
                }
            }

            foreach (var listing in listings)
            {
                if (listing.OnHoldParent == true)
                {
                    listing.OnHold = true;
                }
            }
        }

        private void PrepareReferenceStyleItemsRemaingQuantities(IUnitOfWork db,
            ILogService log,
            IList<StyleEntireDto> allStyles,
            IList<StyleReferenceDTO> allStyleReferences,
            IList<StyleItemReferenceDTO> allStyleItemReferences,
            IList<StyleItemDTO> allStyleItems,
            IList<ListingQuantityDTO> listings,
            DateTime when)
        {
            var changedRefStyleItems = new List<StyleItemDTO>();

            var refAllStyles = allStyles.Where(s => s.Type == (int)StyleTypes.References).ToList();

            foreach (var refStyle in refAllStyles)
            {
                var refStyleItems = allStyleItems.Where(si => si.StyleId == refStyle.Id).ToList();
                //var styleReferences = allStyleReferences.Where(sr => sr.StyleId == refStyle.Id).ToList();
                foreach (var refStyleItem in refStyleItems)
                {
                    IList<StyleItemDTO> toSubstractRemaining = new List<StyleItemDTO>();

                    int? minQuantity = null;
                    //Get min available remaining quantity
                    var linkedStyleItemRefs = allStyleItemReferences.Where(si => si.StyleItemId == refStyleItem.StyleItemId).ToList();
                    
                    foreach (var linkedStyleItemRef in linkedStyleItemRefs)
                    {
                        var linkedStyleItemListingCount = listings.Where(l => l.StyleItemId == linkedStyleItemRef.LinkedStyleItemId).Count();

                        //NOTE: total links to style item
                        var totalRefCountToRealSi = allStyleItemReferences.Count(si => si.LinkedStyleItemId == linkedStyleItemRef.LinkedStyleItemId);
                        var totalRefCountIntoThisStyle = linkedStyleItemRefs.Count(si => si.LinkedStyleItemId == linkedStyleItemRef.LinkedStyleItemId);
                        var linkedStyleItem = allStyleItems.FirstOrDefault(si => si.StyleItemId == linkedStyleItemRef.LinkedStyleItemId);

                        if (linkedStyleItem == null)
                        {
                            minQuantity = 0;
                        }
                        else
                        {
                            decimal multiplier = (totalRefCountToRealSi + 1);

                            //NOTE: in that case no one sell this item
                            if (linkedStyleItemListingCount == 0
                                && linkedStyleItem.RemainingQuantity <= totalRefCountToRealSi)
                            {
                                toSubstractRemaining.Add(linkedStyleItem);
                                //NOTE: when we have multiple reference to one styleItem
                                multiplier = allStyleItemReferences.Count(si => si.LinkedStyleItemId == linkedStyleItemRef.LinkedStyleItemId
                                    && si.StyleId == refStyleItem.StyleId);

                                //NOTE: set multiplier equal the Remaining to get always a result min qty = 1
                                multiplier = Math.Max(multiplier, linkedStyleItem.RemainingQuantity ?? 0);
                            }

                            if (linkedStyleItem.RemainingQuantity > 10 && linkedStyleItem.RemainingQuantity <= 20)
                                multiplier = (totalRefCountToRealSi + 1) / 2;
                            if (linkedStyleItem.RemainingQuantity > 20 && linkedStyleItem.RemainingQuantity <= 30)
                                multiplier = 2;
                            if (linkedStyleItem.RemainingQuantity > 30 && linkedStyleItem.RemainingQuantity < 50)
                                multiplier = 1.5M;
                            if (linkedStyleItem.RemainingQuantity > 30 && linkedStyleItem.RemainingQuantity <= 50)
                                multiplier = 1.25M;
                            if (linkedStyleItem.RemainingQuantity > 50)
                                multiplier = 1;

                            multiplier = Math.Max(multiplier, totalRefCountIntoThisStyle);

                            minQuantity = Math.Min((int)Math.Floor((linkedStyleItem.RemainingQuantity ?? 0) / multiplier), minQuantity ?? Int32.MaxValue);
                        }
                    }
                    
                    log.Debug("Get quantity for ref styleItemId=" + refStyleItem.StyleItemId
                              + ", styleId=" + refStyleItem.StyleId
                              + ", remainingQuantity=" + refStyleItem.RemainingQuantity);


                    //var pieceCount = refStyleItems.Count(i => i.StyleId == refStyleItem.StyleId);
                    //var receiveMultiplier = pieceCount == 0 ? 0 : 1 / (decimal)(pieceCount + 1);
                    refStyleItem.RemainingQuantity = minQuantity ?? 0;// (int)Math.Round(receiveMultiplier * (minQuantity ?? 0));

                    if (refStyleItem.Quantity != refStyleItem.RemainingQuantity
                        || !refStyleItem.QuantitySetBy.HasValue)
                    {
                        refStyleItem.Quantity = refStyleItem.RemainingQuantity; //NOTE: for viewing on UI
                        changedRefStyleItems.Add(refStyleItem);
                    }

                    //NOTE: DISABLE: keep source style qty w/o changes!!! affects when we have multiple virtual style
                    //Substract from linked style items
                    //foreach (var linkedStyleItemRef in linkedStyleItemRefs)
                    //{
                    //    var linkedStyleItem = allStyleItems.FirstOrDefault(si => si.StyleItemId == linkedStyleItemRef.LinkedStyleItemId);
                    //    if (linkedStyleItem != null)
                    //        linkedStyleItem.RemainingQuantity -= refStyleItem.RemainingQuantity;
                    //}
                    foreach (var styleItem in toSubstractRemaining)
                    {
                        styleItem.RemainingQuantity -= minQuantity;
                    }
                }
            }

            if (changedRefStyleItems.Any())
            {
                var styleItemIdList = changedRefStyleItems.Select(si => si.StyleItemId).ToList();
                var dbStyleItems = db.StyleItems.GetAll().Where(si => styleItemIdList.Contains(si.Id)).ToList();

                foreach (var changedRefStyleItem in changedRefStyleItems)
                {
                    var dbStyleItem = dbStyleItems.FirstOrDefault(si => si.Id == changedRefStyleItem.StyleItemId);
                    if (dbStyleItem != null)
                    {
                        dbStyleItem.Quantity = changedRefStyleItem.Quantity;
                        dbStyleItem.QuantitySetDate = _time.GetAmazonNowTime();
                        dbStyleItem.UpdateDate = when;
                    }
                }
                db.Commit();
            }
        }


        private void FixupRank(IList<ListingQuantityDTO> listings)
        {
            //NOTE: Rank correction, exclude 0 to prevent 0 dividing
            foreach (var l in listings)
            {
                var rank = l.Rank ?? RankHelper.DefaultRank;
                if (rank == 0)
                    rank = RankHelper.DefaultRank;

                l.ReciprocalRank = rank;
            }

            var rankSum = listings.Sum(l => l.ReciprocalRank);

            //Calc reverse rank
            foreach (var l in listings)
            {
                l.ReciprocalRank = 1.0f - l.ReciprocalRank / (float)rankSum;
            }

            //Normilize rank
            var reverseRankSum = listings.Sum(l => l.ReciprocalRank);

            if (listings.Count == 1)
            {
                listings[0].ReciprocalRank = 1;
            }
            else
            {
                foreach (var l in listings)
                {
                    l.ReciprocalRank = l.ReciprocalRank/(float) reverseRankSum;
                }
            }
        }

        public IList<ListingQuantityDTO> ResetOnHoldListings(IList<ListingQuantityDTO> listings)
        {
            foreach (var listing in listings)
            {
                if (listing.RealQuantity != 0 || listing.RestockDate != null)
                {
                    listing.RealQuantity = 0;
                    listing.RestockDate = null;
                    listing.DirtyStatus = (int)DirtyStatusEnum.OnHoldChanges;
                }
            }
            return listings;
        }

        public IList<ListingQuantityDTO> ResetUnpublishedListings(IList<ListingQuantityDTO> listings)
        {
            foreach (var listing in listings)
            {
                if (listing.RealQuantity != 0)
                {
                    listing.RealQuantity = 0;
                    listing.DirtyStatus = (int)DirtyStatusEnum.UnpublishChanges;
                }
            }
            return listings;
        }

        public IList<ListingQuantityDTO> ResetUnlinkedListings(IList<ListingQuantityDTO> listings)
        {
            foreach (var listing in listings)
            {
                if (listing.RealQuantity != 0)
                {
                    listing.RealQuantity = 0;
                    listing.DirtyStatus = (int)DirtyStatusEnum.DirectChanges;
                }
            }
            return listings;
        }

        public IList<ListingQuantityDTO> UpdateRestockDate(IList<ListingQuantityDTO> listings, 
            DateTime? restockDate)
        {
            foreach (var listing in listings)
            {
                if (restockDate != null && restockDate.Value.Date <= _time.GetAppNowTime().Date)
                {
                    restockDate = null;
                }

                if (listing.RestockDate != restockDate)
                {
                    listing.RestockDate = restockDate;
                    listing.DirtyStatus = (int) DirtyStatusEnum.RestockDateChanges;
                }
            }
            return listings;
        }


        //TODO: Create distribute quantity history 
        public IList<ListingQuantityDTO> ReDistributeQuantityBetweenStyleListings(IList<ListingQuantityDTO> listings, 
            int totalRemainingQuantity,
            int maxQuantity,
            int lowQuantity)
        {
            var sortedListings = listings
                .OrderBy(l => l.Rank)
                .ThenBy(l => l.Id)
                .ToList();
            
            var currentQuantitySum = listings.Sum(l => l.RealQuantity);
            var notUsedQuantity = totalRemainingQuantity - currentQuantitySum;

            //STEP 0. Calculate reciprocal/inverse rank
            FixupRank(sortedListings);
            
            //NOTE: max 30, up if lower then 10
            if (currentQuantitySum > totalRemainingQuantity ||
                listings.Any(l => l.RealQuantity < lowQuantity))
            {
                //STEP 1. Distribute not used quantity
                if (notUsedQuantity > 0)
                {
                    foreach (var listing in sortedListings)
                    {
                        if (listing.RealQuantity < lowQuantity && notUsedQuantity > 0)
                        {
                            var currentQuantity = listing.RealQuantity;
                            var newQuantity = Math.Min(maxQuantity, currentQuantity + notUsedQuantity);

                            //Update left quantity
                            notUsedQuantity -= (newQuantity - currentQuantity);

                            listing.RealQuantity = newQuantity;
                            listing.DirtyStatus = (int) DirtyStatusEnum.DirectChanges;
                        }
                    }
                }

                //STEP 2. Rebalance (if style STILL have listing with quantity that less the lower threshold)
                //NOTE: in this case we haven't not used quantity (STEP 1 don't help)
                //NOTE: OR In case of total quantity was reduced (some items may be sold via store)
                if (currentQuantitySum > totalRemainingQuantity
                    || listings.Any(l => l.RealQuantity < lowQuantity))
                {
                    currentQuantitySum = listings.Sum(l => l.RealQuantity);
                    //NOTE: Total remaining quantity not enough, do complete redistribution
                    var remainingQuantity = totalRemainingQuantity; //OR currentQuantitySum + notUsedQuantity;

                    //STEP 2.1. Trying to set every listing at least one quantity
                    //NOTE: should resolve case when one listing have very bad rank and don't receive quantity
                    if (remainingQuantity <= listings.Count)
                    {
                        //STEP 2.1.1 When left quantities less then listing count (distribute by one to listings while quantity exists, then 0)
                        foreach (var listing in sortedListings)
                        {
                            var newQuantity = Math.Min(1, remainingQuantity);
                            if (listing.RealQuantity != newQuantity)
                            {
                                listing.RealQuantity = newQuantity;
                                listing.DirtyStatus = (int) DirtyStatusEnum.DirectChanges;
                            }
                            remainingQuantity -= newQuantity;
                        }
                    }
                    else
                    {
                        //STEP 2.1.2 Left quantity more then listing count (but every listing should receive at least one quantity)
                        var notProcessingListingCount = sortedListings.Count;
                        foreach (var listing in sortedListings)
                        {
                            //ex.: rankSum=3.300.300, rank=300.000, quantitySum=5 => newQuantity=5
                            //ex.: rankSum=3.300.300, rank=300.000, quantitySum=11 => newQuantity=10
                            var newQuantity = (int)Math.Ceiling(currentQuantitySum * listing.ReciprocalRank);

                            if (newQuantity > maxQuantity)
                                newQuantity = maxQuantity;

                            //ex.: remain=3, notProcessingListingCount=2, newQuantity=3 => should be newQuantity=2
                            if (newQuantity > (remainingQuantity - (notProcessingListingCount - 1 /*Current listing*/)))
                                newQuantity = remainingQuantity - (notProcessingListingCount - 1);

                            if (newQuantity == 0)
                                newQuantity = 1; //every listing should receive at least one, otherwise we should finished at STEP 2.1.1

                            if (listing.RealQuantity != newQuantity)
                            {
                                listing.RealQuantity = newQuantity;
                                listing.DirtyStatus = (int) DirtyStatusEnum.DirectChanges;
                            }

                            remainingQuantity -= listing.RealQuantity;
                            notProcessingListingCount -= 1;
                        }
                    }
                }
            }

            //Apply not used qty
            currentQuantitySum = listings.Sum(l => l.RealQuantity);
            notUsedQuantity = totalRemainingQuantity - currentQuantitySum;
            if (notUsedQuantity > 0)
            {
                foreach (var listing in sortedListings)
                {
                    if (listing.RealQuantity < maxQuantity && listing.RealQuantity > 0)
                    {
                        var toAdd = Math.Min(notUsedQuantity, maxQuantity - listing.RealQuantity);
                        notUsedQuantity -= toAdd;
                        if (toAdd > 0)
                        {
                            listing.RealQuantity += toAdd;
                            listing.DirtyStatus = (int)DirtyStatusEnum.DirectChanges;
                        }
                    }
                }
            }

            return sortedListings;
        }

        private IList<ListingQuantityDTO> CopyToLinkedListings(IList<ListingQuantityDTO> sourceListings, IList<ListingQuantityDTO> allLinkedListings)
        {
            foreach (var listing in sourceListings)
            {
                var linkedListings = allLinkedListings.Where(l => l.SKU == listing.SKU).ToList();
                if (linkedListings.Any())
                {
                    foreach (var linkedListing in linkedListings)
                    {
                        if (linkedListing.RealQuantity != listing.RealQuantity)
                        {
                            linkedListing.RealQuantity = listing.RealQuantity;
                            linkedListing.DirtyStatus = (int)DirtyStatusEnum.LinkedChanges;
                        }

                        if (linkedListing.RestockDate != listing.RestockDate)
                        {
                            linkedListing.RestockDate = listing.RestockDate;
                            linkedListing.DirtyStatus = (int)DirtyStatusEnum.LinkedChanges;
                        }

                        if (linkedListing.OnHold != listing.OnHold)
                        {
                            linkedListing.OnHold = listing.OnHold;
                            linkedListing.DirtyStatus = (int)DirtyStatusEnum.LinkedChanges;
                        }
                        linkedListing.IsLinked = true;
                    }
                }
            }
            return allLinkedListings.ToList();
        }


        public IList<ListingQuantityDTO> ProcessSystemAction(ISystemActionService actionService)
        {
            var results = new List<ListingQuantityDTO>();
            using (var db = _dbFactory.GetRWDb())
            {
                var recalcActions = actionService.GetUnprocessedByType(db, SystemActionType.QuantityDistribute, null, null);

                foreach (var action in recalcActions)
                {
                    var actionStatus = SystemActionStatus.None;
                    try
                    {
                        var inputData = SystemActionHelper.FromStr<QuantityDistributeInput>(action.InputData);
                        var styleIdList = inputData.StyleIdList;
                        if ((styleIdList == null 
                            || !styleIdList.Any())
                            && inputData.StyleId.HasValue)
                        {
                            styleIdList = new long[] { inputData.StyleId.Value };
                        }

                        if (styleIdList != null && styleIdList.Any())
                        {
                            results.AddRange(RedistributeForStyle(db, styleIdList));
                        }

                        actionStatus = SystemActionStatus.Done;
                        _log.Info("Quantity distributed for styleId=" + String.Join(", ", styleIdList) + ", actionId=" + action.Id);
                    }
                    catch (Exception ex)
                    {
                        actionStatus = SystemActionStatus.Fail;
                        
                        _log.Error("Fail quantity distributed, actionId=" + action.Id + ", status=" + actionStatus, ex);
                    }

                    actionService.SetResult(db,
                        action.Id,
                        actionStatus,
                        null,
                        null);
                }

                db.Commit();
            }

            return results;
        }
    }
}
