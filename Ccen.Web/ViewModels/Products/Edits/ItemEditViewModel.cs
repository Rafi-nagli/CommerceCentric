using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Amazon.Api;
using Amazon.Api.Exports;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions;
using Amazon.DTO;
using Amazon.DTO.Listings;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Models;
using Amazon.Web.Models;
using Amazon.Web.Models.Exports;
using Amazon.Web.ViewModels.CustomBarcodes;
using Amazon.Web.ViewModels.Products.Edits;
using Amazon.Web.ViewModels.Results;
using Kendo.Mvc.Extensions;

namespace Amazon.Web.ViewModels.Products
{
    public class ItemEditViewModel
    {
        public long? Id { get; set; }
        public string ASIN { get; set; }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public bool ForceEnableColorVariations { get; set; }
        public bool OnHold { get; set; }
        public bool LockMarketUpdate { get; set; }

        public string SKU { get; set; }

        public bool IsAutoParentDesc { get; set; }
        public string Description { get; set; }
        public string BulletPoint1 { get; set; }
        public string BulletPoint2 { get; set; }
        public string BulletPoint3 { get; set; }
        public string BulletPoint4 { get; set; }
        public string BulletPoint5 { get; set; }

        public string Name { get; set; }
        public string Brand { get; set; }

        public string Image { get; set; }
        
        public string NewComment { get; set; }

        public IList<long> CategoryIdList { get; set; }

        public IList<ItemVariationEditViewModel> VariationList { get; set; }

        public DateTime CreateDate { get; set; }

        public ItemEditViewModel()
        {
            VariationList = new List<ItemVariationEditViewModel>();
        }

        public static ItemEditViewModel CreateFromStyleString(IUnitOfWork db, 
            IAutoCreateListingService autoCreateListingService,
            string styleString,
            MarketType market,
            string marketplaceId,
            out IList<MessageString> messages)
        {
            var parentItemDto = autoCreateListingService.CreateFromStyle(db,
                styleString,
                null,
                market,
                marketplaceId,
                out messages);
            
            var model = new ItemEditViewModel();

            model.ASIN = parentItemDto.ASIN;
            model.SKU = parentItemDto.SKU;

            model.Market = parentItemDto.Market;
            model.MarketplaceId = parentItemDto.MarketplaceId;

            model.Name = parentItemDto.AmazonName;
            
            model.VariationList = parentItemDto.Variations.Select(i => new ItemVariationEditViewModel(i, false))
                .OrderBy(si => SizeHelper.GetSizeIndex(si.StyleSize))
                .ThenBy(si => si.Color)
                .ToList();

            return model;
        }

        public static ItemEditViewModel Edit(IUnitOfWork db,
            string asin,
            int market,
            string marketplaceId)
        {
            var parentItem = db.ParentItems.GetAllAsDto().FirstOrDefault(s => s.ASIN == asin
                && s.Market == market
                && s.MarketplaceId == marketplaceId);

            var items = db.Items.GetAllViewAsDto().Where(i => i.ParentASIN == parentItem.ASIN
                                                           && i.Market == parentItem.Market
                                                           && i.MarketplaceId == parentItem.MarketplaceId)
                                                           .ToList();

            var model = new ItemEditViewModel();

            model.ASIN = parentItem.ASIN;
            model.SKU = parentItem.SKU;
            model.Id = parentItem.Id;
            model.OnHold = parentItem.OnHold;
            //model.LockMarketUpdate = parentItem.LockMarketUpdate;
            model.ForceEnableColorVariations = parentItem.ForceEnableColorVariations;
            
            model.Market = (int)market;
            model.MarketplaceId = marketplaceId;

            model.Name = parentItem.AmazonName;
            model.Brand = parentItem.BrandName;
            model.IsAutoParentDesc = parentItem.IsAutoParentDesc;
            model.Description = parentItem.Description;
            model.BulletPoint1 = parentItem.BulletPoint1;
            model.BulletPoint2 = parentItem.BulletPoint2;
            model.BulletPoint3 = parentItem.BulletPoint3;
            model.BulletPoint4 = parentItem.BulletPoint4;
            model.BulletPoint5 = parentItem.BulletPoint5;
            model.Image = parentItem.ManualImage;

            model.VariationList = items
                .OrderBy(i => i.StyleId)
                .ThenBy(i => SizeHelper.GetSizeIndex(i.StyleSize))
                .Select(i => new ItemVariationEditViewModel(i, true)).ToList();

            return model;
        }


        public static ItemEditViewModel CreateFromParentASIN(IUnitOfWork db, 
            IAutoCreateListingService autoCreateListingService,
            string asin,
            int market,
            string marketplaceId,
            bool includeAllChild,
            out IList<MessageString> messages)
        {
            var parentItemDto = autoCreateListingService.CreateFromParentASIN(db,
                asin,
                market,
                marketplaceId,
                includeAllChild,
                false,
                0,
                out messages);
            
            var model = new ItemEditViewModel();

            model.ASIN = parentItemDto.ASIN;
            model.SKU = parentItemDto.ASIN;
            
            model.Market = parentItemDto.Market;
            model.MarketplaceId = parentItemDto.MarketplaceId;

            model.Name = parentItemDto.AmazonName;

            model.VariationList = parentItemDto.Variations
                .OrderBy(i => i.StyleId)
                .ThenBy(i => SizeHelper.GetSizeIndex(i.StyleSize))
                .Select(i => new ItemVariationEditViewModel(i, false)).ToList();

            return model;
        }

        public void PrepareData()
        {
            if (String.IsNullOrEmpty(ASIN))
                ASIN = SKU;
            ASIN = StringHelper.TrimWhitespace(StringHelper.ToUpper(ASIN));
            Name = StringHelper.TrimWhitespace(Name);

            foreach (var item in VariationList)
            {
                item.Barcode = StringHelper.TrimWhitespace(item.Barcode);
                item.SKU = StringHelper.ToUpper(StringHelper.TrimWhitespace(item.SKU));

                item.Size = StringHelper.TrimWhitespace(item.Size);
                item.Color = StringHelper.TrimWhitespace(item.Color);

                if (String.IsNullOrEmpty(item.Size))
                    item.Size = item.StyleSize;
            }
        }

        public bool IsValid(IUnitOfWork db,
            out IList<MessageString> messages)
        {
            messages = new List<MessageString>();

            //NOTE: only when edit, if new product, system automatically added index
            if (Id != null && Id != 0)
            {
                //Check ASIN
                var existParent = db.ParentItems.GetAllAsDto().FirstOrDefault(pi => pi.ASIN == ASIN
                    && pi.Id != Id
                    && pi.Market == Market
                    && (pi.MarketplaceId == MarketplaceId || String.IsNullOrEmpty(MarketplaceId)));
                if (existParent != null)
                {
                    messages.Add(new MessageString()
                    {
                        Status = MessageStatus.Error,
                        Message = String.Format("Specified {0} already exists in the current marketplace",
                            MarketHelper.IsAmazon((MarketType)Market) ? "Parent ASIN" : "Parent SKU")
                    });
                    return false;
                }
            }

            var actualVariationList = VariationList.Where(v => v.IsSelected).ToList();

            //Check styleId/size combination
            if (Market != (int)MarketType.Amazon
                && Market != (int)MarketType.AmazonEU
                && Market != (int)MarketType.AmazonAU)
            {
                var duplicateStyleItemCombinations = actualVariationList.GroupBy(v => new { v.StyleString, v.StyleItemId }).Select(v =>
                      new
                      {
                          StyleString = v.Key.StyleString,
                          Size = v.Key.StyleItemId,
                          Count = v.Count(),
                      }).Where(v => v.Count > 1).ToList();
                var duplicateStyleItemCombinatioMessages = new List<MessageString>();
                duplicateStyleItemCombinations.ForEach(s => duplicateStyleItemCombinatioMessages.Add(new MessageString()
                {
                    Status = MessageStatus.Error,
                    Message = String.Format("The StyleId/Size combination: {0}/{1} is duplicated", s.StyleString, s.Size)
                }));
                messages.AddRange(duplicateStyleItemCombinatioMessages);

                //Check size/color combination
                var duplicateSizeColorCombinations = actualVariationList.GroupBy(v => new { v.Size, v.Color }).Select(v =>
                      new
                      {
                          Size = v.Key.Size,
                          Color = v.Key.Color,
                          Count = v.Count(),
                      }).Where(v => v.Count > 1).ToList();
                var duplicateSizeColorCombinatioMessages = new List<MessageString>();
                duplicateSizeColorCombinations.ForEach(s => duplicateSizeColorCombinatioMessages.Add(new MessageString()
                {
                    Status = MessageStatus.Error,
                    Message = String.Format("The Color/Size combination: {0}/{1} is duplicated", s.Size, s.Color)
                }));
                messages.AddRange(duplicateSizeColorCombinatioMessages);
            }

            //Check SKU
            var skuList = actualVariationList
                .Where(v => !String.IsNullOrEmpty(v.SKU))
                .Select(v => v.SKU)
                .ToList();

            var existSKUs = db.Listings.GetAll().Where(l => !l.IsRemoved
                                                            && skuList.Contains(l.SKU)
                                                            && l.Market == Market
                                                            && l.MarketplaceId == MarketplaceId).ToList();

            foreach (var item in actualVariationList)
            {
                if (!String.IsNullOrEmpty(item.SKU))
                {
                    var existItemSKU = existSKUs.Where(s => s.ItemId != item.Id && s.SKU == item.SKU).Take(1).ToList();

                    var skuMessages = new List<MessageString>();
                    existItemSKU.ForEach(s => skuMessages.Add(new MessageString()
                    {
                        Status = MessageStatus.Error,
                        Message = String.Format("The variation SKU: \"{0}\" already exists in current marketplace", s.SKU)
                    }));
                    messages.AddRange(skuMessages);
                }
            }

            //Check Barcode
            var barcodeList = actualVariationList.Where(v => !String.IsNullOrEmpty(v.Barcode)
                    && !v.AutoGeneratedBarcode)
                .Select(v => v.Barcode)
                .ToList();


            //NOTE: currently only check with other Parent ASINs (into one listing barcodes may have duplicates FBA/FBP/FBO)
            var existBarcodeItems = db.Items.GetAllViewActual().Where(l => barcodeList.Contains(l.Barcode)
                                                            && l.Market == Market
                                                            && l.MarketplaceId == MarketplaceId
                                                            && l.ParentASIN != ASIN).ToList();

            var removalItemIds = VariationList.Where(v => !v.IsSelected).Select(v => v.Id).ToList();
            foreach (var item in actualVariationList)
            {
                if (!String.IsNullOrEmpty(item.Barcode))
                {
                    var duplicateBarcodeItems = existBarcodeItems.Where(s => s.Id != item.Id //Exclude self
                        && !removalItemIds.Contains(s.Id) //Exclude removal items
                        && s.Barcode == item.Barcode).Take(1).ToList();
                    
                    var barcodeMessages = new List<MessageString>();
                    duplicateBarcodeItems.ForEach(s => barcodeMessages.Add(new MessageString()
                    {
                        Status = MessageStatus.Error,
                        Message = String.Format("The variation Barcode: \"{0}\" already exists in current marketplace", s.Barcode)
                    }));
                    messages.AddRange(barcodeMessages);
                }
            }

            var itemIds = actualVariationList.Select(i => i.Id).ToList();
            var existOnMarketItemIds = db.Items.GetAll()
                .Where(i => itemIds.Contains(i.Id) && i.IsExistOnAmazon == true)
                .Select(i => i.Id).ToList();
            foreach (var item in actualVariationList)
            {
                if (!(item.Price > 0))
                    messages.Add(new MessageString()
                    {
                        Message = "\"" + item.Size + "\"" + " has invalid price",
                        Status = MessageStatus.Error
                    });
                if (!item.AutoGeneratedBarcode 
                    && String.IsNullOrEmpty(item.Barcode) 
                    && Market != (int)MarketType.Magento
                    && !(Market == (int)MarketType.Amazon && item.Id.HasValue && existOnMarketItemIds.Contains(item.Id.Value)))
                    messages.Add(new MessageString()
                    {
                        Message = "\"" + item.Size + "\"" + " hasn't barcode",
                        Status = MessageStatus.Error
                    });
            }

            return !messages.Any();
        }

        public static StyleVariationListViewModel CreateStyleVariations(IUnitOfWork db,
            ILogService log,
            string styleString,
            IList<ItemSizeMapping> existSizeMappings,
            string walmartUrl,
            MarketType market,
            string marketplaceId,
            IList<long> excludeStyleItemIds)
        {
            var items = new List<ItemVariationEditViewModel>();
            var walmartLookupResults = new List<MessageString>();

            var style = db.Styles.GetActiveByStyleIdAsDto(styleString);
            if (style == null)
                return new StyleVariationListViewModel() {Variations = items};

            var styleItems = db.StyleItems
                .GetByStyleIdWithBarcodesAsDto(style.Id)
                .OrderBy(o => SizeHelper.GetSizeIndex(o.Size))
                .ToList();
            
            var forceReplace = styleItems.Any(s => (s.Size ?? "").Contains("/"));

            IList<MarketBarcodeViewModel> walmartBarcodes = null;
            if (!String.IsNullOrEmpty(walmartUrl))
            {
                walmartBarcodes = MarketBarcodeViewModel.SearchBarcodes(log, StringHelper.TrimWhitespace(walmartUrl));
                if (walmartBarcodes == null || !walmartBarcodes.Any())
                    walmartLookupResults.Add(MessageString.Error("Unable to extract barcodes from the provided url"));
                else
                    walmartLookupResults.Add(MessageString.Info("The following barcodes were extracted: <br/>" 
                        + String.Join("<br/>", walmartBarcodes.Select(b => " -" + StringHelper.JoinTwo("/", b.Size, b.Color) + ": " + b.Barcode))));
            }

            var usedWalmartBarcodes = new List<string>();
            foreach (var styleItem in styleItems)
            {
                if (excludeStyleItemIds != null 
                    && excludeStyleItemIds.Contains(styleItem.StyleItemId))
                    continue;

                var newItem = new ItemVariationEditViewModel();

                var index = 0;
                var baseSKU = style.StyleID + "-" + SizeHelper.PrepareSizeForSKU(styleItem.Size, forceReplace);

                while (db.Listings.CheckForExistenceSKU(SkuHelper.SetSKUMiddleIndex(baseSKU, index),
                    (MarketType)market,
                    marketplaceId))
                    index++;

                newItem.IsSelected = true;
                newItem.SKU = SkuHelper.SetSKUMiddleIndex(baseSKU, index);
                newItem.StyleId = styleItem.StyleId;
                newItem.StyleString = style.StyleID;
                newItem.StyleItemId = styleItem.StyleItemId;
                newItem.StyleColor = styleItem.Color;
                newItem.StyleSize = styleItem.Size;
                newItem.Size = newItem.StyleSize;

                //NOTE: looking more suitable item size value
                var existMapping = existSizeMappings.FirstOrDefault(m => m.StyleSize == newItem.StyleSize);
                if (existMapping != null)
                {
                    newItem.Size = existMapping.ItemSize;
                }
                else
                {
                    //Getting all item sizes mapping for that style size, if any already used choose it
                    var sizeMappings = db.SizeMappings.GetAllAsDto().Where(s => s.StyleSize == newItem.StyleSize).ToList();
                    var existItemSizes = existSizeMappings.Select(s => s.ItemSize).ToList();
                    foreach (var sizeMapping in sizeMappings)
                    {
                        if (existItemSizes.Contains(sizeMapping.ItemSize))
                            newItem.Size = sizeMapping.ItemSize;
                    }
                }

                if (!String.IsNullOrEmpty(walmartUrl))
                {
                    if (walmartBarcodes != null)
                    {
                        var possibleStyleItemSizes = db.SizeMappings
                            .GetAllAsDto()
                            .Where(s => s.StyleSize == newItem.StyleSize)
                            .OrderBy(s => s.Priority)
                            .Select(s => s.ItemSize)
                            .ToList();
                        possibleStyleItemSizes.Insert(0, newItem.StyleSize);
                        possibleStyleItemSizes = possibleStyleItemSizes.Select(s => s.ToLower()).ToList();

                        var wmBarcode = walmartBarcodes.FirstOrDefault(b => possibleStyleItemSizes.Contains((b.Size ?? "").ToLower()));
                        if (wmBarcode != null)
                        {
                            usedWalmartBarcodes.Add(wmBarcode.Barcode);
                            newItem.Barcode = wmBarcode.Barcode;
                        }
                    }
                }
                else
                {
                    if (market == MarketType.Walmart || market == MarketType.WalmartCA)
                    {
                        newItem.AutoGeneratedBarcode = true;
                    }
                    else
                    {
                        if (styleItem.Barcodes != null)
                        {
                            foreach (var barcode in styleItem.Barcodes)
                            {
                                if (!String.IsNullOrEmpty(barcode.Barcode)
                                    && !db.Items.CheckForExistenceBarcode(barcode.Barcode, market, marketplaceId))
                                {
                                    newItem.Barcode = barcode.Barcode;
                                    break;
                                }
                            }
                        }
                        if (String.IsNullOrEmpty(newItem.Barcode))
                            newItem.AutoGeneratedBarcode = true;
                    }
                }

                items.Add(newItem);
            }

            if (usedWalmartBarcodes.Any())
                walmartLookupResults.Add(MessageString.Success("The following barcodes were applied: " + String.Join(", ", usedWalmartBarcodes)));
            else if (walmartBarcodes != null && walmartBarcodes.Any())
                walmartLookupResults.Add(MessageString.Error("Barcodes haven't been applied. Matching with style sizes was not found."));

            var model = new StyleVariationListViewModel()
            {
                Name = style.Name,
                StyleString = style.StyleID,
                Variations = items,
                WalmartLookupMessages = walmartLookupResults
            };

            return model;
        }

        public static List<ItemVariationEditViewModel> GetMissingSizes(IUnitOfWork db,
            ILogService log,
            IList<ItemSizeMapping> existSizeMappings,
            MarketType market,
            string marketplaceId)
        {
            var results = new List<ItemVariationEditViewModel>();

            var existStyleItemIdList = existSizeMappings.Select(sm => sm.StyleItemId).ToList();
            var styleItemList = db.StyleItems.GetAllAsDto().Where(si => existStyleItemIdList.Contains(si.StyleItemId)).ToList();
            var styleIdList = styleItemList.Select(s => s.StyleId).ToList();
            var styleList = db.Styles.GetAllActiveAsDto().Where(s => styleIdList.Contains(s.Id)).ToList();

            foreach (var style in styleList)
            {
                var styleVariations = CreateStyleVariations(db,
                    log,
                    style.StyleID,
                    existSizeMappings,
                    null,
                    market,
                    marketplaceId,
                    existStyleItemIdList);

                if (styleVariations != null && styleVariations.Variations != null)
                    results.AddRange(styleVariations.Variations);
            }

            return results;
        }

        public static string GetUnusedBarcodeForStyleItem(IUnitOfWork db, 
            long styleItemId,
            MarketType market,
            string marketplaceId)
        {
            var barcodes = db.StyleItemBarcodes.GetByStyleItemId(styleItemId);
            if (barcodes != null)
            {
                foreach (var barcode in barcodes)
                {
                    if (!String.IsNullOrEmpty(barcode.Barcode)
                        && !db.Items.CheckForExistenceBarcode(barcode.Barcode, market, marketplaceId))
                    {
                        return barcode.Barcode;
                    }
                }
            }
            return null;
        }

        public static void Delete(IUnitOfWork db,
            int id,
            DateTime when,
            long? by,
            out IList<MessageString> messages)
        {
            var dbParentItem = db.ParentItems.Get(id);
            var dbItems = db.Items.GetAll().Where(i => i.ParentASIN == dbParentItem.ASIN
                                                       && i.Market == dbParentItem.Market
                                                       && (String.IsNullOrEmpty(dbParentItem.MarketplaceId)
                                                            || i.MarketplaceId == dbParentItem.MarketplaceId)).ToList();

            var itemIdList = dbItems.Select(i => i.Id).ToList();
            var dbListings = db.Listings.GetAll().Where(l => itemIdList.Contains(l.ItemId)).ToList();

            foreach (var dbListing in dbListings)
            {
                dbListing.IsRemoved = true;
            }
            db.Commit();

            db.ParentItems.Remove(dbParentItem);
            db.Commit();

            messages = new List<MessageString>();
        }

        public static void SendProductUpdate(IUnitOfWork db,
            ISystemActionService actionService,
            int parentItemId,
            DateTime when,
            long? by)
        {
            var dbParentItem = db.ParentItems.Get(parentItemId);
            var dbItems = db.Items.GetAll().Where(i => i.ParentASIN == dbParentItem.ASIN
                && i.Market == dbParentItem.Market
                && i.MarketplaceId == dbParentItem.MarketplaceId)
                .ToList();
            var itemIds = dbItems.Select(i => (long)i.Id).ToList();
            AddActions(db, itemIds, when, by);
        }

        public static ItemEditViewModel Clone(IUnitOfWork db,
            ICacheService cache,
            IBarcodeService barcodeService,
            ISystemActionService actionService,
            IAutoCreateListingService autoCreateListingService,
            IItemHistoryService itemHistoryService,
            int id, 
            DateTime when,
            long? by,
            string newComment,
            out IList<MessageString> messages)
        {
            var parent = db.ParentItems.GetAsDTO(id);

            var model = ItemEditViewModel.CreateFromParentASIN(db,
                autoCreateListingService,
                parent.ASIN,
                parent.Market,
                parent.MarketplaceId,
                false,
                out messages);

            model.Id = null;
            //model.OnHold = true;

            var parentBaseASIN = SkuHelper.RemoveSKULastIndex(model.ASIN);
            var parentIndex = 1;
            while (db.ParentItems.GetAsDTO(parentBaseASIN + "-" + parentIndex, (MarketType)parent.Market, parent.MarketplaceId) != null)
                parentIndex++;
            var parentSKU = parentBaseASIN + "-" + parentIndex;

            var forceReplace = model.VariationList.Any(s => (s.Size ?? "").Contains("/"));

            model.ASIN = parentSKU;
            model.NewComment = newComment;

            foreach (var item in model.VariationList)
            {
                item.Id = null;
                item.Barcode = null;
                item.AutoGeneratedBarcode = true;
                
                var baseSKU = item.StyleString + "-" + SizeHelper.PrepareSizeForSKU(item.StyleSize, forceReplace);
                var index = parentIndex;
                
                while (db.Listings.CheckForExistenceSKU(SkuHelper.SetSKUMiddleIndex(baseSKU, index),
                    (MarketType)parent.Market,
                    parent.MarketplaceId))
                    index++;

                item.SKU = SkuHelper.SetSKUMiddleIndex(baseSKU, index);
            }

            model.Save(db,
                        cache,
                        barcodeService,
                        actionService,
                        itemHistoryService,
                        when,
                        by);

            return model;
        }

        private class StyleIsRequreBrandInfo
        {
            public long StyleItemId { get; set; }
            public long StyleId { get; set; }
            public string StyleString { get; set; }
            public bool IsRequiredManufactureBarcode { get; set; }
        }


        public List<MessageString> ValidateAsync(IUnitOfWork db,
           ILogService log,
           DateTime when)
        {
            var messages = new List<MessageString>();

            //Validate size names
            var notValidSizes = new List<string>();
            foreach (var size in VariationList.Where(s => !String.IsNullOrEmpty(s.Size)))
            {
                var validSizeName = false;

                var existSizeMapping = db.SizeMappings.GetAllAsDto().FirstOrDefault(s => s.ItemSize == size.Size
                                                                  || s.StyleSize == size.Size);
                if (existSizeMapping == null)
                {
                    var existSize = db.Sizes.GetAllAsDto().FirstOrDefault(s => s.Name == size.Size);
                    validSizeName = existSize != null;
                }
                else
                {
                    validSizeName = true;
                }

                if (!validSizeName)
                {
                    notValidSizes.Add(size.Size);
                }
            }

            if (notValidSizes.Any())
            {
                messages.Add(new MessageString()
                {
                    Message = "The following size names may be incorrect: " + String.Join(", ", notValidSizes) + " . Are you sure you would like to save these changes?",
                    Status = MessageStatus.Info,
                });
            }

            //Validate barcodes
            if (Market == (int)MarketType.Amazon
                || Market == (int)MarketType.AmazonEU
                || Market == (int)MarketType.AmazonAU)
            {
                var formattedBrand = StringHelper.TrimWhitespace(Brand);
                var styleItemIdList = VariationList.Select(v => v.StyleItemId).ToList();

                var requireManufactureByStyle = (from st in db.Styles.GetAll()
                                                 join si in db.StyleItems.GetAll() on st.Id equals si.StyleId
                                                 join sfv in db.StyleFeatureValues.GetAll() on st.Id equals sfv.StyleId
                                                 join fv in db.FeatureValues.GetAll() on sfv.FeatureValueId equals fv.Id
                                                 where styleItemIdList.Contains(si.Id)
                                                    && fv.FeatureId == StyleFeatureHelper.MAIN_LICENSE
                                                 select new StyleIsRequreBrandInfo
                                                 {
                                                     StyleItemId = si.Id,
                                                     StyleId = st.Id,
                                                     StyleString = st.StyleID,
                                                     IsRequiredManufactureBarcode = fv.IsRequiredManufactureBarcode
                                                 }).ToList();

                if (!String.IsNullOrEmpty(Brand))
                {
                    var customBrand = (from fv in db.FeatureValues.GetAll()
                                      where fv.FeatureId == StyleFeatureHelper.MAIN_LICENSE
                                        && fv.Value == formattedBrand
                                      select fv).FirstOrDefault();

                    requireManufactureByStyle.ForEach(fv => fv.IsRequiredManufactureBarcode = (customBrand?.IsRequiredManufactureBarcode ?? false));
                }


                foreach (var size in VariationList)
                {
                    var styleInfo = requireManufactureByStyle.FirstOrDefault(st => st.StyleItemId == size.StyleItemId);
                    if (styleInfo != null
                        && styleInfo.IsRequiredManufactureBarcode
                        && (size.AutoGeneratedBarcode
                            || BarcodeHelper.IsCustomGenerated(size.Barcode)))
                    {
                        messages.Add(new MessageString()
                        {
                            Message = styleInfo.StyleString + " / " + size.StyleSize + " - required the manufacture barcode",
                            Status = MessageStatus.Error,
                        });
                    }
                }
            }

            return messages;
        }

        public void Save(IUnitOfWork db,
            ICacheService cacheService,
            IBarcodeService barcodeService,
            ISystemActionService actionService,
            IItemHistoryService itemHistoryService,
            DateTime when, 
            long? by)
        {
            //Prepare StyleId
            foreach (var item in VariationList.Where(v => v.IsSelected))
            {
                var style = db.Styles.GetAllAsDto().FirstOrDefault(s => !s.Deleted && s.StyleID == item.StyleString);
                item.StyleId = style != null ? style.Id : (long?)null;
            }

            ApplyProduct(db, cacheService, barcodeService, itemHistoryService, when, by);

            if (VariationList != null)
            {
                if (!MarketHelper.IsAmazon((MarketType)Market))
                    AddActions(db, VariationList.Where(v => v.IsSelected).Select(v => (long)v.Id).ToList(), when, by);
            }

            if (Id.HasValue && !String.IsNullOrEmpty(NewComment))
            {
                db.ProductComments.Add(new ProductComment()
                {
                    ProductId = (int) Id.Value,
                    Message = NewComment,
                    CreatedBy = by,
                    CreateDate = when,
                });
                db.Commit();
            }
        }

        private static void AddActions(IUnitOfWork db, IList<long> itemIds, DateTime when, long? by)
        {
            var actions = new List<SystemActionType>()
            {
                SystemActionType.UpdateOnMarketProductData,
                SystemActionType.UpdateOnMarketProductRelationship,
                SystemActionType.UpdateOnMarketProductImage,
            };

            foreach (var itemId in itemIds)
            {
                foreach (var actionType in actions)
                {
                    var newAction = new SystemActionDTO()
                    {
                        ParentId = null,
                        Status = (int)SystemActionStatus.None,
                        Type = (int)actionType,
                        Tag = itemId.ToString(),
                        InputData = null,

                        CreateDate = when,
                        CreatedBy = by,
                    };
                    db.SystemActions.AddAction(newAction);
                }
            }
            db.Commit();
        }

        //public void CreateProduct(IUnitOfWork db,
        //    ICacheService cacheService,
        //    IBarcodeService barcodeService,
        //    DateTime when, 
        //    long? by)
        //{

        //    var parentASIN = GetUnusedParentASIN(db, ASIN, Market, MarketplaceId);

        //    var parentItem = new ParentItem()
        //    {
        //        ASIN = parentASIN,
        //        Market = Market,
        //        MarketplaceId = MarketplaceId,

        //        ForceEnableColorVariations = ForceEnableColorVariations,
        //        OnHold = OnHold,
        //        LockMarketUpdate = LockMarketUpdate,

        //        SKU = ASIN,
        //        AmazonName = Name,
        //        BrandName = Brand,
        //        IsAutoParentDesc = IsAutoParentDesc,
        //        Description = Description,
        //        BulletPoint1 = BulletPoint1,
        //        BulletPoint2 = BulletPoint2,
        //        BulletPoint3 = BulletPoint3,
        //        BulletPoint4 = BulletPoint4,
        //        BulletPoint5 = BulletPoint5,
        //        ManualImage = Image,
                
        //        CreateDate = when,
        //        CreatedBy = by
        //    };

        //    db.ParentItems.Add(parentItem);
        //    db.Commit();

        //    Id = parentItem.Id;

        //    var variationDtoList = VariationList
        //        .Where(v => v.IsSelected)
        //        .Select(v => new ItemDTO()
        //    {
        //        ParentASIN = parentItem.ASIN,
        //        Name = Name, //Equal parent name

        //        StyleString = v.StyleString,
        //        StyleId = v.StyleId,
        //        StyleItemId = v.StyleItemId,
        //        Size = v.Size,
        //        Color = v.Color,

        //        ASIN = v.SKU,
        //        SKU = v.SKU,
        //        IsDefault = v.IsDefault,
        //        ListingId = v.SKU,

        //        Barcode = v.AutoGeneratedBarcode ? BarcodeHelper.GenerateBarcode(barcodeService, v.SKU, when) : v.Barcode,

        //        CurrentPrice = v.Price,
        //        RealQuantity = 0,

        //        PublishedStatus = (int)PublishedStatuses.New,
        //    }).ToList();

        //    db.Items.UpdateItemsForParentItem(parentItem.ASIN,
        //        parentItem.Market,
        //        parentItem.MarketplaceId,
        //        variationDtoList,
        //        when,
        //        by);

        //    foreach (var variation in VariationList)
        //    {
        //        if (variation.Id.HasValue)
        //    }

        //    cacheService.RequestParentItemIdUpdates(db, 
        //        new List<long>() { parentItem.Id }, 
        //        UpdateCacheMode.IncludeChild, 
        //        by);
        //}

        public void ApplyProduct(IUnitOfWork db,
            ICacheService cacheService,
            IBarcodeService barcodeService,
            IItemHistoryService itemHistoryService,
            DateTime when,
            long? by)
        {
            var isNew = !Id.HasValue || Id == 0;
            if (String.IsNullOrEmpty(ASIN))
                ASIN = SKU;

            if (String.IsNullOrEmpty(SKU))
            {
                if (isNew)
                {
                    if (String.IsNullOrEmpty(ASIN))
                    {
                        ASIN = VariationList
                            .Where(v => v.IsSelected)
                            .OrderByDescending(v => v.IsDefault)
                            .ThenBy(v => v.Id)
                            .FirstOrDefault()?.StyleString;
                    }
                    SKU = StringHelper.JoinTwo("-", ASIN, "PARENT");
                }
                else
                {
                    var mainVariationStyleString = VariationList
                        .Where(v => v.IsSelected)
                        .OrderByDescending(v => v.IsDefault)
                        .ThenBy(v => v.Id)
                        .FirstOrDefault()?.StyleString;
                    SKU = StringHelper.JoinTwo("-", mainVariationStyleString, ASIN);
                }
            }            
                        
            ParentItem parentItem = null;
            string previousASIN = String.Empty;
            if (!isNew)
            {
                parentItem = db.ParentItems.Get(Id.Value);
                previousASIN = parentItem.ASIN;

                parentItem.ASIN = ASIN; 
                parentItem.SKU = SKU;

                parentItem.ForceEnableColorVariations = ForceEnableColorVariations;
                parentItem.OnHold = OnHold;
                //parentItem.LockMarketUpdate = LockMarketUpdate;
                parentItem.AmazonName = Name;
                parentItem.BrandName = Brand;

                parentItem.IsAutoParentDesc = IsAutoParentDesc;
                parentItem.Description = Description;
                parentItem.BulletPoint1 = BulletPoint1;
                parentItem.BulletPoint2 = BulletPoint2;
                parentItem.BulletPoint3 = BulletPoint3;
                parentItem.BulletPoint4 = BulletPoint4;
                parentItem.BulletPoint5 = BulletPoint5;
                parentItem.ManualImage = Image;

                parentItem.UpdateDate = when;
                parentItem.UpdatedBy = by;

                db.Commit();
            }
            else
            {
                var parentASIN = GetUnusedParentASIN(db, ASIN, Market, MarketplaceId);
                previousASIN = parentASIN;

                parentItem = new ParentItem()
                {
                    ASIN = parentASIN,
                    Market = Market,
                    MarketplaceId = MarketplaceId,

                    ForceEnableColorVariations = ForceEnableColorVariations,
                    OnHold = OnHold,
                    LockMarketUpdate = LockMarketUpdate,

                    SKU = SKU,
                    AmazonName = Name,
                    BrandName = Brand,
                    IsAutoParentDesc = IsAutoParentDesc,
                    Description = Description,
                    BulletPoint1 = BulletPoint1,
                    BulletPoint2 = BulletPoint2,
                    BulletPoint3 = BulletPoint3,
                    BulletPoint4 = BulletPoint4,
                    BulletPoint5 = BulletPoint5,
                    ManualImage = Image,

                    CreateDate = when,
                    CreatedBy = by
                };

                db.ParentItems.Add(parentItem);
                db.Commit();

                Id = parentItem.Id;
            }

            var variationDtoList = VariationList
                .Where(v => v.IsSelected)
                .Select(v => new ItemDTO()
            {
                ParentASIN = parentItem.ASIN,
                Name = Name,

                Id = v.Id ?? 0,
                ListingEntityId = v.ListingEntityId,
                StyleString = v.StyleString,
                StyleId = v.StyleId,
                StyleItemId = v.StyleItemId,
                Size = v.Size,
                Color = v.Color,

                ASIN = v.SKU,
                SKU = v.SKU,
                IsDefault = v.IsDefault,
                ListingId = v.SKU,
                               
                Barcode = v.AutoGeneratedBarcode ? BarcodeHelper.GenerateBarcode(barcodeService, v.SKU, when) : v.Barcode,

                RealQuantity = v.RealQuantity,
                
                CurrentPrice = v.Price,
                IsPrime = v.IsPrime,

                PublishedStatus = isNew ? 
                    (int)PublishedStatuses.New :
                    (v.OverridePublishedStatus.HasValue && v.OverridePublishedStatus.Value != v.PublishedStatus ? v.OverridePublishedStatus.Value : (int)PublishedStatuses.None)
            }).ToList();

            db.Items.UpdateItemsForParentItem(itemHistoryService,
                "ItemEditViewModel.ApplyProduct",
                previousASIN,
                parentItem.Market,
                parentItem.MarketplaceId,
                variationDtoList,
                when,
                by);

            foreach (var variation in VariationList)
            {
                if (!variation.Id.HasValue)
                {
                    var updatedDto = variationDtoList.FirstOrDefault(v => v.SKU == variation.SKU);
                    if (updatedDto != null)
                    {
                        variation.Id = updatedDto.Id;
                    }
                }
            }

            cacheService.RequestParentItemIdUpdates(db,
                new List<long>() { parentItem.Id },
                UpdateCacheMode.IncludeChild,
                by);
        }

        private string GetUnusedParentASIN(IUnitOfWork db,
            string asin,
            int market,
            string marketplaceId)
        {
            string newASIN = asin;
            var index = 0;
            ParentItemDTO parentItem = null;
            do
            {
                newASIN = asin + (index > 0 ? "-" + index : "");
                index++;

                parentItem = db.ParentItems.GetAllAsDto().FirstOrDefault(pi => pi.ASIN == newASIN 
                    && pi.Market == market
                    && pi.MarketplaceId == marketplaceId);
            } while (parentItem != null);

            return newASIN;
        }

        public static CallResult<bool> GetAmazonBarcodeStatus(IMarketApi api, string barcode)
        {
            string html = null;
            try
            {
                var products = ((AmazonApi)api).GetProductForBarcode(new List<string> { barcode });
                if (products.Any())
                {
                    return CallResult<bool>.Success(true);
                }
                else
                {
                    return CallResult<bool>.Success(false);
                }
            }
            catch (Exception ex)
            {
                return CallResult<bool>.Fail(ex.Message, ex);
            }
        }
    }
}