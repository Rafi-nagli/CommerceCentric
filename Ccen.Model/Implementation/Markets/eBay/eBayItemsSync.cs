using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.DTO;
using Amazon.Model.Implementation.Sync;
using eBay.Api;
using Amazon.Core.Models;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;
using System.Linq.Expressions;
using Amazon.Core.Models.Items;

namespace Amazon.Model.Implementation.Markets.eBay
{
    public class eBayItemsSync : IItemsSync
    {
        private ILogService _log;
        private ITime _time;
        private eBayApi _api;
        private IDbFactory _dbFactory;
        private IStyleManager _styleManager;
        private ISystemActionService _actionService;
        private string _eBayImageDirectory;
        private string _eBayImageBaseUrl;
        private string _eBayBaseTemplateDirctory;

        public eBayItemsSync(ILogService log,
            ITime time,
            eBayApi api,
            IDbFactory dbFactory,
            IStyleManager styleManager,
            string eBayImageDirectory,
            string eBayImageBaseUrl,
            string eBayBaseTemplateDirctory)
        {
            _log = log;
            _time = time;
            _api = api;
            _dbFactory = dbFactory;
            _styleManager = styleManager;
            _eBayImageDirectory = eBayImageDirectory;
            _eBayImageBaseUrl = eBayImageBaseUrl;
            _eBayBaseTemplateDirctory = eBayBaseTemplateDirctory;
        }

        public void SendItemUpdates()
        {
            SendItemUpdates(null);
        }

        public void SendItemUpdates(IList<long> itemIds)
        {
            _log.Info("Begin ItemUpdates");
            var templatePath = Path.Combine(_eBayBaseTemplateDirctory, _api.TemplatePath);
            var descriptionTemplateContent = File.ReadAllText(templatePath);

            using (var db = _dbFactory.GetRWDb())
            {
                var toInProgressDate = _time.GetAppNowTime().AddHours(-30);
                var toErrorDate = _time.GetAppNowTime().AddHours(-30);
                var itemsToUpdateList = new List<Item>();

                if (itemIds == null)
                {
                    itemsToUpdateList = db.Items.GetAll()
                        .Where(i => (i.ItemPublishedStatus == (int) PublishedStatuses.None
                                     || i.ItemPublishedStatus == (int) PublishedStatuses.New
                                     || (i.ItemPublishedStatus == (int) PublishedStatuses.PublishingErrors
                                         && i.ItemPublishedStatusDate < toErrorDate)
                                     || i.ItemPublishedStatus == (int) PublishedStatuses.HasChanges
                                     || i.ItemPublishedStatus == (int) PublishedStatuses.HasChangesWithProductId
                                     || i.ItemPublishedStatus == (int) PublishedStatuses.HasChangesWithSKU
                                     || ((i.ItemPublishedStatus == (int) PublishedStatuses.PublishedInProgress
                                          || i.ItemPublishedStatus == (int) PublishedStatuses.ChangesSubmited)
                                         && i.ItemPublishedStatusDate < toInProgressDate))
                            //NOTE: Added in-progress statuses if items in it more then one day
                                    && i.Market == (int) _api.Market
                                    && i.MarketplaceId == _api.MarketplaceId
                                    && i.StyleItemId.HasValue)
                        .ToList();
                }
                else
                {
                    itemsToUpdateList = db.Items.GetAll().Where(i => itemIds.Contains(i.Id)).ToList();
                }

                //NOTE: Need to submit items with group, otherwise we have incorrect color variations calculation, and sometimes image calculation
                var parentASINList = itemsToUpdateList.Select(i => i.ParentASIN).Distinct().ToList();
                _log.Info("Parent ASIN Count to submit=" + parentASINList.Count + ", item count=" + itemsToUpdateList.Count);

                var allItemList = db.Items.GetAllActualExAsDto()
                      .Where(i => parentASINList.Contains(i.ParentASIN)
                        && i.Market == (int)_api.Market
                        && i.MarketplaceId == _api.MarketplaceId).ToList();

                //Refresh parentASIN list, exclude asins with not actual items
                parentASINList = allItemList.Select(i => i.ParentASIN).Distinct().ToList();


                var allParentItemList = db.ParentItems.GetAllAsDto().Where(p => parentASINList.Contains(p.ASIN)
                                                                         && p.Market == (int)_api.Market
                                                                         && p.MarketplaceId == _api.MarketplaceId)
                    .ToList();

                var allStyleIdList = allItemList.Where(i => i.StyleId.HasValue).Select(i => i.StyleId.Value).ToList();
                var allStyleList = db.Styles.GetAllAsDtoEx().Where(s => allStyleIdList.Contains(s.Id)).ToList();
                var allStyleImageList = db.StyleImages.GetAllAsDto().Where(sim => allStyleIdList.Contains(sim.StyleId)).ToList();
                var allFeatures = db.FeatureValues.GetValuesByStyleIds(allStyleIdList);

                foreach (var item in allItemList)
                {
                    var parent = allParentItemList.FirstOrDefault(p => p.ASIN == item.ParentASIN);
                    if (parent != null)
                        item.OnHold = parent.OnHold;
                }

                //Exclude OnHold (after ParentItem onHold was applied)
                allItemList = allItemList.Where(i => !i.OnHold).ToList();

                foreach (var styleImage in allStyleImageList)
                {
                    try
                    {
                        //NOTE: Need to replace Amazon https to our http
                        var filepath = ImageHelper.BuildEbayImage(_eBayImageDirectory, styleImage.Image);
                        var filename = Path.GetFileName(filepath);
                        styleImage.Image = _eBayImageBaseUrl + "/" + filename;
                        //if (styleImage.Image.ToLower().StartsWith("https://paimg.commercentric.com/image"))
                        //{
                        //    styleImage.Image = styleImage.Image.Replace("https://", "http://");
                        //}
                    }
                    catch (Exception ex)
                    {
                        _log.Info("BuildWalmartImage error, image=" + styleImage.Image, ex);
                    }
                }

                if (allItemList.Any())
                {
                    _log.Info("Items to update=" + String.Join(", ", allItemList.Select(i => i.SKU).ToList()));

                    foreach (var parentItem in allParentItemList)
                    {
                        var groupItems = allItemList.Where(i => i.ParentASIN == parentItem.ASIN).ToList();
                        var groupStyleIdList = groupItems.Where(i => i.StyleId.HasValue).Select(i => i.StyleId).Distinct().ToList();
                        var styles = allStyleList.Where(s => groupStyleIdList.Contains(s.Id)).ToList();
                        var styleFeatures = allFeatures.Where(f => groupStyleIdList.Contains(f.StyleId)).ToList();

                        var enableColorVariation = (parentItem != null && parentItem.ForceEnableColorVariations)
                            || groupItems.GroupBy(i => i.Size)
                            .Select(g => new
                            {
                                Count = g.Count(),
                                Size = g.Key,
                            })
                            .Count(g => g.Count > 1) > 0;

                        var styleImages = allStyleImageList
                           .Where(im => groupStyleIdList.Contains(im.StyleId)
                                && !im.IsSystem
                                && im.Category != (int)StyleImageCategories.Swatch)
                           .OrderByDescending(im => ImageHelper.GetSortIndex(im.Category))
                           .ThenByDescending(im => im.IsDefault)
                           .ThenBy(im => im.Id)
                           .ToList();

                        //SEND
                        var result = _api.SendItem(parentItem,
                            Path.GetFileNameWithoutExtension(templatePath) + "_" + File.GetLastWriteTimeUtc(templatePath).ToString("ddMMyyyyHHmmss"),
                            descriptionTemplateContent,
                            groupItems,
                            styles,
                            styleImages,
                            styleFeatures);

                        var itemIdList = groupItems.Select(i => i.Id).ToList();
                        var dbItems = db.Items.GetAll().Where(i => itemIdList.Contains(i.Id)).ToList();
                        var dbParentItem = db.ParentItems.GetAll().FirstOrDefault(pi => pi.Id == parentItem.Id);

                        //Remove all exist errors
                        var dbExistErrors = db.ItemAdditions.GetAll().Where(i => itemIdList.Contains(i.ItemId)
                                    && i.Field == ItemAdditionFields.PublishError).ToList();
                        foreach (var dbExistError in dbExistErrors)
                        {
                            db.ItemAdditions.Remove(dbExistError);
                        }

                        if (result.IsSuccess)
                        {
                            foreach (var dbItem in dbItems)
                            {
                                dbItem.SourceMarketId = result.Data;
                                dbItem.ItemPublishedStatus = (int) PublishedStatuses.Published;
                                dbItem.ItemPublishedStatusDate = _time.GetAppNowTime();
                            }
                            if (dbParentItem != null)
                            {
                                dbParentItem.SourceMarketId = result.Data;
                            }
                        }
                        else
                        {
                            foreach (var dbItem in dbItems)
                            {
                                dbItem.ItemPublishedStatus = (int)PublishedStatuses.PublishingErrors;
                                dbItem.ItemPublishedStatusDate = _time.GetAppNowTime();

                                db.ItemAdditions.Add(new Core.Entities.Listings.ItemAddition()
                                {
                                    ItemId = dbItem.Id,
                                    Field = ItemAdditionFields.PublishError,
                                    Value = result.Message,
                                    CreateDate = _time.GetAppNowTime(),
                                });

                                _log.Info("Update item status, itemId=" + dbItem.Id + ", status=" + dbItem.ItemPublishedStatus);
                            }
                        }

                        db.Commit();
                    }
                }
                _log.Info("End ItemUpdates");
            }
        }

        public void ReadItemsInfo()
        {
            SyncItemsInfo(null);
        }

        public void SyncItemsInfo(IList<string> marketItemIds)
        {
            _log.Info("Begin ReadItemsInfo");

            var asinsWithError = new List<string>();
            IList<ParentItemDTO> parentItemList = null;
            if (marketItemIds == null)
                parentItemList = _api.GetAllItems();
            else
                parentItemList = _api.GetItems(_log, _time, MarketItemFilters.Build(marketItemIds), ItemFillMode.Defualt, out asinsWithError).ToList();
            var parentASINsWithError = new List<string>();
            var parentItems = _api.GetItems(_log,
                _time,
                MarketItemFilters.Build(parentItemList.Select(p => p.ASIN).ToList()),
                ItemFillMode.Defualt,
                out parentASINsWithError);
            var market = _api.Market;
            var marketplaceId = _api.MarketplaceId;

            var missingSourceItemIds = new List<string>();
            var missingItems = new List<ItemDTO>();
            var existItemsCount = 0;

            var updatedItemIds = new List<long>();
            var updatedDbListingIds = new List<long>();
            var updatedDbItemIds = new List<long>();
            var index = 0;
            var pageSize = 100;
            
            var allItems = new List<ItemDTO>();

            using (var db = _dbFactory.GetRWDb())
            {
                allItems = (from l in db.Listings.GetAll()
                            join i in db.Items.GetAll() on l.ItemId equals i.Id
                            where l.Market == (int)market
                               && (String.IsNullOrEmpty(marketplaceId) || l.MarketplaceId == marketplaceId)
                               && !l.IsRemoved
                            select new ItemDTO()
                            {
                                Id = i.Id,
                                ListingEntityId = l.Id,
                                SourceMarketId = i.SourceMarketId,
                                SKU = l.SKU,
                                AmazonCurrentPrice = l.AmazonCurrentPrice,
                                AmazonRealQuantity = l.AmazonRealQuantity
                            }).ToList();
            }
            
            while (index < parentItems.Count())
            {
                _log.Info("Index: " + index);
                using (var db = _dbFactory.GetRWDb())
                {
                    var pageItems = parentItems.Skip(index).Take(pageSize).ToList();
                    foreach (var parentItem in pageItems)
                    {
                        foreach (var item in parentItem.Variations)
                        {
                            var listingInfo = allItems.FirstOrDefault(l => l.SourceMarketId == item.SourceMarketId
                                && l.SKU == item.SKU);

                            if (listingInfo == null)
                            {
                                missingItems.Add(item);
                                continue;
                            }
                            else
                            {
                                existItemsCount++;
                            }
                            
                            _log.Info("Update qty, listingId=" + listingInfo.ListingEntityId
                                            + ", sku=" + listingInfo.SKU
                                            + ", qty=" + listingInfo.AmazonRealQuantity + "=>" + item.Listings[0].Quantity
                                            + ", qty=" + listingInfo.AmazonCurrentPrice + "=>" + item.Listings[0].Price);
                            
                            if (String.IsNullOrEmpty(listingInfo.SourceMarketId)
                                || listingInfo.SourceMarketId != item.SourceMarketId)
                            {
                                if (!updatedDbItemIds.Contains(listingInfo.Id))
                                {
                                    db.Items.TrackItem(new Item()
                                    {
                                        Id = listingInfo.Id,
                                        SourceMarketId = item.SourceMarketId,
                                        SourceMarketUrl = item.SourceMarketUrl,
                                    },
                                    new List<Expression<Func<Item, object>>>()
                                    {
                                        l => l.SourceMarketId,
                                        l => l.SourceMarketUrl
                                    });

                                    updatedDbItemIds.Add(listingInfo.Id);
                                }
                                else
                                {
                                    _log.Error("Item Id already updated: " + listingInfo.Id);
                                }
                            }

                            if (!updatedDbListingIds.Contains(listingInfo.ListingEntityId.Value))
                            {
                                db.Listings.TrackItem(new Listing()
                                {
                                    Id = listingInfo.ListingEntityId.Value,
                                    AmazonCurrentPrice = item.Listings[0].Price,
                                    AmazonRealQuantity = item.Listings[0].Quantity,                                        
                                    AmazonCurrentPriceUpdateDate = _time.GetAppNowTime(),
                                    AmazonRealQuantityUpdateDate = _time.GetAppNowTime()
                                },
                                new List<Expression<Func<Listing, object>>>()
                                {
                                    l => l.AmazonCurrentPrice,
                                    l => l.AmazonRealQuantity,
                                    l => l.AmazonCurrentPriceUpdateDate,
                                    l => l.AmazonRealQuantityUpdateDate
                                });

                                updatedDbListingIds.Add(listingInfo.ListingEntityId.Value);
                            }
                            else
                            {
                                _log.Error("Listing Id already updated: " + listingInfo.ListingEntityId);
                            }

                            updatedItemIds.Add(listingInfo.Id);
                        }
                    }
                    db.Commit();
                }

                index += pageSize;
            }
            
            _log.Info(String.Format("Exist count/Missing count: {0}/{1}", existItemsCount, missingItems.Count));

            foreach (var missingItem in missingItems)
            {
                _log.Info(String.Format("Send zero, for SourceMarketId={0}, SKU={1}, Barcode={2}", missingItem.SourceMarketId, missingItem.SKU, missingItem.Barcode));
                var result = _api.SetItemQuantity(missingItem.SourceMarketId, missingItem.SKU, missingItem.Barcode, 0);
                if (result.IsFail
                    && StringHelper.ContainsNoCase(result.Message, "At least one of the variations associated with this listing must have a quantity greater than 0."))
                {
                    var endItemResult = _api.EndFixedPriceItem(missingItem.SourceMarketId);
                    if (endItemResult.IsFail)
                    {
                        _log.Info("Unable to set zero through closing listing. Message: " + endItemResult.Message);
                    }
                }
            }

            var missingItemIds = missingItems.Select(i => i.SourceMarketId).Distinct().ToList();
            var existItemIds = allItems.Select(i => i.SourceMarketId).Distinct().ToList();
            foreach (var missingItemId in missingItemIds)
            {
                if (existItemIds.Contains(missingItemId))
                    continue;
                var endItemResult = _api.EndFixedPriceItem(missingItemId);
                if (endItemResult.IsFail)
                {
                    _log.Info("Failed end item for SourceMarketId=" + missingItemId + ". Message=" + endItemResult.Message + ". Details: " + endItemResult.Details);
                }
                else
                {
                    _log.Info("Success. End item: " + missingItemId);
                }
            }

            using (var db = _dbFactory.GetRWDb())
            {
                //Remove sourceMarketId of non-exists parents
                var allDbParentItems = db.ParentItems.GetAll().Where(l => l.Market == (int)market
                                                        && (l.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId)))
                    .ToList();




                //Mark listings as unpublished
                var allDbItems = db.Items.GetAll().Where(l => l.Market == (int)market
                                                                        && (l.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))
                                                                        //&& l.ItemPublishedStatus == (int)PublishedStatuses.Published
                                                                        )
                    .ToList();

                foreach (var item in allDbItems)
                {
                    if (!updatedItemIds.Contains(item.Id)
                        && (item.ItemPublishedStatus == (int)PublishedStatuses.Published
                            || !String.IsNullOrEmpty(item.SourceMarketId)))
                    {
                        _log.Info("Mark as unpbulished, itemId=" + item.Id + ", in case of non exists on eBay");
                        item.SourceMarketId = null;
                        item.SourceMarketUrl = null;                       

                        item.ItemPublishedStatusBeforeRepublishing = item.ItemPublishedStatus;
                        item.ItemPublishedStatus = (int)PublishedStatuses.Unpublished;
                        item.ItemPublishedStatusDate = _time.GetAppNowTime();
                        item.ItemPublishedStatusReason = "Unpublished, unable to find on eBay";

                        item.IsExistOnAmazon = false;

                        var dbItemListings = db.Listings.GetAll().Where(l => l.ItemId == item.Id).ToList();
                        foreach (var dbListing in dbItemListings)
                        {
                            dbListing.AmazonRealQuantity = null;
                            dbListing.AmazonCurrentPrice = null;
                            dbListing.AmazonCurrentPriceUpdateDate = null;
                            dbListing.AmazonRealQuantityUpdateDate = null;
                            dbListing.SendToAmazonQuantity = null;
                        }
                    }
                    else
                    {
                        if (updatedItemIds.Contains(item.Id)
                            && item.ItemPublishedStatus == (int)PublishedStatuses.Unpublished)
                        {
                            _log.Info("Mark unpublished => published, itemId=" + item.Id + ", exists on eBay");
                            item.ItemPublishedStatusBeforeRepublishing = item.ItemPublishedStatus;
                            item.ItemPublishedStatus = (int)PublishedStatuses.Published;
                            item.ItemPublishedStatusDate = _time.GetAppNowTime();
                            item.ItemPublishedStatusReason = "Published status received from eBay";
                        }
                    }
                }
                db.Commit();
            }

            _log.Info("End ReadItemsInfo");
        }
        

        public void ReadItems(DateTime? lastSync)
        {
            _log.Info("Begin ReadItems");

            var parentItemList = _api.GetAllItems();
            var parentASINsWithError = new List<string>();
            var parentItems = _api.GetItems(_log, 
                _time,
                MarketItemFilters.Build(parentItemList.Select(p => p.ASIN).ToList()),
                ItemFillMode.Defualt,
                out parentASINsWithError);
            var market = _api.Market;
            var marketplaceId = _api.MarketplaceId;

            var existListingIds = new List<string>();

            using (var db = _dbFactory.GetRWDb())
            {
                foreach (var parentItem in parentItems)
                {
                    foreach (var item in parentItem.Variations)
                    {
                        var parentDb = db.ParentItems.GetByASIN(item.ParentASIN, market, marketplaceId);
                        var itemDb = db.Items.GetByASIN(item.ASIN, market, marketplaceId);
                        var listingDbList = db.Listings.GetByListingIdIncludeRemoved(item.ListingId, market, marketplaceId);

                        if (parentDb == null)
                        {
                            _log.Info("Add new parent=" + parentItem.ASIN);
                            parentDb = new ParentItem()
                            {
                                ASIN = parentItem.ASIN,
                                Market = (int) market,
                                MarketplaceId = marketplaceId,
                                AmazonName = parentItem.AmazonName,
                                ImageSource = parentItem.ImageSource,
                                AdditionalImages = parentItem.AdditionalImages,

                                CreateDate = _time.GetAppNowTime()
                            };
                            db.ParentItems.Add(parentDb);
                        }
                        else
                        {
                            parentDb.AmazonName = parentItem.AmazonName;
                            if (!String.IsNullOrEmpty(parentItem.ImageSource))
                                parentDb.ImageSource = parentItem.ImageSource;
                            if (!String.IsNullOrEmpty(parentItem.AdditionalImages))
                                parentDb.AdditionalImages = parentItem.AdditionalImages;

                            parentDb.UpdateDate = _time.GetAppNowTime();
                        }

                        StyleItemDTO styleItem = null;
                        if (itemDb == null || !itemDb.StyleItemId.HasValue)
                        {
                            //NOTE: Keep existing styleId
                            if (itemDb != null && itemDb.StyleId.HasValue)
                                item.StyleId = itemDb.StyleId;
                            
                            styleItem = FindOrCreateStyleItem(db, item);
                        }

                        if (itemDb == null)
                        {
                            _log.Info("Add new item, styleId=" + item.StyleString 
                                + "(found style=" + (styleItem != null ? styleItem.StyleId.ToString() : "[null]") + ")"
                                + "(found styleItem=" + (styleItem != null ? styleItem.StyleItemId.ToString() : "[null]") + ")");

                            itemDb = new Item()
                            {
                                ParentASIN = parentItem.ASIN,
                                ASIN = item.ASIN,
                                Market = (int) market,
                                MarketplaceId = marketplaceId,

                                Title = item.Name,

                                Size = item.Size,
                                ColorVariation = item.ColorVariation,
                                StyleString = item.StyleString,

                                StyleId = styleItem != null && styleItem.StyleId > 0 ? styleItem.StyleId : (long?)null,
                                StyleItemId = styleItem != null && styleItem.StyleItemId > 0 ? styleItem.StyleItemId : (long?)null,

                                PrimaryImage = item.ImageUrl,
                                AdditionalImages = item.AdditionalImages,

                                BrandName = item.BrandName,
                                Color = item.Color,

                                CreateDate = _time.GetAppNowTime()
                            };
                            db.Items.Add(itemDb);
                        }
                        else
                        {
                            //Update style
                            if (!itemDb.StyleItemId.HasValue)
                            {
                                itemDb.StyleString = item.StyleString;
                                
                                if (styleItem != null)
                                {
                                    if (styleItem.StyleItemId > 0)
                                    {
                                        itemDb.StyleId = styleItem.StyleId;
                                        itemDb.StyleItemId = styleItem.StyleItemId;
                                    }
                                    else
                                    {
                                        if (!itemDb.StyleId.HasValue)
                                            itemDb.StyleId = styleItem.StyleId;
                                    }
                                }
                            }

                            itemDb.Title = item.Name;

                            itemDb.Size = item.Size;
                            itemDb.ColorVariation = item.ColorVariation;

                            if (!String.IsNullOrEmpty(item.ImageUrl))
                                itemDb.PrimaryImage = item.ImageUrl;
                            if (!String.IsNullOrEmpty(item.AdditionalImages))
                                itemDb.AdditionalImages = item.AdditionalImages;

                            itemDb.BrandName = item.BrandName;
                            itemDb.Color = item.Color;

                            itemDb.UpdateDate = _time.GetAppNowTime();
                        }
                        db.Commit(); //generate ItemId

                        if (listingDbList.Count == 0)
                        {
                            var listingDb = new Listing()
                            {
                                Market = (int) market,
                                MarketplaceId = marketplaceId,

                                ASIN = itemDb.ASIN,
                                ItemId = itemDb.Id,
                                SKU = item.Listings[0].ListingId, //item.Listings[0].SKU
                                ListingId = item.Listings[0].ListingId,

                                CurrentPrice = item.Listings[0].Price,
                                CurrentPriceCurrency = "USD",
                                CurrentPriceInUSD = item.Listings[0].Price,
                                AmazonCurrentPrice = item.Listings[0].Price,

                                RealQuantity = item.Listings[0].Quantity,
                                SoldQuantity = item.Listings[0].SoldQuantity,
                                AmazonRealQuantity = item.Listings[0].Quantity,

                                CreateDate = _time.GetAppNowTime(),
                            };
                            db.Listings.Add(listingDb);
                            existListingIds.Add(listingDb.ListingId);
                        }
                        else
                        {
                            foreach (var listingDb in listingDbList)
                            {
                                _log.Info("Update qty, listingId=" + listingDb.ListingId
                                    + ", prev qty=" + listingDb.AmazonRealQuantity
                                    + ", new qty=" + item.Listings[0].Quantity);

                                listingDb.SKU = item.Listings[0].ListingId; 

                                //TODO: When start sent price updates remove this line
                                listingDb.CurrentPrice = item.Listings[0].Price;

                                listingDb.AmazonCurrentPrice = item.Listings[0].Price;

                                //listingDb.RealQuantity = item.Listings[0].Quantity;
                                listingDb.AmazonRealQuantity = item.Listings[0].Quantity;

                                listingDb.SoldQuantity = item.Listings[0].SoldQuantity;
                                listingDb.UpdateDate = _time.GetAppNowTime();

                                listingDb.IsRemoved = false;

                                existListingIds.Add(listingDb.ListingId);
                            }
                        }

                        db.Commit();
                    }
                }

                _log.Info("MarkAsRemoved");
                var removedList = db.Listings.MarkNotExistingAsRemoved(existListingIds, market, marketplaceId);
                _log.Info("Removed: " + String.Join(",", removedList));
            }

            _log.Info("End ReadItems");
        }

        private StyleItemDTO FindOrCreateStyleItem(IUnitOfWork db, ItemDTO dtoItem)
        {
            var styleItem = _styleManager.TryGetStyleItemIdFromOtherMarkets(db, dtoItem);
            if (styleItem == null)
            {
                styleItem = _styleManager.FindOrCreateStyleAndStyleItemForItem(db, 
                    ItemType.Pajama,
                    dtoItem, 
                    false,
                    false);
            }
            return styleItem;
        }

        public void SendInventoryUpdates()
        {
            SendInventoryUpdates(null);
        }

        public void SendInventoryUpdates(IList<long> itemIds)
        {
            _log.Info("Begin SendInventoryUpdates");
            using (var db = _dbFactory.GetRWDb())
            {
                IList<ItemDTO> items = null;
                IList<Listing> listings = null;

                if (itemIds == null)
                {
                    var beforeDate = _time.GetAppNowTime().AddMinutes(-360); //NOTE: update not often, every 6 hours
                    listings = db.Listings.GetQuantityUpdateRequiredList(_api.Market, _api.MarketplaceId);
                    var itemIdList = listings.Where(l => !l.LastQuantityUpdatedOnMarket.HasValue || l.LastQuantityUpdatedOnMarket < beforeDate).Select(l => l.ItemId).ToList();

                    items = db.Items.GetAllViewAsDto()
                        .Where(i => itemIdList.Contains(i.Id)
                                    && (i.PublishedStatus == (int) PublishedStatuses.Published
                                        || i.PublishedStatus == (int)PublishedStatuses.HasChanges
                                        || i.PublishedStatus == (int)PublishedStatuses.PublishingErrors)
                                    && !String.IsNullOrEmpty(i.SourceMarketId))
                        .ToList();
                }
                else
                {
                    items = db.Items.GetAllViewAsDto()
                        .Where(i => itemIds.Contains(i.Id))
                        .ToList();
                    listings = db.Listings.GetAll().Where(l => itemIds.Contains(l.ItemId)).ToList();
                }

                var listingWithErrorList = new List<Listing>();

                foreach (var listing in listings)
                {
                    var item = items.FirstOrDefault(i => i.Id == listing.ItemId);
                    if (item != null)
                    {
                        _log.Info("Send qty updates, sourceMarketId=" + item.SourceMarketId + ", qty=" + listing.RealQuantity);
                        var result = _api.SetItemQuantity(item.SourceMarketId,
                            listing.SKU,
                            item.Barcode,
                            listing.RealQuantity);
                        if (result.IsSuccess)
                        {
                            listing.QuantityUpdateRequested = false;
                            //NOTE: overwrite quantities, prevent to check after read qties
                            //listing.AmazonRealQuantity = listing.RealQuantity;
                            db.Commit();
                            _log.Info("Qty updated, sourceMarketId=" + item.SourceMarketId + ", SKU=" + listing.SKU + ", sendQty=" + listing.RealQuantity);
                        }
                        else
                        {
                            listingWithErrorList.Add(listing);
                            _log.Info("Can't update qty, result=" + result.ToString());
                        }
                        listing.LastQuantityUpdatedOnMarket = _time.GetAppNowTime();
                        db.Commit();
                    }
                    else
                    {
                        //Nothing, generally these listings aren't published
                    }
                }

                foreach (var listing in listingWithErrorList)
                {
                    var item = items.FirstOrDefault(i => i.Id == listing.ItemId);
                    if (item != null)
                    {
                        var allChildItems = db.Items.GetAllViewAsDto()
                            .Where(i => i.ParentASIN == item.ParentASIN
                                && i.Market == (int)_api.Market
                                && (i.MarketplaceId == _api.MarketplaceId
                                    || String.IsNullOrEmpty(_api.MarketplaceId))).ToList();
                        if (allChildItems.All(i => i.RealQuantity == 0))
                        {
                            _log.Info("Trying end item, ItemId=" + item.SourceMarketId);
                            var endItemResult = _api.EndFixedPriceItem(item.SourceMarketId);
                            if (endItemResult.IsSuccess)
                            {
                                _log.Info("Listing ended");

                                //NOTE: other child listings still available (not removed)
                                //listing.IsRemoved = true;

                                listing.QuantityUpdateRequested = false;
                                listing.AmazonRealQuantity = listing.RealQuantity;
                                db.Commit();
                                _log.Info("Qty updated, listingId=" + listing.ListingId + ", sendQty=" +
                                            listing.RealQuantity);
                            }
                            else
                            {
                                _log.Info("Can't endItem, result=" + endItemResult.ToString());
                            }
                        }
                    }
                    else
                    {
                        _log.Warn("Can't find item, for listing=" + listing.ListingId);
                    }
                }
            }
            _log.Info("End SendInventoryUpdates");
        }

        public void SendPriceUpdates()
        {
            _log.Info("Begin SendPriceUpdates");
            using (var db = _dbFactory.GetRWDb())
            {
                var beforeDate = _time.GetAppNowTime().AddMinutes(-360); //NOTE: update not often, every 180 min
                var listings = db.Listings.GetPriceUpdateRequiredList(_api.Market, _api.MarketplaceId);
                var itemIdList = listings.Where(l => 
                    !l.LastPriceUpdatedOnMarket.HasValue || l.LastPriceUpdatedOnMarket < beforeDate).Select(l => l.ItemId).ToList();
                var items = db.Items.GetAllViewAsDto()
                    .Where(i => itemIdList.Contains(i.Id)
                        && (i.PublishedStatus == (int)PublishedStatuses.Published
                            || i.PublishedStatus == (int)PublishedStatuses.HasChanges)
                        && !String.IsNullOrEmpty(i.SourceMarketId))
                    .ToList();

                var listingWithErrorList = new List<Listing>();
                var today = _time.GetAppNowTime();
                var market = _api.Market;
                var marketplaceId = _api.MarketplaceId;

                foreach (var listing in listings)
                {
                    var item = items.FirstOrDefault(i => i.Id == listing.ItemId);
                    if (item != null)
                    {
                        var currentPrice = listing.CurrentPrice;
                        if (item.SalePrice.HasValue && item.SaleStartDate <= today)
                            currentPrice = item.SalePrice.Value;

                        var usePromotion = false;
                        var parentItem = db.ParentItems.GetAll().FirstOrDefault(pi => pi.ASIN == item.ParentASIN 
                            && pi.Market == (int) market
                            && pi.MarketplaceId == marketplaceId);
                        
                        if (parentItem != null)
                        {
                            var promotionId = StringHelper.TryGetLong(parentItem.PromotionId);
                            var discountValue = item.SalePrice.HasValue ? (listing.CurrentPrice - item.SalePrice.Value) : 0;
                            var childItems = db.Items.GetAllViewActual()
                                    .Where(i => i.ParentASIN == parentItem.ASIN 
                                        && i.Market == (int) market
                                        && i.MarketplaceId == marketplaceId)
                                    .ToList();
                            if (false //NOTE: Temporary disable promotions, eBay has issue with it
                                && childItems.Any()
                                && childItems.All(i => i.SalePrice.HasValue)
                                && childItems.All(i => i.CurrentPrice - i.SalePrice > 0 //NOTE: not anti-sale
                                && i.CurrentPrice - i.SalePrice == discountValue && i.SaleStartDate <= today))
                            {
                                //enable promotion
                                usePromotion = true;
                                
                                if (!promotionId.HasValue)
                                {
                                    var createPromotionResult = false;

                                    //Create promotion if no
                                    var addSaleResult = _api.AddSale(new SaleDTO()
                                    {
                                        SaleName = parentItem.ASIN,
                                        SaleStartDate = _time.GetAppNowTime().Date,
                                        SaleEndDate = _time.GetAppNowTime().Date.AddDays(45),
                                        DiscountValue = discountValue,
                                    });
                                    if (addSaleResult.IsSuccess)
                                    {
                                        var addItemToSaleResult = _api.AddItemsToSale(addSaleResult.Data, new List<string>() {parentItem.SourceMarketId});
                                        if (addItemToSaleResult.IsSuccess)
                                        {
                                            //Update promotion info
                                            parentItem.PromotionId = addSaleResult.Data.ToString();
                                            parentItem.PromotionCreateDate = _time.GetUtcTime();
                                            db.Commit();

                                            //Request updating for all childs (except current)
                                            var childIdList = childItems.Select(i => i.Id).ToList();
                                            var dbChildListings =
                                                db.Listings.GetAll().Where(l => childIdList.Contains(l.ItemId)).ToList();
                                            foreach (var dbListing in dbChildListings)
                                            {
                                                if (dbListing.Id != listing.Id)
                                                    dbListing.PriceUpdateRequested = true;
                                            }
                                            db.Commit();

                                            createPromotionResult = true;
                                        }
                                    }

                                    if (!createPromotionResult)
                                    {
                                        listing.LastPriceUpdatedOnMarket = _time.GetAppNowTime().AddHours(12);
                                        //NOTE: after any errors, reprocessing with delay, prevent reaching call limits
                                        db.Commit();

                                        _log.Error("Can't create promotion, for ParentASIN=" + parentItem.ASIN);
                                        continue;
                                    }
                                }
                                else
                                {
                                    //Update promotion price if needed
                                    var getSaleResult = _api.GetSale(promotionId.Value);
                                    if (getSaleResult.IsSuccess)
                                    {
                                        if (getSaleResult.Data.DiscountValue != discountValue)
                                            _api.UpdateSale(promotionId.Value, discountValue);
                                    }
                                }
                            }
                            else
                            {
                                //when can't use promotion
                                usePromotion = false;
                                
                                //also need to disable promotion (if exists) for other child items by requesting updates for them
                                if (promotionId.HasValue)
                                {
                                    //var removeItemResult = _api.RemoveItemsFromSale(Int32.Parse(parentItem.PromotionId), new List<string>() { parentItem.SourceMarketId });
                                    var deleteResult = _api.DeleteSale(promotionId.Value);
                                    _log.Info("API: delete promotion, Id=" + parentItem.PromotionId + ", result=" + deleteResult.IsSuccess);
                                    if (deleteResult.IsSuccess)
                                    {
                                        //Update promotion info
                                        parentItem.PromotionId = null;

                                        //Request updating for all childs
                                        var childIdList = childItems.Select(i => i.Id).ToList();
                                        var dbChildListings = db.Listings.GetAll().Where(l => childIdList.Contains(l.ItemId)).ToList();
                                        foreach (var dbListing in dbChildListings)
                                        {
                                            if (dbListing.Id != listing.Id)
                                                dbListing.PriceUpdateRequested = true;
                                        }
                                        db.Commit();
                                    }
                                    else
                                    {
                                        listing.LastPriceUpdatedOnMarket = _time.GetAppNowTime();
                                        //NOTE: after any errors, reprocessing with delay, prevent reaching call limits
                                        db.Commit();

                                        _log.Error("Can't delete promotion, for ParentASIN=" + parentItem.ASIN + ", message=" + deleteResult.Message);
                                        continue;
                                    }
                                }
                            }
                        }

                        if (usePromotion)
                            currentPrice = listing.CurrentPrice; //Exclude sale when promotion enabled, back price to original

                        _log.Info("Send price updates, sourceMarketId=" + item.SourceMarketId
                                    + ", size=" + item.StyleSize
                                    + ", price=" + listing.CurrentPrice
                                    + ", includeSale=" + currentPrice
                                    + ", usePromotion=" + usePromotion);
                        var result = _api.SetItemPrice(item.SourceMarketId,
                            item.Size,
                            item.ColorVariation,
                            listing.SKU,
                            item.Barcode,
                            currentPrice);


                        var updateResult = result.IsSuccess;
                        if (result.IsSuccess)
                        {
                            //Check listing in promotion
                            if (usePromotion)
                            {
                                updateResult = false;
                                var promotionId = StringHelper.TryGetLong(parentItem.PromotionId);
                                if (promotionId.HasValue)
                                {
                                    var saleInfo = _api.GetSale(promotionId.Value);
                                    if (saleInfo != null)
                                    {
                                        if (!saleInfo.Data.LinkedItemIds.Contains(item.SourceMarketId))
                                        {
                                            var addResult = _api.AddItemsToSale(promotionId.Value, new List<string>() {item.SourceMarketId});
                                            if (addResult.IsSuccess)
                                                updateResult = true;
                                        }
                                        else
                                        {
                                            updateResult = true;
                                        }
                                    }
                                }

                            }
                        }


                        if (updateResult)
                        { 
                            listing.PriceUpdateRequested = false;
                            listing.AmazonCurrentPrice = currentPrice;
                            listing.LastPriceUpdatedOnMarket = _time.GetAppNowTime();

                            _log.Info("Price updated, listingId=" + listing.ListingId + ", sendPrice=" +
                                        currentPrice);
                        }
                        else
                        {
                            listing.LastPriceUpdatedOnMarket = _time.GetAppNowTime();
                                //NOTE: after any errors, reprocessing with delay, prevent reaching call limits
                            listingWithErrorList.Add(listing);
                            _log.Info("Can't update prices, result=" + result.ToString());
                        }
                        db.Commit();
                    }
                    else
                    {
                        //Nothing, generally these listings aren't published
                    }
                }

                foreach (var listing in listingWithErrorList)
                {
                    //TODO: additional logic
                }
            }
            _log.Info("End SendPriceUpdates");
        }
    }
}
