using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Features;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.Core.Models.Items;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;
using RestSharp.Extensions;

namespace Amazon.Model.Implementation.Markets.Shopify
{
    public class ShopifyItemsImporter
    {
        public enum ImportModes
        {
            Full = 0,
            OnlyRecent = 1,
        }

        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private IStyleHistoryService _styleHistoryService;
        private IList<string> _skusBlackList;

        public ShopifyItemsImporter(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            IStyleHistoryService styleHistoryService,
            IList<string> skusBlackList)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _styleHistoryService = styleHistoryService;
            _skusBlackList = skusBlackList;
        }

        private IList<ParentItemDTO> DivideBySubStyle(IList<ParentItemDTO> items)
        {
            var results = new List<ParentItemDTO>();
            foreach (var item in items)
            {
                if (item.Variations != null
                    && item.Variations.Select(v => v.SubStyleVariation).Count() > 1)
                {
                    var itemGroups = item.Variations.GroupBy(oi => oi.SubStyleVariation, oi => oi)
                        .Select(g => g.ToList())
                        .ToList();
                    var subItems = new List<ParentItemDTO>();

                    foreach (var itemGroup in itemGroups)
                    {
                        var itemCopy = (ParentItemDTO)item.Clone();
                        itemCopy.Variations = itemGroup;
                        results.Add(itemCopy);
                        subItems.Add(itemCopy);
                    }

                    var forceEnableSubStyleVariation = subItems.Select(r => GetSimpleMainSKU(r))
                       .GroupBy(r => r)
                       .Select(r => r.Count())
                       .Any(r => r > 1); //Has duplicate SKUs

                    results.ForEach(r => r.ForceEnableSubStyleVariations = forceEnableSubStyleVariation);
                }
                else
                {
                    results.Add(item);
                }
            }

            return results;
        }

        private IList<ParentItemDTO> DivideByColor(IList<ParentItemDTO> items)
        {
            var results = new List<ParentItemDTO>();
            foreach (var item in items)
            {
                var dropShipperId = GetDropShipperId(item.SearchKeywords,
                    item.Type,
                    item.Department,
                    item.AmazonName);

                foreach (var v in item.Variations)
                {
                    v.Color = StringHelper.TrimWhitespace(v.Color) ?? "";
                }

                var colorGroups = item.Variations.GroupBy(v => v.Color, v => v)
                        .Select(g => g.ToList())
                        .ToList();

                var splitRequired = colorGroups.Count() > 1;
                if (colorGroups.Count(g => g.Count() > 1) <= 1) //NOTE: only one group with more than 1 variation
                    splitRequired = false;

                if (splitRequired)
                {
                    var subItems = new List<ParentItemDTO>();
                    foreach (var colorGroup in colorGroups)
                    {
                        var itemCopy = (ParentItemDTO)item.Clone();
                        itemCopy.Variations = colorGroup;
                        itemCopy.SKU = colorGroup.FirstOrDefault()?.SKU;
                        results.Add(itemCopy);
                        subItems.Add(itemCopy);
                    }

                    var forceEnableColorVariation = subItems.Select(r => GetSimpleMainSKU(r))
                        .GroupBy(r => r)
                        .Select(r => r.Count())
                        .Any(r => r > 1)
                        || results.Any(r => IsOverseasSKU(r.SKU)); //Has duplicate SKUs

                    results.ForEach(r => r.ForceEnableColorVariations = forceEnableColorVariation);
                }
                else
                {
                    results.Add(item);
                }
            }

            return results;
        }

        public void Import(IMarketApi api,
            IList<string> itemIdList,
            ImportModes mode,
            bool processOnlyNewStyles,
            bool overrideStyleIds,
            bool overrideStyleTitles,
            bool importDescription)
        {
            var allowRemoving = itemIdList == null && mode == ImportModes.Full;

            var asinsWithError = new List<string>();
            var fromDate = _time.GetAppNowTime().AddDays(-1);
            var items = api.GetItems(_log,
                _time,
                new MarketItemFilters()
                {
                    ASINList = itemIdList,
                    FromDate = mode == ImportModes.OnlyRecent ? fromDate : (DateTime?)null
                },
                ItemFillMode.Defualt,
                out asinsWithError);

            _log.Info("Source items count: " + items.Count());
            items = DivideBySubStyle(items.ToList());
            _log.Info("After by style count: " + items.Count());
            items = DivideByColor(items.ToList());
            _log.Info("After by color: " + items.Count());

            _log.Info("Results items count: " + items.Count());
            var index = 0;

            var updatedListingIds = new List<long>();
            var processOnlyStyle = true;

            items = items.OrderBy(i => i.SourceMarketId).ToList();

            foreach (var item in items)
            {
                _log.Info("item: " + index);
                CreateOrUpdateListing(_dbFactory,
                    _time,
                    _log,
                    _styleHistoryService,
                    api.Market,
                    api.MarketplaceId,
                    item,
                    canCreate: true,
                    updateQty: false,
                    updateStyleInfo: !processOnlyNewStyles,
                    mapByBarcode: true,
                    processOnlyNewStyles: processOnlyNewStyles,
                    overrideStyleIds: overrideStyleIds,
                    overrideStyleTitles: overrideStyleTitles,
                    importDescription: importDescription);

                updatedListingIds.AddRange(item.Variations.Where(v => v.ListingEntityId.HasValue).Select(v => v.ListingEntityId.Value).ToList());
                index++;
            }

            if (allowRemoving)
            {
                RemoveAllOutOfDateListings(_dbFactory,
                    api.Market,
                    api.MarketplaceId,
                    updatedListingIds);
            }
        }

        private string GetSimpleMainSKU(ParentItemDTO product, bool isMaxSimple = false)
        {
            var baseSKU = product.Variations.FirstOrDefault()?.SKU ?? "";
            var skuParts = baseSKU.Split("-".ToCharArray());
            var skuList = product.Variations.Select(v => v.SKU).ToList();
            var mainSKU = (skuList.Distinct().Count() > 1 && skuParts.Count() > 1) ? String.Join("-", skuParts.Take(skuParts.Count() - 1))
                : SkuHelper.RemoveSKULastPartNames(SkuHelper.RemoveSKULastIndex(baseSKU), new List<string>() { "Kid", "Kids", "Adult", "Adults", "OneSize" });
            //var mainSKU = (skuList.Distinct().Count() > 1 && skuParts.Count() > 1) ? String.Join("-", skuParts.Take(skuParts.Count() - 1)) : baseSKU;


            //NOTE: example: SUM91CPDR7NFL - 2T/3T
            mainSKU = StringHelper.TrimWhitespace(mainSKU);
            return mainSKU;
        }

        private bool IsOverseasSKU(string sku)
        {
            if (String.IsNullOrEmpty(sku))
                return true;

            return sku.Contains(":")
                || sku.Contains(";");
        }

        private List<string> GetSKUCandidats(ParentItemDTO product)
        {
            var baseSKU = product.Variations.FirstOrDefault()?.SKU ?? "";
            var subStylePart = product.Variations.FirstOrDefault().SubStyleVariation;
            var skuColor = SkuHelper.PrepareSKU(product.Variations.FirstOrDefault().Color);

            var mainSKUsToLookup = new List<string>();
            var mainSKU = GetSimpleMainSKU(product);

            //var mainSKU = (skuList.Distinct().Count() > 1 && skuParts.Count() > 1) ? String.Join("-", skuParts.Take(skuParts.Count() - 1)) : baseSKU;
            if (product.DropShipperId == DSHelper.OverseasId
                || IsOverseasSKU(baseSKU))
            {
                if (!String.IsNullOrEmpty(mainSKU))
                    mainSKUsToLookup.Add(mainSKU);
                mainSKU = product.SourceMarketId;
            }
            else
            {
                mainSKUsToLookup.Add(product.SourceMarketId); //NOTE: help to keep StyleID when style convert Overseas => MBG
            }

            if (!String.IsNullOrEmpty(subStylePart))
            {
                if (product.ForceEnableSubStyleVariations)
                {
                    mainSKU += "-" + subStylePart.Replace(" ", "");
                    mainSKUsToLookup.Add(mainSKU);
                }
                else
                {
                    mainSKUsToLookup.Add(mainSKU);
                    mainSKUsToLookup.Add(mainSKU + "-" + subStylePart.Replace(" ", ""));
                }
            }

            mainSKUsToLookup.Insert(0, mainSKU); //NOTE: check both with/w/o color in name
            if (product.ForceEnableColorVariations)
            {
                var colorizedSKU = StringHelper.JoinTwo("-", mainSKU, skuColor);
                if (!mainSKU.Contains("-" + skuColor))
                {
                    mainSKU = colorizedSKU; //NOTE: override main SKU
                    mainSKUsToLookup.Insert(0, colorizedSKU);
                }
                else
                {
                    //NOTE: we keep main SKU as is
                    mainSKUsToLookup.Add(colorizedSKU); //NOTE: add to the list end
                }

                var colorizedSourceMarketId = StringHelper.JoinTwo("-", product.SourceMarketId, skuColor);
                mainSKUsToLookup.Add(colorizedSourceMarketId);

                var preparedMainSKU = SkuHelper.PrepareSKU(mainSKU);
                if (mainSKU != preparedMainSKU)
                    mainSKUsToLookup.Insert(0, preparedMainSKU);

                if (!String.IsNullOrEmpty(product.SKU)) //NOTE: add first variation SKU before dividing by color
                {
                    var mainPart = (product.SKU ?? "").Split('-').FirstOrDefault();
                    mainSKUsToLookup.Add(mainPart);
                    mainSKUsToLookup.Add(product.SKU);
                }
            }
            else
            {
                var coloredMainSKU = SkuHelper.PrepareSKU(StringHelper.JoinTwo("-", mainSKU, skuColor));
                mainSKUsToLookup.Add(coloredMainSKU); //As options to lookup, not the general SKU
            }

            if (_skusBlackList != null)
                mainSKUsToLookup = mainSKUsToLookup.Where(s => !_skusBlackList.Contains(s)).ToList();

            return mainSKUsToLookup.ToList();
        }

        private Style LookupStyle(IUnitOfWork db, IList<string> skuToLookup)
        {
            Style resultStyle = null;
            //Get by SKU/Name
            //NOTE: Wrong if product has multi-style (all SKU link to one style)!!!
            //existStyle = db.Styles.GetAll().FirstOrDefault(st => st.LiteCountingStatus == product.SourceMarketId
            //    && !String.IsNullOrEmpty(st.LiteCountingStatus));

            //NOTE: lookup each candidate with priority (firstly check with color into name if exists)
            foreach (var sku in skuToLookup)
            {
                if (String.IsNullOrEmpty(sku))
                    continue;

                var candidates = db.Styles.GetAll()
                    .Where(s => (s.StyleID == sku || s.OriginalStyleID.Contains(sku)) && !s.Deleted)
                    .ToList();

                candidates.ForEach(c => c.OriginalStyleID = ";" + (c.OriginalStyleID ?? "").Replace(",", ";").Replace(" ", "").Trim(';') + ";");

                resultStyle = candidates.FirstOrDefault(s => StringHelper.IsEqualNoCase(s.StyleID, sku));
                if (resultStyle == null)
                {
                    resultStyle = candidates.FirstOrDefault(s => StringHelper.ContainsNoCase(s.OriginalStyleID, ";" + sku + ";"));
                }

                if (resultStyle != null)
                    break;
            }

            return resultStyle;
        }

        private void CreateOrUpdateListing(IDbFactory dbFactory,
            ITime time,
            ILogService log,
            IStyleHistoryService styleHistoryService,
            MarketType market,
            string marketplaceId,
            ParentItemDTO product,
            bool canCreate,
            bool updateQty,
            bool updateStyleInfo,
            bool mapByBarcode,
            bool processOnlyNewStyles,
            bool overrideStyleIds,
            bool overrideStyleTitles,
            bool importDescription)
        {
            using (var db = dbFactory.GetRWDb())
            {
                //var productImageUrl = product.ImageSource;

                if (!product.Variations.Any())
                {
                    log.Debug("No variations, productId=" + product.SourceMarketId + ", handle=" + product.SourceMarketUrl);
                }

                var dropShipperId = GetDropShipperId(product.SearchKeywords, product.Type, product.Department, product.AmazonName);
                product.DropShipperId = dropShipperId;
                var mainSKUsToLookup = GetSKUCandidats(product);
                var mainSKU = mainSKUsToLookup.FirstOrDefault();

                _log.Info("Main SKU: " + mainSKU + ", variations: " + String.Join(", ", product.Variations.Select(v => v.SKU)));
                var firstMRSP = product.Variations.FirstOrDefault()?.ListPrice;
                //var firstBarcode = product.Variations.FirstOrDefault()?.Barcode;

                //Style
                Style existStyle = null;
                ParentItem existParentItem = null;

                existStyle = LookupStyle(db, mainSKUsToLookup);

                if (existStyle != null && updateStyleInfo && !processOnlyNewStyles)
                {
                    _log.Info("Style found by StyleID: " + existStyle.StyleID);

                    #region Lite Style Updates
                    //NOTE: always override
                    if (!String.IsNullOrEmpty(product.Tags))
                        existStyle.Tags = product.Tags;
                    if (!String.IsNullOrEmpty(product.Type))
                    {
                        existStyle.BulletPoint2 = product.Type;
                        UpdateFeatureByName(db, existStyle.Id, StyleFeatureHelper.ProductType, product.Type);
                    }
                    existStyle.Manufacturer = product.Department;

                    //NOTE: We update style title after published status changed
                    var publishedAtValue = DateHelper.ToDateTimeString(product.Variations?.FirstOrDefault()?.PuclishedStatusDate);
                    var isUpdated = UpdateFeatureByName(db, existStyle.Id, StyleFeatureHelper.PublishedAtName, publishedAtValue);
                    if (String.IsNullOrEmpty(existStyle.Name)
                        || overrideStyleTitles
                        || (!String.IsNullOrEmpty(publishedAtValue) && isUpdated))
                    {
                        styleHistoryService.AddRecord(existStyle.Id, StyleHistoryHelper.StyleNameKey, existStyle.Name, product.AmazonName, null);
                        _log.Info("Style Name changed: " + existStyle.Name + " => " + product.AmazonName);
                        existStyle.Name = product.AmazonName;
                    }

                    if (importDescription)
                    {
                        if (String.IsNullOrEmpty(existStyle.Description))
                            existStyle.Description = StringHelper.TrimTags(product.Description);
                    }

                    //NOTE: temp we have inconsistency
                    if (existStyle.StyleID == mainSKU)
                        existStyle.OriginalStyleID = mainSKU;

                    existStyle.SourceMarketId = product.SourceMarketId.ToString();

                    if (existStyle.AutoDSSelection || existStyle.DropShipperId == null)
                    {
                        if (existStyle.DropShipperId != dropShipperId)
                        {
                            _log.Info("DS Changed: " + existStyle.DropShipperId + "=>" + dropShipperId);
                            styleHistoryService.AddRecord(existStyle.Id, StyleHistoryHelper.DropShipperKey, existStyle.DropShipperId, dropShipperId, null);
                        }
                        existStyle.DropShipperId = dropShipperId;
                    }
                    db.Commit();

                    #endregion

                    ProcessStyleImages(db, existStyle, product);

                    existParentItem = CreateOrUpdateParentItem(db, product, importDescription);

                    ProcessVariations(db,
                        existParentItem.ASIN,
                        market,
                        marketplaceId,
                        product,
                        existStyle,
                        canCreate,
                        false,
                        overrideStyleIds: overrideStyleIds);
                }
                else
                {
                    _log.Info("New style, StyleId=" + mainSKU);

                    #region Full Style Updates
                    var isNewStyle = false;
                    //Create new
                    if (existStyle == null)
                    {
                        if (!canCreate)
                        {
                            _log.Info("Unable to find style for, SKU: " + mainSKU);
                            return;
                        }

                        existStyle = new Style()
                        {
                            StyleID = mainSKU,
                            OriginalStyleID = mainSKU,
                            AutoDSSelection = true,
                            CreateDate = time.GetAppNowTime(),
                        };
                        db.Styles.Add(existStyle);

                        isNewStyle = true;
                    }

                    if (updateStyleInfo || isNewStyle)
                    {
                        //NOTE: no StyleId, OriginalStyleId updates, may manually changed
                        if (isNewStyle)
                        {
                            existStyle.StyleID = mainSKU;
                            existStyle.OriginalStyleID = mainSKU;
                        }
                        existStyle.Description = StringHelper.TrimTags(product.Description);

                        styleHistoryService.AddRecord(existStyle.Id, StyleHistoryHelper.StyleNameKey, existStyle.Name, product.AmazonName, null);
                        existStyle.Name = product.AmazonName;

                        existStyle.Image = product.ImageSource;
                        existStyle.Manufacturer = product.Department;
                        //TODO: Remove Tags
                        existStyle.Tags = product.Tags;
                        existStyle.BulletPoint2 = product.Type;
                        existStyle.MSRP = firstMRSP;
                        existStyle.Price = product.Variations.Select(v => v.AmazonCurrentPrice).FirstOrDefault();

                        existStyle.SourceMarketId = product.SourceMarketId.ToString();

                        if (existStyle.AutoDSSelection)
                        {
                            if (existStyle.DropShipperId != dropShipperId)
                            {
                                _log.Info("DS Changed: " + existStyle.DropShipperId + "=>" + dropShipperId);
                                styleHistoryService.AddRecord(existStyle.Id, StyleHistoryHelper.DropShipperKey, existStyle.DropShipperId, dropShipperId, null);
                            }
                            existStyle.DropShipperId = dropShipperId;
                        }

                        existStyle.UpdateDate = time.GetAppNowTime();
                    }

                    db.Commit();

                    if (updateStyleInfo || isNewStyle)
                    {
                        //Style Image
                        ProcessStyleImages(db, existStyle, product);
                    }

                    //StyleFeatures
                    if (updateStyleInfo || isNewStyle)
                    {
                        ProcessFeatures(db, existStyle, product);
                    }

                    //NOTE: We update style title after published status changed
                    var publishedAtValue = DateHelper.ToDateTimeString(product.Variations?.FirstOrDefault()?.PuclishedStatusDate);
                    var isUpdated = UpdateFeatureByName(db, existStyle.Id, StyleFeatureHelper.PublishedAtName, publishedAtValue);
                    if (overrideStyleTitles
                        || (!String.IsNullOrEmpty(publishedAtValue) && isUpdated))
                    {
                        styleHistoryService.AddRecord(existStyle.Id, StyleHistoryHelper.StyleNameKey, existStyle.Name, product.AmazonName, null);
                        existStyle.Name = product.AmazonName;
                    }
                    #endregion

                    if (updateStyleInfo || isNewStyle)
                    {
                        //ParentItem
                        existParentItem = CreateOrUpdateParentItem(db, product, importDescription);
                    }

                    if (updateStyleInfo || isNewStyle)
                    {
                        ProcessVariations(db,
                            existParentItem.ASIN,
                            market,
                            marketplaceId,
                            product,
                            existStyle,
                            canCreate || isNewStyle,
                            false,
                            overrideStyleIds: overrideStyleIds);
                    }
                }
            }
        }

        private void ProcessStyleImages(IUnitOfWork db,
            Style existStyle,
            ParentItemDTO product)
        {
            var isSyncDisabled = db.StyleFeatureTextValues.GetAllWithFeature().Where(sfv => sfv.StyleId == existStyle.Id
                && sfv.FeatureName == StyleFeatureHelper.DisableImagesSync).FirstOrDefault()?.Value == "1";

            if (isSyncDisabled)
                return;

            var dbExistStyleImages = db.StyleImages.GetAll()
                .Where(si => si.StyleId == existStyle.Id
                    && !String.IsNullOrEmpty(si.SourceMarketId)) //NOTE: updating only images from market, ignore custom images
                .ToList();

            var variantImages = product.Variations.Where(v => v.Images != null)
                .SelectMany(v => v.Images)
                .GroupBy(im => im.SourceImageId)
                .Select(g => new ImageDTO()
                {
                    SourceImageId = g.Key,
                    ImageUrl = g.First().ImageUrl
                })
                .OrderByDescending(im => im.SourceImageId)
                .ToList();
            var marketImages = new List<ImageDTO>();
            marketImages.AddRange(variantImages);
            foreach (var productImage in product.Images)
            {
                if (marketImages.All(mi => mi.SourceImageId != productImage.SourceImageId))
                    marketImages.Add(productImage);
            }

            var mainImage = marketImages.FirstOrDefault()?.ImageUrl;// StringHelper.GetFirstNotEmpty(product.Variations.FirstOrDefault()?.ImageUrl, product.ImageSource);
            //if (!String.IsNullOrEmpty(mainImage))
            //    marketImages.Add(mainImage);
            //if (product.ImageSource != mainImage)
            //    marketImages.Add(product.ImageSource);
            //if (!String.IsNullOrEmpty(product.AdditionalImages))
            //    marketImages.AddRange(product.AdditionalImages.Split("; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where(i => i != mainImage));
            var defaultImage = dbExistStyleImages.FirstOrDefault(i => i.IsDefault)?.Image;

            if ((!dbExistStyleImages.Any(i => i.Type != (int)StyleImageType.Unavailable)
                && !String.IsNullOrEmpty(mainImage))
                || (marketImages.Count() != dbExistStyleImages.Count())
                || marketImages.Any(im => !dbExistStyleImages.Any(em => em.Image == im.ImageUrl)) //NOTE: on market we have any new image
                || existStyle.Image != mainImage
                || defaultImage != mainImage) //NOTE: Main image was changed
            {
                var isSetDefault = false;
                for (var i = 0; i < marketImages.Count; i++)
                {
                    var marketImage = marketImages[i];
                    var dbExistImage = dbExistStyleImages.FirstOrDefault(im => im.SourceMarketId == marketImage.SourceImageId);

                    if (dbExistImage == null)
                    {
                        dbExistImage = new StyleImage()
                        {
                            StyleId = existStyle.Id,
                            CreateDate = _time.GetAppNowTime(),
                        };
                        db.StyleImages.Add(dbExistImage);
                    }
                    if (dbExistImage.Image != marketImage.ImageUrl)
                        dbExistImage.Image = marketImage.ImageUrl;
                    if (dbExistImage.Category != (int)StyleImageCategories.Deleted
                        && !isSetDefault)
                    {
                        dbExistImage.IsDefault = true;
                        mainImage = marketImage.ImageUrl;
                        isSetDefault = true;
                    }
                    else
                    {
                        dbExistImage.IsDefault = false;
                    }
                    dbExistImage.Tag = marketImage.ImageUrl;
                    dbExistImage.Type = (int)StyleImageType.HiRes;
                    dbExistImage.SourceMarketId = marketImage.SourceImageId;
                }
                if (existStyle.Image != mainImage)
                {
                    _styleHistoryService.AddRecord(existStyle.Id, StyleHistoryHelper.PictureKey, existStyle.Image, mainImage, null);
                    existStyle.Image = mainImage;
                }

                var existSourceMarketIds = marketImages.Select(im => im.SourceImageId).ToList();
                var toRemoveImages = dbExistStyleImages.Where(sim => String.IsNullOrEmpty(sim.SourceMarketId)
                    || !existSourceMarketIds.Contains(sim.SourceMarketId)).ToList();

                foreach (var toRemove in toRemoveImages)
                {
                    _log.Info("Remove image, id=" + toRemove.Id + ", styleId=" + toRemove.StyleId + ", sourceMarketId=" + toRemove.SourceMarketId);
                    //TODO: remove from Disk if necessary
                    db.StyleImages.Remove(toRemove);
                }

                db.Commit();
            }
        }

        private ParentItem CreateOrUpdateParentItem(IUnitOfWork db,
            ParentItemDTO product,
            bool importDescription)
        {
            var existParentItem = db.ParentItems.GetAll()
                .FirstOrDefault(pi => pi.Market == (int)product.Market
                    && (pi.MarketplaceId == product.MarketplaceId || String.IsNullOrEmpty(product.MarketplaceId))
                    && pi.SourceMarketId == product.SourceMarketId);

            if (existParentItem == null)
            {
                existParentItem = new ParentItem()
                {
                    Market = (int)product.Market,
                    MarketplaceId = product.MarketplaceId,
                    ASIN = product.ASIN,
                    SourceMarketId = product.SourceMarketId,
                    CreateDate = _time.GetAppNowTime(),
                };
                db.ParentItems.Add(existParentItem);
            }
            existParentItem.ASIN = product.ASIN;
            existParentItem.SourceMarketId = product.SourceMarketId;
            existParentItem.AmazonName = product.AmazonName;
            existParentItem.Type = product.Type;

            if (importDescription)
            {
                existParentItem.Description = StringHelper.TrimTags(product.Description);
            }
            existParentItem.ImageSource = product.ImageSource;
            existParentItem.SourceMarketUrl = product.SourceMarketUrl;
            existParentItem.SearchKeywords = product.SearchKeywords;

            existParentItem.IsAmazonUpdated = true;
            existParentItem.LastUpdateFromAmazon = _time.GetAppNowTime();
            existParentItem.UpdateDate = _time.GetAppNowTime();

            db.Commit();

            product.Id = existParentItem.Id;

            return existParentItem;
        }

        private bool UpdateFeatureByName(IUnitOfWork db,
            long styleId,
            string featureName,
            string newFeatureValue)
        {
            var wasChanged = false;

            var feature = db.Features.GetAll().FirstOrDefault(f => f.Name == featureName);
            if (feature == null)
                return wasChanged;

            var featureValue = (from fv in db.StyleFeatureTextValues.GetAll()
                                where fv.FeatureId == feature.Id
                                     && fv.StyleId == styleId
                                select fv).FirstOrDefault();
            if (featureValue != null)
            {
                if (featureValue.Value != newFeatureValue)
                {
                    featureValue.Value = newFeatureValue;
                    featureValue.UpdateDate = _time.GetAppNowTime();
                    wasChanged = true;
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(newFeatureValue))
                {
                    db.StyleFeatureTextValues.Add(new StyleFeatureTextValue()
                    {
                        FeatureId = feature.Id,
                        Value = newFeatureValue,
                        CreateDate = _time.GetAppNowTime(),
                    });
                    //NOTE: only when we change the value, not add
                    //wasChanged = true;
                }
            }
            db.Commit();

            return wasChanged;
        }

        private void ProcessFeatures(IUnitOfWork db,
            Style style,
            ParentItemDTO product)
        {
            var existFeatures = db.StyleFeatureTextValues.GetAll().Where(tv => tv.StyleId == style.Id).ToList();
            var existDdlFeatures = db.StyleFeatureValues.GetAll().Where(tv => tv.StyleId == style.Id).ToList();
            var allFeatureValues = db.FeatureValues.GetAllAsDto().ToList();
            var allFeatures = db.Features.GetAllAsDto().ToList();
            var newFeatures = ComposeFeatureList(db,
                allFeatureValues,
                allFeatures,
                product.Type,
                product.SearchKeywords);
            newFeatures.Add(ComposeFeatureValue(db, "Product Type", product.Type));
            newFeatures.Add(ComposeFeatureValue(db, StyleFeatureHelper.PublishedAtName, DateHelper.ToDateTimeString(product.Variations?.FirstOrDefault()?.PuclishedStatusDate)));
            //newFeatures.Add(ComposeFeatureValue(db, "Brand Name", product.Department));
            foreach (var feature in newFeatures)
            {
                if (feature == null)
                    continue;

                var existFeature = existFeatures.FirstOrDefault(f => f.FeatureId == feature.Id);
                var existDdlFeature = existDdlFeatures.FirstOrDefault(f => f.FeatureId == feature.Id);
                if (existFeature == null && existDdlFeature == null)
                {
                    if (feature.Type == (int)FeatureValuesType.TextBox || feature.Type == (int)FeatureValuesType.TextArea)
                    {
                        db.StyleFeatureTextValues.Add(new StyleFeatureTextValue()
                        {
                            FeatureId = feature.FeatureId,
                            Value = StringHelper.Substring(feature.Value, 512),
                            StyleId = style.Id,

                            CreateDate = _time.GetAppNowTime()
                        });
                    }
                    if (feature.Type == (int)FeatureValuesType.DropDown)
                    {
                        db.StyleFeatureValues.Add(new StyleFeatureValue()
                        {
                            FeatureId = feature.FeatureId,
                            FeatureValueId = feature.FeatureValueId.Value,
                            StyleId = style.Id,

                            CreateDate = _time.GetAppNowTime()
                        });
                    }
                }
                else
                {
                }
            }
            db.Commit();
        }

        private void RemoveAllOutOfDateListings(IDbFactory dbFactory,
            MarketType market,
            string marketplaceId,
            IList<long> updatedListingIds)
        {
            try
            {
                _log.Info("RemoveAllOutOfDateListings");
                //Mark as deleted not exists
                using (var db = _dbFactory.GetRWDb())
                {
                    var allListingIds = db.Listings.GetAll()
                        .Where(l => l.Market == (int)market
                            && (l.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId)))
                        .Select(l => l.Id)
                        .ToList();

                    var notUpdatedListingIds = allListingIds.Where(l => !updatedListingIds.Contains(l)).ToList();
                    _log.Info("Not updated count: " + notUpdatedListingIds.Count());

                    var notExistListings = db.Listings.GetAll()
                        .Where(l => notUpdatedListingIds.Contains(l.Id)
                                    && l.Market == (int)market
                                    && (l.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId)))
                        .ToList();

                    if (notExistListings.Count > updatedListingIds.Count() * 0.5)
                    {
                        _log.Info("Skip removed step, not exists listing more then 50% of updated listings: " + notExistListings.Count + "/" + updatedListingIds.Count);
                        return;
                    }

                    _log.Info("Listings to remove: " + notExistListings.Count());
                    foreach (var listing in notExistListings)
                    {
                        _log.Info("Listing was removed, SKU=" + listing.SKU);
                        listing.IsRemoved = true;
                        listing.UpdateDate = _time.GetAppNowTime();
                    }
                    db.Commit();
                }
            }
            catch (Exception ex)
            {
                _log.Error("RemoveAllOutOfDateListings", ex);
            }
        }

        private void ProcessVariations(IUnitOfWork db,
            string parentASIN,
            MarketType market,
            string marketplaceId,
            ParentItemDTO product,
            Style style,
            bool canCreate,
            bool canUpdate,
            bool overrideStyleIds)
        {
            foreach (var variation in product.Variations)
            {
                CreateOrUpdateStyleItemFromVariation(db, style, variation, canCreate);

                CreateOrUpdateListingFromVariation(db, product, variation, overrideStyleIds);
            }
        }

        private void CreateOrUpdateStyleItemFromVariation(IUnitOfWork db,
            Style style,
            ItemDTO variation,
            bool canCreate)
        {
            //StyleItem
            var existStyleItem = db.StyleItems.GetAll().FirstOrDefault(si => si.StyleId == style.Id
                    && si.SourceMarketId == variation.ListingId.ToString());

            if (existStyleItem == null)
            {
                var styleItemsBySize = db.StyleItems.GetAll()
                    .Where(si => si.StyleId == style.Id
                                          && si.Size == variation.Size
                                          && (si.Color == variation.Color
                                            || (String.IsNullOrEmpty(si.Color) && String.IsNullOrEmpty(variation.Color)))).ToList();
                if (styleItemsBySize.Count == 1)
                {
                    existStyleItem = styleItemsBySize[0];
                }
                else
                {
                    if (styleItemsBySize.Count > 1)
                    {
                        _log.Info("Multiple variation by size/color: " + style.Id + ", size/color=" + variation.Size +
                                  "/" + variation.Color);
                        existStyleItem = styleItemsBySize[0];
                    }
                }
            }

            var isNewStyleItem = false;
            if (existStyleItem == null)
            {
                if (!canCreate)
                {
                    _log.Info("Unable to find StyleItem for styleId: " + style.StyleID);
                    return;
                }

                existStyleItem = new StyleItem()
                {
                    StyleId = style.Id,
                    Size = variation.Size,
                    Color = variation.Color,
                    SourceMarketId = variation.SourceMarketId,
                    CreateDate = _time.GetAppNowTime()
                };
                db.StyleItems.Add(existStyleItem);

                isNewStyleItem = true;
            }
            else
            {
                if (existStyleItem.SourceMarketId != variation.SourceMarketId)
                {
                    existStyleItem.SourceMarketId = variation.SourceMarketId;
                }

                if (style.DropShipperId != DSHelper.PAonMBGId) //NOTE: Keep PA listings as is
                {
                    existStyleItem.Size = variation.Size;
                    existStyleItem.Color = variation.Color;
                }
                existStyleItem.UpdateDate = _time.GetAppNowTime();
            }

            //TEMP: disabled
            //if (isNewStyleItem)
            //{
            //    existStyleItem.Quantity = variation.AmazonRealQuantity;
            //    existStyleItem.QuantitySetDate = _time.GetAppNowTime();
            //}
            db.Commit();

            variation.StyleId = existStyleItem.StyleId;
            variation.StyleItemId = existStyleItem.Id;
            variation.StyleString = style.StyleID;

            //StyleItem Barcode
            if (!String.IsNullOrEmpty(variation.Barcode))
            {
                var existBarcode = db.StyleItemBarcodes.GetAll().FirstOrDefault(b => b.Barcode == variation.Barcode
                        && b.StyleItemId == existStyleItem.Id);
                if (existBarcode == null)
                {
                    _log.Info("Added new barcode: " + variation.Barcode);
                    existBarcode = new StyleItemBarcode()
                    {
                        Barcode = variation.Barcode,
                        CreateDate = _time.GetAppNowTime(),
                    };
                    db.StyleItemBarcodes.Add(existBarcode);
                }
                existBarcode.StyleItemId = existStyleItem.Id;
                existBarcode.UpdateDate = _time.GetAppNowTime();
                db.Commit();
            }

        }

        private void CreateOrUpdateListingFromVariation(IUnitOfWork db,
            ParentItemDTO product,
            ItemDTO variation,
            bool overrideStyleIds)
        {
            Item existItem = null;
            Listing existListing = null;

            //By styleItemId
            existItem = db.Items.GetAll().FirstOrDefault(i => i.SourceMarketId == variation.SourceMarketId);
            if (existItem != null)
            {
                existListing = db.Listings.GetAll().FirstOrDefault(l => l.ItemId == existItem.Id);
            }

            var isNewSku = !StringHelper.IsEqualNoCase(existListing?.SKU, variation.SKU);

            if (existItem == null)
            {
                _log.Info("Created ASIN: " + variation.ASIN);
                existItem = new Item()
                {
                    ParentASIN = product.ASIN,
                    ASIN = variation.ASIN,
                    SourceMarketId = variation.SourceMarketId,
                    Market = (int)product.Market,
                    MarketplaceId = product.MarketplaceId,
                    CreateDate = _time.GetAppNowTime(),
                    UpdateDate = _time.GetAppNowTime(),
                };
                db.Items.Add(existItem);
            }

            existItem.ASIN = variation.ASIN;
            existItem.SourceMarketId = variation.SourceMarketId;
            existItem.ParentASIN = product.ASIN;

            if (!existItem.StyleItemId.HasValue
                || !existItem.StyleId.HasValue
                || (overrideStyleIds && isNewSku)) //NOTE: 10/01/2020 override only when SKU was changed
            {
                if (existItem.StyleId != variation.StyleId)
                {
                    _log.Info("Changed style, styleId: " + existItem.StyleId + "=>" + variation.StyleId);
                    existItem.StyleId = variation.StyleId;
                }
                if (existItem.StyleItemId != variation.StyleItemId)
                {
                    _log.Info("Changed style, styleItemId: " + existItem.StyleItemId + "=>" + variation.StyleItemId);
                    existItem.StyleItemId = variation.StyleItemId;
                }
                existItem.StyleString = variation.StyleString;
            }
            else
            {
                if (existItem.StyleItemId != variation.StyleItemId)
                {
                    _log.Info("Possible style issue, itemId: " + existItem.Id + ", styleId: " + existItem.StyleId + "=>" + variation.StyleId + ", styleItemId: " + existItem.StyleItemId + "=>" + variation.StyleItemId);
                }
            }

            existItem.Title = product.AmazonName;
            existItem.Barcode = variation.Barcode;
            existItem.Color = StringHelper.Substring(variation.Color, 50);
            existItem.Size = StringHelper.Substring(variation.Size, 50);

            existItem.SourceMarketId = variation.SourceMarketId;
            existItem.SourceMarketUrl = variation.SourceMarketUrl;

            existItem.PrimaryImage = variation.ImageUrl;
            existItem.ListPrice = (decimal?)variation.ListPrice;

            existItem.UpdateDate = _time.GetAppNowTime();
            existItem.IsExistOnAmazon = true;
            existItem.LastUpdateFromAmazon = _time.GetAppNowTime();
            existItem.ItemPublishedStatus = variation.PublishedStatus;
            existItem.ItemPublishedStatusDate = variation.PuclishedStatusDate ?? _time.GetAppNowTime();

            db.Commit();

            variation.Id = existItem.Id;

            if (existListing == null)
            {
                _log.Info("Created listing: " + variation.SKU);
                existListing = new Listing()
                {
                    ItemId = existItem.Id,

                    ASIN = variation.ASIN,
                    ListingId = variation.ListingId,

                    Market = (int)product.Market,
                    MarketplaceId = product.MarketplaceId,

                    CreateDate = _time.GetAppNowTime(),
                };
                db.Listings.Add(existListing);
            }

            existListing.ASIN = variation.ASIN;
            existListing.ListingId = variation.ListingId;
            existListing.SKU = variation.SKU;

            existListing.CurrentPrice = (decimal)(variation.AmazonCurrentPrice ?? 0);
            existListing.AmazonCurrentPrice = (decimal?)variation.AmazonCurrentPrice;
            existListing.AmazonCurrentPriceUpdateDate = _time.GetAmazonNowTime();
            existListing.PriceFromMarketUpdatedDate = _time.GetAppNowTime();

            //existListing.RealQuantity = variation.AmazonRealQuantity ?? 0;
            existListing.AmazonRealQuantity = variation.AmazonRealQuantity ?? 0;
            existListing.AmazonRealQuantityUpdateDate = _time.GetAppNowTime();

            existListing.UpdateDate = _time.GetAppNowTime();
            existListing.IsRemoved = false;

            db.Commit();

            variation.ListingEntityId = existListing.Id;
        }

        public int? GetDropShipperId(string tags, string type, string vendor, string name)
        {
            if (StringHelper.ContrainOneOfKeywords(tags, new string[] { "Premium Apparel" }))
                return DSHelper.PAonMBGId;
            if (StringHelper.ContainsNoCase(vendor, "Overseas Fulfillment")
                || StringHelper.ContainsNoCase(vendor, "Overseas Fufillment")
                || StringHelper.ContainsNoCase(vendor, "Overseas Fulfillement")
                || StringHelper.ContainsNoCase(vendor, "Overseas Fulfullment")
                || StringHelper.ContrainOneOfKeywords(tags, new string[] { "Mia Belle Overseas Fulfillment", "Overseas Fulfillment", "Overseas Fufillment", "Overseas Fulfillement" })
                || StringHelper.ContrainOneOfKeywords(tags, new string[] { "dropshipping" }))
                return DSHelper.OverseasId;
            if (StringHelper.ContrainOneOfKeywords(tags, new string[] { "preorder", "pre-order" })
                || StringHelper.ContrainOneOfKeywords(name, new string[] { "preorder", "pre-order" }))
                return DSHelper.PreorderId;

            return DSHelper.DefaultMBGId;
        }

        public StyleFeatureValueDTO ComposeFeatureValue(IUnitOfWork db,
            string featureName,
            string featureValue)
        {
            StyleFeatureValueDTO result = null;
            var feature = db.Features.GetAll().FirstOrDefault(f => f.Name == featureName);
            if (feature != null)
            {
                if (feature.ValuesType == (int)FeatureValuesType.DropDown)
                {
                    var existFeatureValues = db.FeatureValues.GetAll().Where(f => f.FeatureId == feature.Id);
                    if (!String.IsNullOrEmpty(featureValue))
                    {
                        var existFeatureValue = existFeatureValues.FirstOrDefault(v => String.Compare(v.Value, featureValue, StringComparison.OrdinalIgnoreCase) == 0);
                        if (existFeatureValue == null)
                        {
                            existFeatureValue = new FeatureValue()
                            {
                                FeatureId = feature.Id,
                                Value = featureValue,
                                Order = (int)featureValue.ToLower()[0],
                            };
                            db.FeatureValues.Add(existFeatureValue);
                            db.Commit();
                        }

                        result = new StyleFeatureValueDTO()
                        {
                            FeatureName = featureName,
                            Value = featureValue,
                            FeatureId = feature.Id,
                            FeatureValueId = existFeatureValue.Id,
                            Type = feature.ValuesType,
                        };
                    }
                }

                if (feature.ValuesType == (int)FeatureValuesType.TextBox || feature.ValuesType == (int)FeatureValuesType.TextArea)
                {
                    result = new StyleFeatureValueDTO()
                    {
                        FeatureName = featureName,
                        Value = featureValue,
                        FeatureId = feature.Id,
                        Type = feature.ValuesType,
                    };
                }
            }
            else
            {
                _log.Info("Doesnt exist feature name: " + featureName);
            }

            return result;
        }

        public IList<StyleFeatureValueDTO> ComposeFeatureList(IUnitOfWork db,
            IList<FeatureValueDTO> featureValues,
            IList<FeatureDTO> features,
            string productType,
            string tags)
        {
            var results = new List<StyleFeatureValueDTO>();

            tags = tags ?? "";
            productType = productType ?? "";

            //Gender
            string gender = null;
            if (productType.ToLower().Contains("girl"))
            {
                gender = "Girls";
            }
            if (productType.ToLower().Contains("boy"))
            {
                gender = "Boys";
            }
            if (productType.ToLower().Contains("men"))
            {
                gender = "Mens";
            }
            if (productType.ToLower().Contains("women"))
            {
                gender = "Womens";
            }
            if (!String.IsNullOrEmpty(gender))
            {
                var feature = features.FirstOrDefault(f => f.Id == StyleFeatureHelper.GENDER);
                var featureValue = featureValues.FirstOrDefault(fv => fv.Value == gender
                                                                      && fv.FeatureId == StyleFeatureHelper.GENDER);
                if (feature != null && featureValue != null)
                {
                    results.Add(new StyleFeatureValueDTO()
                    {
                        FeatureId = feature.Id,
                        FeatureValueId = featureValue.Id,
                        Type = feature.ValuesType,
                        Value = featureValue.Value
                    });
                }
            }
            featureValues = featureValues.Where(fv => fv.FeatureId != StyleFeatureHelper.GENDER
                && fv.FeatureId != StyleFeatureHelper.SHIPPING_SIZE
                && fv.FeatureId != StyleFeatureHelper.SUB_LICENSE1
                && fv.FeatureId != StyleFeatureHelper.SIZE
                && fv.FeatureId != StyleFeatureHelper.SUB_LICENSE2)
                .ToList();

            //Other features
            foreach (var featureValue in featureValues)
            {
                var feature = features.FirstOrDefault(f => f.Id == featureValue.FeatureId);

                if (feature != null)
                {
                    if (results.All(f => f.FeatureId != featureValue.FeatureId))
                    {
                        if (productType.IndexOf(featureValue.Value, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            results.Add(new StyleFeatureValueDTO()
                            {
                                FeatureId = featureValue.FeatureId,
                                FeatureValueId = featureValue.Id,
                                Type = feature.ValuesType,
                                Value = featureValue.Value
                            });
                        }
                    }
                }
            }

            return results;
        }
    }
}
