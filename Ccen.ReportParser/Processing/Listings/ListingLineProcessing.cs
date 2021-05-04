using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Notifications;
using Amazon.Core.Contracts.Notifications.NotificationParams;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.Core.Models.Items;
using Amazon.DTO;
using Amazon.ImageProcessing;
using Amazon.Model.Models;

namespace Amazon.ReportParser.Processing.Listings
{
    public class ListingLineProcessing
    {
        private ILogService _log;
        private ITime _time;
        private long _companyId;
        private IStyleManager _styleManager;
        private INotificationService _notificationService;
        private IStyleHistoryService _styleHistoryService;
        private IItemHistoryService _itemHistoryService;
        private ISystemActionService _actionService;
        private ISyncInformer _syncInfo;

        private bool _canCreateStyleInfo;
        private bool _enableFakeParentASIN;


        public ListingLineProcessing(ParseContext parseContext, 
            ITime time, 
            bool canCreateStyleInfo)
        {
            _log = parseContext.Log;
            _companyId = parseContext.CompanyId;
            _styleManager = parseContext.StyleManager;
            _syncInfo = parseContext.SyncInformer;
            _notificationService = parseContext.NotificationService;
            _styleHistoryService = parseContext.StyleHistoryService;
            _itemHistoryService = parseContext.ItemHistoryService;
            _actionService = parseContext.ActionService;
            _time = time;
            _canCreateStyleInfo = canCreateStyleInfo;
        }
        
        public void ProcessExistingItems(IUnitOfWork db, IMarketApi api, List<ItemDTO> items)
        {
            _log.Debug("Begin process existing items");

            var index = 0;
            //var logUpdatedItems = new List<string>();
            foreach (var dtoItem in items)
            {
                try
                {
                    Item dbItem = null;
                    var dbListing = db.Listings.GetBySKU(dtoItem.SKU, api.Market, api.MarketplaceId);
                    if (dbListing != null)
                        dbItem = db.Items.Get(dbListing.ItemId);

                    if (dbItem == null)
                    {
                        _log.Fatal("ProcessExistingItems. Can't find item in DB with SKU=" + dtoItem.SKU);
                        continue;
                    }

                    if (dtoItem.IsExistOnAmazon == true)
                    {
                        _log.Info("Filled by Amazon (exist item), asin=" + dtoItem.ASIN + ", SKU=" + dtoItem.SKU + ", parentASIN=" + dtoItem.ParentASIN);
                        
                        SetItemFieldsIfExistOnAmazon(db, dtoItem, dbItem);

                        SetListingFieldsIfExistOnAmazon(db, dbListing, dtoItem);
                    }
                    else
                    {
                        if (dtoItem.IsExistOnAmazon == false) //NOTE: skip when =NULL
                        {
                            SetItemFieldsIfNotExistOnAmazon(db, dbItem);
                        }

                        _syncInfo.AddWarning(dtoItem.ASIN, "Item is not filled by Amazon (exist item)");
                        _log.Warn("Item is not filled by Amazon, item=" + dtoItem.ASIN);
                    }
                }
                catch (Exception ex)
                {
                    _syncInfo.AddError(dtoItem.ASIN, "Error while updating item", ex);
                    _log.Error(string.Format("Error while updating item {0}", dtoItem.ASIN), ex);
                }
                index++;

                if (index % 100 == 0)
                {
                    db.Commit();
                    db.ReCreate();
                }
            }
            db.Commit();
            //_syncInfo.AddEntities(SyncMessageStatus.Success, logUpdatedItems, "Updated by Amazon (exist items)");
            _log.Debug("End process items");
        }

        private void SetListingFieldsIfExistOnAmazon(IUnitOfWork db, Listing dbListing, ItemDTO dtoItem)
        {
            dbListing.ASIN = dtoItem.ASIN;
            dbListing.IsFBA = dtoItem.IsFBA;
            dbListing.IsPrime = dtoItem.IsPrime;
            dbListing.ListingId = dtoItem.ListingId;
            dbListing.IsRemoved = false;

            if (dtoItem.LowestPrice.HasValue)
                dbListing.LowestPrice = dtoItem.LowestPrice;

            if (dbListing.AmazonRealQuantity != dtoItem.RealQuantity)
            {
                _log.Debug("Price changed: " + dbListing.SKU + ": " + dbListing.AmazonRealQuantity + "=>" + dtoItem.RealQuantity);
                dbListing.AmazonRealQuantity = dtoItem.RealQuantity;
                dbListing.AmazonRealQuantityUpdateDate = _time.GetAppNowTime();
            }
            if (dbListing.AmazonCurrentPrice != dtoItem.CurrentPrice)
            {
                _log.Debug("Price changed: " + dbListing.SKU + ": " + dbListing.AmazonCurrentPrice + "=>" + dtoItem.CurrentPrice);
                dbListing.AmazonCurrentPrice = dtoItem.CurrentPrice;
                dbListing.AmazonCurrentPriceUpdateDate = _time.GetAppNowTime();
            }


            //NOTE: business price updates always, then fixup service, 
            //correcting it and mark as update required

            if (dbListing.BusinessPrice != dtoItem.BusinessPrice)
            {
                _log.Info("Set business price=" + dbListing.BusinessPrice + "=>" + dtoItem.BusinessPrice);
                dbListing.BusinessPrice = dtoItem.BusinessPrice;
            }
        }

        private void SetItemFieldsIfNotExistOnAmazon(IUnitOfWork db, Item dbItem)
        {
            dbItem.IsExistOnAmazon = false;
            dbItem.MarketParentASIN = "";
            dbItem.LastUpdateFromAmazon = DateTime.UtcNow;
            //NOTE: Reset publishing status
            if (dbItem.ItemPublishedStatus == (int)PublishedStatuses.Published)
            {                
                dbItem.ItemPublishedStatusReason = "System Warning: the listing has the Published status, but it does not appear in the listing report.";
                dbItem.ItemPublishedStatusDate = _time.GetAppNowTime();
                dbItem.ItemPublishedStatus = (int)PublishedStatuses.PublishedInProgress;
            }
        }

        private void SetItemFieldsIfExistOnAmazon(IUnitOfWork db, ItemDTO dtoItem, Item dbItem)
        {
            //NOTE: Skip any ParentASIN updates
            //NOTE: updates for childs only when we change ParentItem objects
            //NOTE: to keep source relationships
            //if (!String.IsNullOrEmpty(dtoItem.ParentASIN))
            //{
            //    dbItem.ParentASIN = dtoItem.ParentASIN;
            //}

            //NOTE: only when no ParentASIN
            if (String.IsNullOrEmpty(dbItem.ParentASIN)
                && !String.IsNullOrEmpty(dtoItem.ParentASIN))
            {
                dbItem.ParentASIN = dtoItem.ParentASIN;
            }

            if (dbItem.ASIN != dtoItem.ASIN)
            {
                _log.Info("For SKU=" + dtoItem.SKU + "(id: " + dbItem.Id + ")" + ", ASIN=" + dbItem.ASIN + "=>" + dtoItem.ASIN);
                dbItem.ASIN = dtoItem.ASIN;
            }

            if (dbItem.ItemPublishedStatus == (int)PublishedStatuses.New
                || dbItem.ItemPublishedStatus == (int)PublishedStatuses.PublishedInactive
                || dbItem.ItemPublishedStatus == (int)PublishedStatuses.PublishedInProgress
                || dbItem.ItemPublishedStatus == (int)PublishedStatuses.PublishingErrors
                || dbItem.ItemPublishedStatus == (int)PublishedStatuses.None
                || dbItem.ItemPublishedStatus == (int)PublishedStatuses.HasPublishRequest)
                //NOTE: should be excluded HasUnpublishdRequest
            {                
                dbItem.ItemPublishedStatus = (int)PublishedStatuses.Published;
                dbItem.ItemPublishedStatusReason = "";
                dbItem.ItemPublishedStatusDate = _time.GetAppNowTime();  
            }

            dbItem.ItemPublishedStatusFromMarket = (int)PublishedStatuses.Published;
            dbItem.ItemPublishedStatusFromMarketDate = _time.GetAppNowTime();

            dbItem.IsAmazonParentASIN = dtoItem.ParentASIN == dbItem.ParentASIN;
            dbItem.SourceMarketId = dtoItem.SourceMarketId;
            dbItem.MarketParentASIN = dtoItem.ParentASIN;

            if (dbItem.OnMarketTemplateName != dtoItem.OnMarketTemplateName)
                _log.Debug("Template changed: " + dbItem.OnMarketTemplateName + "=>" + dtoItem.OnMarketTemplateName);
            dbItem.OnMarketTemplateName = dtoItem.OnMarketTemplateName;

            //Updates size if came not null
            if (!String.IsNullOrEmpty(dtoItem.Size))
            {
                var newSize = StringHelper.Substring(dtoItem.Size, 50);
                if (dbItem.Size != newSize)
                {
                    if (_itemHistoryService != null)
                        _itemHistoryService.AddRecord(dbItem.Id,
                            ItemHistoryHelper.SizeKey,
                            dbItem.Size,
                            "ListingLineProcessing.SetItemFieldsIfExistOnAmazon",
                            newSize,
                            null,
                            null);
                    dbItem.Size = newSize;
                }
            }
            else
            {
                dtoItem.Size = dbItem.Size; //NOTE: item DTO size using in below code (should be actual)
            }

            dbItem.IsExistOnAmazon = true;
            dbItem.LastUpdateFromAmazon = _time.GetUtcTime();

            //Set style string
            if (String.IsNullOrEmpty(dbItem.StyleString))
            {
                dtoItem.StyleString = SkuHelper.RetrieveStyleIdFromSKU(db, 
                    dtoItem.SKU, 
                    dtoItem.Name);

                _log.Debug(string.Format("StyleString updated from={0}, to={1}", dbItem.StyleString, dtoItem.StyleString));
                dbItem.StyleString = dtoItem.StyleString;
            }
            else
            {
                dtoItem.StyleString = dbItem.StyleString;
            }

            //Additional Fields
            dbItem.Title = dtoItem.Name;
            if (!String.IsNullOrEmpty(dtoItem.ImageUrl))
                dbItem.PrimaryImage = dtoItem.ImageUrl;
            dbItem.BrandName = dtoItem.BrandName;
            dbItem.Type = StringHelper.Substring(dtoItem.Type, 50);
            dbItem.ListPrice = dtoItem.ListPrice;
            dbItem.Color = StringHelper.Substring(dtoItem.Color, 50);
            dbItem.Department = dtoItem.Department;
            dbItem.AdditionalImages = dtoItem.AdditionalImages;
            if (!String.IsNullOrEmpty(dtoItem.Features))
                dbItem.Features = dtoItem.Features;
            if (!String.IsNullOrEmpty(dtoItem.SearchKeywords))
                dbItem.SearchKeywords = dtoItem.SearchKeywords;
            if (!String.IsNullOrEmpty(dtoItem.Barcode))
                dbItem.Barcode = dtoItem.Barcode;

            //Setting styleId, create style, styleItem if not exist
            if (!dbItem.StyleItemId.HasValue)
            {
                //Kepp exists styleId
                dtoItem.StyleId = dbItem.StyleId;

                _log.Info("FindOrCreateStyleItem");
                var styleItem = FindOrCreateStyleItem(db, dtoItem);
                if (styleItem != null)
                {
                    if (styleItem.StyleItemId > 0)
                    {
                        _styleHistoryService.AddRecord(styleItem.StyleId,
                            StyleHistoryHelper.AttachListingKey,
                            dbItem.StyleItemId,
                            dbItem.Market + ":" + dbItem.MarketplaceId,
                            styleItem.StyleItemId,
                            dbItem.Id.ToString() + ":" + dbItem.ASIN,
                            null);

                        dbItem.StyleId = styleItem.StyleId;
                        dbItem.StyleItemId = styleItem.StyleItemId;
                    }
                    else
                    {
                        if (!dbItem.StyleId.HasValue)
                            dbItem.StyleId = styleItem.StyleId;
                    }

                    _log.Debug(String.Format("Set for ASIN={0}, styleId={1}, styleItemId={2}",
                        dtoItem.ASIN,
                        styleItem.StyleId,
                        styleItem.StyleItemId));
                }
                else
                {
                    _log.Info("Empty StyleItem");
                }
            }

            //Update style image
            if (dbItem.StyleId.HasValue
                && !String.IsNullOrEmpty(dbItem.PrimaryImage))
            {
                _styleManager.UpdateStyleImageIfEmpty(db, dbItem.StyleId.Value, dbItem.PrimaryImage);
            }

            if (dtoItem.Weight.HasValue
                && dbItem.StyleItemId.HasValue)
            {
                var dbStyleItem = db.StyleItems.Get(dbItem.StyleItemId.Value);
                if (!dbStyleItem.Weight.HasValue)
                    dbStyleItem.Weight = dtoItem.Weight;
            }

            if (!String.IsNullOrEmpty(dtoItem.Barcode)
                && dbItem.StyleItemId.HasValue)
            {
                _styleManager.StoreOrUpdateBarcode(db,
                    dbItem.StyleItemId.Value,
                    dtoItem.Barcode);
            }
        }
        
        public void ProcessNewListingsWithItems(IUnitOfWork db, IMarketApi api, ITime time, IList<ItemDTO> items)
        {
            _log.Debug("Begin process new items");

            foreach (var dtoItem in items)
            {
                try
                {
                    if (dtoItem.IsExistOnAmazon == true)
                    {
                        _syncInfo.AddSuccess(dtoItem.ASIN, "New listing item was filled by Amazon");

                        if (String.IsNullOrEmpty(dtoItem.ParentASIN))
                            _syncInfo.AddWarning(dtoItem.ASIN, "Empty ParentASIN");

                        dtoItem.IsAmazonParentASIN = !String.IsNullOrEmpty(dtoItem.ParentASIN);
                        dtoItem.LastUpdateFromAmazon = time.GetUtcTime();
                        //Add new item, no need to additional check
                        dtoItem.StyleString = SkuHelper.RetrieveStyleIdFromSKU(db,
                            dtoItem.SKU, 
                            dtoItem.Name);
                        

                        var dbItem = db.Items.StoreItemIfNotExist(_itemHistoryService,
                            "ListingLineProcessing",
                            dtoItem, 
                            api.Market, 
                            api.MarketplaceId, 
                            _companyId,
                            time.GetAppNowTime());

                        //NOTE: fresh size (for some reason can start came emtpy)
                        dtoItem.Size = dbItem.Size;

                        if (!dbItem.StyleItemId.HasValue)
                        {
                            //Keep exists styleId
                            dtoItem.StyleId = dbItem.StyleId;

                            var styleItem = FindOrCreateStyleItem(db, dtoItem);
                            if (styleItem != null)
                            {
                                if (styleItem.StyleItemId > 0)
                                {
                                    dbItem.StyleId = styleItem.StyleId;
                                    dbItem.StyleItemId = styleItem.StyleItemId;
                                }
                                else
                                {
                                    if (!dbItem.StyleId.HasValue)
                                        dbItem.StyleId = styleItem.StyleId;
                                }
                                _log.Debug(String.Format("Set for ASIN={0}, styleId={1}, styleItemId={2}",
                                    dtoItem.ASIN,
                                    styleItem.StyleId,
                                    styleItem.StyleItemId));
                            }
                            db.Commit();
                        }

                        if (!String.IsNullOrEmpty(dtoItem.Barcode)
                            && dbItem.StyleItemId.HasValue)
                        {
                            _styleManager.StoreOrUpdateBarcode(db,
                                dbItem.StyleItemId.Value,
                                dtoItem.Barcode);
                        }

                        var dbListing = db.Listings.StoreOrUpdate(dtoItem, 
                            dbItem, 
                            api.Market, 
                            api.MarketplaceId, 
                            time.GetAppNowTime());

                        dtoItem.IsDefault = dbListing.IsDefault;

                        _syncInfo.AddSuccess(dtoItem.ASIN, "New listing item was stored");
                        _log.Debug("Store item:" + dbItem.ASIN + ", parentASIN=" + dbItem.ParentASIN + ", SKU=" + dtoItem.SKU + ", StyleString=" + dbItem.StyleString + ", quantity=" + dtoItem.RealQuantity);
                    }
                    else
                    {
                        _syncInfo.AddWarning(dtoItem.ASIN, "Item is not filled by Amazon (new listing item)");
                        _log.Warn("Item is not filled by Amazon (new listing item), item=" + dtoItem.ASIN);
                    }
                }
                catch (Exception ex)
                {
                    _syncInfo.AddError(dtoItem.ASIN, "Error while creating item", ex);
                    _log.Error(string.Format("Error while creating item, asin={0}", dtoItem.ASIN), ex);
                }
            }
            _log.Debug("End process new items");
        }

        private StyleItemDTO FindOrCreateStyleItem(IUnitOfWork db, ItemDTO dtoItem)
        {
            var styleItem = _styleManager.TryGetStyleItemIdFromOtherMarkets(db, dtoItem);
            if (styleItem == null)
            {
                //NOTE: a lot of listings in MX marketplace have incorrect sizes. Prevent to create incorrect style sizes
                var canCreateItem = _canCreateStyleInfo && dtoItem.MarketplaceId != MarketplaceKeeper.AmazonMxMarketplaceId; 
                
                styleItem = _styleManager.FindOrCreateStyleAndStyleItemForItem(db, 
                    ItemType.Pajama,
                    dtoItem,
                    canCreateItem,
                    false);
            }
            return styleItem;
        }

        public void ProcessNewParents(IUnitOfWork db, 
            IMarketApi api, 
            IList<ItemDTO> parents, 
            IList<ItemDTO> items)
        {
            var parentASINsWithError = new List<string>();

            _log.Debug("Begin process new parents, count=" + parents.Count);
            var parentsDto = api.GetItems(_log, 
                _time,
                MarketItemFilters.Build(parents.Select(p => p.ParentASIN).ToList()), 
                ItemFillMode.NoAdv, 
                out parentASINsWithError).ToList();

            _log.Debug("Error when GetItems, parentASINS: " + String.Join(", ", parentASINsWithError));

            //Fill fake parentItem with item info
            var notAmazonUpdatedParents = parentsDto.Where(p => p.IsAmazonUpdated != true).ToList();
            foreach (var parent in notAmazonUpdatedParents)
            {
                _log.Warn("Parent item is not filled by Amazon (setting image from first item), ParentASIN=" + parent.ASIN);
                _syncInfo.AddWarning(parent.ASIN, "Parent item is not filled by Amazon (setting image from first item)");
                
                //NOTE: parent EQUAL to first item (contains the same info)
                var item = items.FirstOrDefault(r => r.ParentASIN == parent.ASIN);
                if (item != null)
                {
                    parent.ImageSource = item.ImageUrl;
                    parent.AmazonName = item.Name;
                }
            }

            foreach (var parent in parentsDto)
            {
                //NOTE: prepare ParentSKU
                if (String.IsNullOrEmpty(parent.SKU))
                {
                    var mainStyleString = db.Items.GetAllViewAsDto()
                        .Where(pi => pi.ASIN == parent.ASIN
                            && pi.Market == (int)api.Market
                            && pi.MarketplaceId == api.MarketplaceId)
                        .OrderByDescending(i => i.IsDefault)
                        .ThenBy(i => i.Id)
                        .FirstOrDefault()?.StyleString;
                    parent.SKU = StringHelper.JoinTwo("-", mainStyleString, parent.ASIN);
                }

                var dbParent = db.ParentItems.CreateOrUpdateParent(parent, api.Market, api.MarketplaceId, _time.GetAppNowTime());
                
                _log.Warn("Parent item was stored, ParentASIN=" + parent.ASIN);
                _syncInfo.AddSuccess(parent.ASIN, "Parent item was stored");
            }
            _log.Debug("End process new parents");
        }

        [Obsolete("Change to UpdateParentsForExistantItems")]
        public void ProcessExistingParents(IUnitOfWork db,
            IMarketApi api,
            ISyncInformer syncInfo,
            IList<string> parentASINs,
            IList<ItemDTO> items)
        {
            var parentASINsWithError = new List<string>();

            _log.Debug("Begin process existing parents, count=" + parentASINs.Count);
            var parentsDto = api.GetItems(_log, 
                _time,
                MarketItemFilters.Build(parentASINs), 
                ItemFillMode.NoAdv,
                out parentASINsWithError).ToList();
            _log.Debug("Error when GetItems, parentASINS: " + String.Join(", ", parentASINsWithError));

            //Only update fields
            foreach (var parent in parentsDto)
            {
                //NOTE: in case when parent item has "no-img" using child image
                if (String.IsNullOrEmpty(parent.ImageSource))
                {
                    var childImage = items.FirstOrDefault(i => i.ParentASIN == parent.ASIN && !String.IsNullOrEmpty(i.ImageUrl));
                    if (childImage != null)
                        parent.ImageSource = childImage.ImageUrl;
                }

                var dbParent = db.ParentItems.CreateOrUpdateParent(parent,
                    api.Market,
                    api.MarketplaceId,
                    _time.GetAppNowTime());
                
                _log.Warn("Parent item was updated, ParentASIN=" + parent.ASIN);
                syncInfo.AddSuccess(parent.ASIN, "Parent item was updated");
            }
            _log.Debug("End process existing parents");
        }

        public void UpdateParentsForExistantItems(IUnitOfWork db,
           IMarketApi api,
           ISyncInformer syncInfo)
        {
            var parentASINsWithError = new List<string>();

            var allExistingItems = (from i in db.Items.GetAll()
                                   join l in db.Listings.GetAll() on i.Id equals l.ItemId
                                   where i.Market == (int)api.Market
                                        && (i.MarketplaceId == api.MarketplaceId || String.IsNullOrEmpty(api.MarketplaceId))
                                        && !l.IsRemoved
                                   select new
                                    {
                                        Id = i.Id,
                                        ParentASIN = i.ParentASIN,
                                        MarketParentASIN = i.MarketParentASIN,
                                        StyleString = i.StyleString,
                                        IsDefault = l.IsDefault,
                                    })
                                    .ToList();

            var parentASINs = allExistingItems
                .Where(i => !String.IsNullOrEmpty(i.ParentASIN))
                .Select(i => i.ParentASIN)
                .Distinct()
                .ToList();

            var marketParentASINs = allExistingItems
                .Where(i => !String.IsNullOrEmpty(i.MarketParentASIN))
                .Select(i => i.MarketParentASIN)
                .Distinct()
                .ToList();

            _log.Debug("Begin process existing parents, count=" + marketParentASINs.Count);
            var marketParentsDto = api.GetItems(_log,
               _time,
               MarketItemFilters.Build(marketParentASINs),
               ItemFillMode.NoAdv,
               out parentASINsWithError).ToList();
            _log.Debug("Error when GetItems, parentASINS: " + String.Join(", ", parentASINsWithError));

            foreach (var parentASIN in parentASINs)
            {
                var existingItems = allExistingItems.Where(i => i.ParentASIN == parentASIN).ToList();
                //Select actual ParentASIN info
                var parentASINCandidates = existingItems.GroupBy(i => i.MarketParentASIN)
                    .Select(i => new
                    {
                        MarketParentASIN = i.Key,
                        Count = i.Count()
                    })
                    .OrderByDescending(i => i.Count)
                    .ToList();

                var parentASINwinner = parentASINCandidates.FirstOrDefault(i => !String.IsNullOrEmpty(i.MarketParentASIN));
                if (parentASINwinner != null)
                {
                    var parentItemDto = marketParentsDto.FirstOrDefault(pi => pi.ASIN == parentASINwinner.MarketParentASIN);

                    //NOTE: TEMP prepare ParentSKU, For first updating Parent SKUs, afterward keep logic only for new
                    if (String.IsNullOrEmpty(parentItemDto.SKU))
                    {
                        var mainStyleString = existingItems.OrderByDescending(i => i.IsDefault).ThenBy(i => i.Id).FirstOrDefault()?.StyleString;
                        parentItemDto.SKU = StringHelper.JoinTwo("-", mainStyleString, parentItemDto.ASIN);
                    }

                    if (parentItemDto != null
                        && parentItemDto.IsAmazonUpdated == true)
                    {
                        if (parentASIN != parentItemDto.ASIN)
                        {
                            var existNewParent = db.ParentItems.GetByASIN(parentItemDto.ASIN, api.Market, api.MarketplaceId);

                            if (existNewParent == null)
                            {
                                db.ParentItems.UpdateParent(parentASIN, parentItemDto, api.Market, api.MarketplaceId, _time.GetAppNowTime());
                            }
                            else
                            {
                                var dbParentItem = db.ParentItems.GetByASIN(parentASIN, api.Market, api.MarketplaceId);
                                if (dbParentItem != null)
                                {
                                    db.ParentItems.Remove(dbParentItem);
                                    db.Commit();
                                }
                            }

                            //Update child items references
                            var childItemsToUpdate = db.Items.GetAll()
                                .Where(i => i.ParentASIN == parentASIN
                                    && i.Market == (int)api.Market
                                    && (i.MarketplaceId == api.MarketplaceId || String.IsNullOrEmpty(api.MarketplaceId)))
                                .ToList();
                            childItemsToUpdate.ForEach(i => i.ParentASIN = parentItemDto.ASIN);
                            db.Commit();

                            _log.Warn("Parent item was updated, ParentASIN=" + parentASIN + "=>" + parentItemDto.ASIN);
                        }
                        else
                        {
                            //NOTE: just update info or create missing parent item
                            db.ParentItems.CreateOrUpdateParent(parentItemDto, api.Market, api.MarketplaceId, _time.GetAppNowTime());
                        }
                    }
                    else
                    {
                        //NOTE: fill FAKE parentItem
                        if (parentItemDto.IsAmazonUpdated != true
                            && (String.IsNullOrEmpty(parentItemDto.AmazonName)
                            || String.IsNullOrEmpty(parentItemDto.ImageSource)))
                        {
                            var mainItemId = existingItems.OrderByDescending(i => i.IsDefault).FirstOrDefault()?.Id;
                            if (mainItemId.HasValue)
                            {
                                var mainItem = db.Items.Get(mainItemId.Value);                                
                                parentItemDto.AmazonName = mainItem?.Title;
                                parentItemDto.ImageSource = mainItem?.PrimaryImage;
                            }
                        }

                        if (parentItemDto != null)
                        {
                            db.ParentItems.CreateOrUpdateParent(parentItemDto, api.Market, api.MarketplaceId, _time.GetAppNowTime());
                        }
                    }
                }
            }

            _log.Debug("End process existing parents");
        }
    }
}
