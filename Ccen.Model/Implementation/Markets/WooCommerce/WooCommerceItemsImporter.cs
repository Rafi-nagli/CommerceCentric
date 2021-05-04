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

namespace Amazon.Model.Implementation.Markets.WooCommerce
{
    public class WooCommerceItemsImporter
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public WooCommerceItemsImporter(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
        }

        public void Import(IMarketApi api)
        {
            var asinsWithError = new List<string>();
            var items = api.GetItems(_log,
                _time,
                null,
                ItemFillMode.Defualt,
                out asinsWithError);

            _log.Info("Items count: " + items.Count());
            var index = 0;

            var updatedListingIds = new List<long>();
            var processOnlyStyle = true;

            foreach (var item in items)
            {
                _log.Info("item: " + index);
                CreateOrUpdateListing(_dbFactory,
                    _time,
                    _log,
                    api.Market,
                    api.MarketplaceId,
                    item,
                    canCreate: true,
                    updateQty: true,
                    updateStyleInfo: true,
                    mapByBarcode: true,
                    processOnlyStyle: false);

                updatedListingIds.AddRange(item.Variations.Where(v => v.ListingEntityId.HasValue).Select(v => v.ListingEntityId.Value).ToList());
                index++;
            }

            //Mark as deleted not exists
            if (!processOnlyStyle)
            {
                using (var db = _dbFactory.GetRWDb())
                {
                    var notExistListings = db.Listings.GetAll().Where(l => !updatedListingIds.Contains(l.Id)
                                                                           && l.Market == (int)api.Market
                                                                           &&
                                                                           (l.MarketplaceId == api.MarketplaceId ||
                                                                            String.IsNullOrEmpty(api.MarketplaceId)))
                        .ToList();
                    foreach (var listing in notExistListings)
                    {
                        _log.Info("Listing was removed, SKU=" + listing.SKU);
                        listing.IsRemoved = true;
                        listing.UpdateDate = _time.GetAppNowTime();
                    }
                    db.Commit();
                }
            }
        }

        public void UpdateStyleFeatures(IList<long> styleIdList)
        {
            var pageSize = 100;
            var index = 0;

            while (index < styleIdList.Count)
            {
                var pageStyleIds = styleIdList.Skip(index).Take(pageSize).ToList();
                _log.Info("Index=" + index);

                using (var db = _dbFactory.GetRWDb())
                {
                    db.DisableValidation();

                    var dbStyles = db.Styles.GetAll()
                        .Where(st => pageStyleIds.Contains(st.Id))
                        .ToList();

                    var allExistFeatures = db.StyleFeatureTextValues
                        .GetAll()
                        .Where(fv => pageStyleIds.Contains(fv.StyleId))
                        .ToList();
                    var allEsistDdlFeatures = db.StyleFeatureValues.GetAll()
                        .Where(fv => pageStyleIds.Contains(fv.StyleId))
                        .ToList();
                    var allFeatures = db.Features.GetAll().ToList();
                    var allFeatureValues = db.FeatureValues.GetAll().ToList();

                    foreach (var dbStyle in dbStyles)
                    {
                        _log.Info(dbStyle.StyleID);

                        var existFeatures = allExistFeatures.Where(tv => tv.StyleId == dbStyle.Id).ToList();
                        var existDdlFeatures = allEsistDdlFeatures.Where(tv => tv.StyleId == dbStyle.Id).ToList();
                        var allDtoFeatureValues = db.FeatureValues.GetAllAsDto().ToList();
                        var newFeatureValues = ComposeFeatureList(db,
                            allDtoFeatureValues,
                            dbStyle.BulletPoint2,
                            dbStyle.BulletPoint1);

                        //newFeatures.Add(ComposeFeatureValue(db, "Product Type", product.Type));
                        //newFeatures.Add(ComposeFeatureValue(db, "Brand Name", dbStyle.Manufacturer));

                        foreach (var newFeatureValue in newFeatureValues)
                        {
                            if (newFeatureValue == null)
                                continue;

                            if (String.IsNullOrEmpty(newFeatureValue.Value))
                                continue;

                            var existFeature = existFeatures.FirstOrDefault(f => f.FeatureId == newFeatureValue.Id);
                            var existDdlFeature = existDdlFeatures.FirstOrDefault(f => f.FeatureId == newFeatureValue.Id);
                            if (existFeature == null && existDdlFeature == null)
                            {
                                var feature = allFeatures.FirstOrDefault(f => String.Compare(f.Name, newFeatureValue.FeatureName, StringComparison.OrdinalIgnoreCase) == 0);
                                if (feature != null)
                                {
                                    if (feature.ValuesType == (int)FeatureValuesType.TextBox)
                                    {
                                        db.StyleFeatureTextValues.Add(new StyleFeatureTextValue()
                                        {
                                            FeatureId = feature.Id,
                                            Value = StringHelper.Substring(newFeatureValue.Value, 512),
                                            StyleId = dbStyle.Id,

                                            CreateDate = _time.GetAppNowTime()
                                        });
                                    }
                                    if (feature.ValuesType == (int)FeatureValuesType.DropDown)
                                    {
                                        var existFeatureValue = allFeatureValues.FirstOrDefault(fv => fv.FeatureId == feature.Id
                                                                                  && String.Compare(fv.Value, newFeatureValue.Value, StringComparison.OrdinalIgnoreCase) == 0);

                                        if (existFeatureValue == null)
                                        {
                                            existFeatureValue = new FeatureValue()
                                            {
                                                FeatureId = feature.Id,
                                                Value = newFeatureValue.Value,
                                                Order = (int)newFeatureValue.Value.ToLower()[0],
                                            };
                                            db.FeatureValues.Add(existFeatureValue);
                                            db.Commit();
                                            allFeatureValues.Add(existFeatureValue);
                                        }

                                        db.StyleFeatureValues.Add(new StyleFeatureValue()
                                        {
                                            FeatureId = feature.Id,
                                            FeatureValueId = existFeatureValue.Id,
                                            StyleId = dbStyle.Id,

                                            CreateDate = _time.GetAppNowTime()
                                        });
                                    }
                                }
                            }
                        }
                    }

                    db.Commit();
                }

                index += pageSize;
            }
        }

        private void CreateOrUpdateListing(IDbFactory dbFactory,
            ITime time,
            ILogService log,
            MarketType market,
            string marketplaceId,
            ParentItemDTO product,
            bool canCreate,
            bool updateQty,
            bool updateStyleInfo,
            bool mapByBarcode,
            bool processOnlyStyle)
        {
            using (var db = dbFactory.GetRWDb())
            {
                //var productImageUrl = product.ImageSource;

                if (!product.Variations.Any())
                {
                    log.Debug("No variations, productId=" + product.SourceMarketId + ", handle=" + product.SourceMarketUrl);
                }

                var skuList = product.Variations.Select(v => v.SKU).ToList();
                var baseSKU = product.Variations.FirstOrDefault()?.SKU ?? "";
                var skuParts = baseSKU.Split("-".ToCharArray());
                
                var mainSKU = (skuList.Distinct().Count() > 1 && skuParts.Count() > 1) ? String.Join("-", skuParts.Take(skuParts.Count() - 1)) : baseSKU;
                _log.Info("Main SKU: " + mainSKU + ", variations: " + String.Join(", ", skuList));
                //var firstMRSP = product.Variations.FirstOrDefault()?.ListPrice;
                //var firstBarcode = product.Variations.FirstOrDefault()?.Barcode;

                //Style

                Style existStyle = null;

                //Get by SKU/Name
                if (existStyle == null)
                {
                    existStyle = db.Styles.GetAll().FirstOrDefault(s => s.StyleID == mainSKU);
                    if (existStyle != null)
                        _log.Info("Style found by StyleID");
                }
                
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
                        CreateDate = time.GetAppNowTime(),
                    };
                    db.Styles.Add(existStyle);

                    isNewStyle = true;
                }
                else
                {

                }

                existStyle.DropShipperId = DSHelper.DefaultDSId;

                if (updateStyleInfo || isNewStyle)
                {
                    existStyle.StyleID = mainSKU;
                    existStyle.Description = product.Description;
                    existStyle.Name = product.AmazonName;

                    existStyle.Image = product.ImageSource;
                    existStyle.Manufacturer = product.Department;
                    existStyle.BulletPoint1 = product.SearchKeywords;
                    existStyle.BulletPoint2 = product.Type;
                    //existStyle.MSRP = firstMRSP;
                    //existStyle.Price = product.Variations.Select(v => v.AmazonCurrentPrice).FirstOrDefault();

                    existStyle.UpdateDate = time.GetAppNowTime();
                }

                db.Commit();

                //if (!processOnlyStyle)
                {
                    //Style Image
                    var existStyleImages = db.StyleImages.GetAll().Where(si => si.StyleId == existStyle.Id).ToList();
                    if (!existStyleImages.Any() && !String.IsNullOrEmpty(product.ImageSource))
                    {
                        var newImage = new StyleImage()
                        {
                            Image = product.ImageSource,
                            IsDefault = true,
                            Type = (int)StyleImageType.HiRes,
                            StyleId = existStyle.Id,
                            CreateDate = _time.GetAppNowTime()
                        };
                        db.StyleImages.Add(newImage);
                        db.Commit();
                    }
                }

                //StyleFeatures
                if (isNewStyle && !processOnlyStyle)
                {
                    var existFeatures =
                        db.StyleFeatureTextValues.GetAll().Where(tv => tv.StyleId == existStyle.Id).ToList();
                    var existDdlFeatures =
                        db.StyleFeatureValues.GetAll().Where(tv => tv.StyleId == existStyle.Id).ToList();
                    var allFeatureValues = db.FeatureValues.GetAllAsDto().ToList();
                    var newFeatures = ComposeFeatureList(db,
                        allFeatureValues,
                        product.Type,
                        product.SearchKeywords);
                    newFeatures.Add(ComposeFeatureValue(db, "Product Type", product.Type));
                    //newFeatures.Add(ComposeFeatureValue(db, "Brand Name", product.Department));
                    foreach (var feature in newFeatures)
                    {
                        if (feature == null)
                            continue;

                        var existFeature = existFeatures.FirstOrDefault(f => f.FeatureId == feature.Id);
                        var existDdlFeature = existDdlFeatures.FirstOrDefault(f => f.FeatureId == feature.Id);
                        if (existFeature == null && existDdlFeature == null)
                        {
                            if (feature.Type == (int)FeatureValuesType.TextBox)
                            {
                                db.StyleFeatureTextValues.Add(new StyleFeatureTextValue()
                                {
                                    FeatureId = feature.FeatureId,
                                    Value = StringHelper.Substring(feature.Value, 512),
                                    StyleId = existStyle.Id,

                                    CreateDate = _time.GetAppNowTime()
                                });
                            }
                            if (feature.Type == (int)FeatureValuesType.DropDown)
                            {
                                db.StyleFeatureValues.Add(new StyleFeatureValue()
                                {
                                    FeatureId = feature.FeatureId,
                                    FeatureValueId = feature.FeatureValueId.Value,
                                    StyleId = existStyle.Id,

                                    CreateDate = _time.GetAppNowTime()
                                });
                            }
                        }
                    }
                    db.Commit();
                }

                ParentItem existParentItem = null;
                //if (!processOnlyStyle)
                {
                    //ParentItem
                    existParentItem = db.ParentItems.GetAll().FirstOrDefault(pi => pi.Market == (int)market
                                                                                       &&
                                                                                       (pi.MarketplaceId ==
                                                                                        marketplaceId ||
                                                                                        String.IsNullOrEmpty(
                                                                                            marketplaceId))
                                                                                       &&
                                                                                       pi.SourceMarketId ==
                                                                                       product.SourceMarketId);
                    if (existParentItem == null)
                    {
                        existParentItem = new ParentItem()
                        {
                            Market = (int)market,
                            MarketplaceId = marketplaceId,
                            ASIN = product.ASIN,
                            SourceMarketId = product.SourceMarketId,
                            CreateDate = time.GetAppNowTime(),
                        };
                        db.ParentItems.Add(existParentItem);
                    }
                    existParentItem.AmazonName = product.AmazonName;
                    existParentItem.Type = product.Type;
                    existParentItem.Description = product.Description;
                    existParentItem.ImageSource = product.ImageSource;
                    existParentItem.SourceMarketUrl = product.SourceMarketUrl;
                    existParentItem.SearchKeywords = product.SearchKeywords;
                    existParentItem.UpdateDate = time.GetAppNowTime();
                    existParentItem.IsAmazonUpdated = true;
                    existParentItem.LastUpdateFromAmazon = time.GetAppNowTime();

                    db.Commit();

                    product.Id = existParentItem.Id;
                }

                //if (!processOnlyStyle)
                {
                    foreach (var variation in product.Variations)
                    {
                        //StyleItem
                        var existStyleItem = db.StyleItems.GetAll()
                            .FirstOrDefault(si => si.StyleId == existStyle.Id);
                        // && (si.Color == variation.Color || String.IsNullOrEmpty(variation.Color)));

                        var isNewStyleItem = false;
                        if (existStyleItem == null)
                        {
                            if (!canCreate && !isNewStyle)
                            {
                                _log.Info("Unable to find StyleItem for styleId: " + existStyle.StyleID);
                                return;
                            }

                            existStyleItem = new StyleItem()
                            {
                                StyleId = existStyle.Id,
                                Size = variation.Size,
                                Color = variation.Color,
                            };
                            db.StyleItems.Add(existStyleItem);

                            isNewStyleItem = true;
                        }

                        //TEMP:
                        //if (updateQty || isNewStyle)
                        //{
                        //    existStyleItem.Quantity = variation.AmazonRealQuantity;
                        //    existStyleItem.QuantitySetDate = time.GetAppNowTime();
                        //}
                        db.Commit();

                        variation.StyleItemId = existStyleItem.Id;

                        //StyleItem Barcode
                        if (!String.IsNullOrEmpty(variation.Barcode))
                        {
                            var existBarcode =
                                db.StyleItemBarcodes.GetAll().FirstOrDefault(b => b.Barcode == variation.Barcode);
                            if (existBarcode == null)
                            {
                                _log.Info("Added new barcode: " + variation.Barcode);
                                existBarcode = new StyleItemBarcode()
                                {
                                    Barcode = variation.Barcode,
                                    CreateDate = time.GetAppNowTime(),
                                };
                                db.StyleItemBarcodes.Add(existBarcode);
                            }
                            existBarcode.StyleItemId = existStyleItem.Id;
                            existBarcode.UpdateDate = time.GetAppNowTime();
                            db.Commit();
                        }

                        //if (!processOnlyStyle)
                        {
                            //Item
                            var existItem =
                                db.Items.GetAll().FirstOrDefault(v => v.SourceMarketId == variation.SourceMarketId
                                                                      && v.Market == (int)market
                                                                      &&
                                                                      (v.MarketplaceId == marketplaceId ||
                                                                       String.IsNullOrEmpty(marketplaceId)));
                            if (existItem == null)
                            {
                                existItem = new Item()
                                {
                                    ParentASIN = existParentItem.ASIN,
                                    ASIN = variation.ASIN,
                                    SourceMarketId = variation.SourceMarketId,
                                    Market = (int)market,
                                    MarketplaceId = marketplaceId,
                                    CreateDate = time.GetAppNowTime(),
                                };
                                db.Items.Add(existItem);
                            }

                            existItem.StyleId = existStyle.Id;
                            existItem.StyleItemId = existStyleItem.Id;

                            existItem.Title = product.AmazonName;
                            existItem.Barcode = variation.Barcode;
                            existItem.Color = variation.Color;
                            existItem.Size = variation.Size;

                            existItem.SourceMarketUrl = variation.SourceMarketUrl;

                            existItem.PrimaryImage = variation.ImageUrl;
                            existItem.ListPrice = (decimal?)variation.ListPrice;

                            existItem.UpdateDate = time.GetAppNowTime();
                            existItem.IsExistOnAmazon = true;
                            existItem.LastUpdateFromAmazon = time.GetAppNowTime();
                            existItem.ItemPublishedStatus = variation.PublishedStatus;
                            existItem.ItemPublishedStatusDate = variation.PuclishedStatusDate ?? time.GetAppNowTime();

                            db.Commit();

                            variation.Id = existItem.Id;

                            //Listing
                            var existListing =
                                db.Listings.GetAll().FirstOrDefault(v => v.ListingId == variation.ListingId.ToString()
                                                                         && v.Market == (int)market
                                                                         &&
                                                                         (v.MarketplaceId == marketplaceId ||
                                                                          String.IsNullOrEmpty(marketplaceId)));
                            if (existListing == null)
                            {
                                existListing = new Listing()
                                {
                                    ItemId = existItem.Id,

                                    ASIN = variation.ASIN,
                                    ListingId = StringHelper.GetFirstNotEmpty(variation.ASIN, variation.ListingId),

                                    Market = (int)market,
                                    MarketplaceId = marketplaceId,

                                    CreateDate = time.GetAppNowTime(),
                                };
                                db.Listings.Add(existListing);
                            }

                            existListing.SKU = variation.SKU;

                            existListing.CurrentPrice = (decimal)(variation.AmazonCurrentPrice ?? 0);
                            existListing.AmazonCurrentPrice = (decimal?)variation.AmazonCurrentPrice;
                            existListing.AmazonCurrentPriceUpdateDate = time.GetAmazonNowTime();
                            existListing.PriceFromMarketUpdatedDate = time.GetAppNowTime();

                            existListing.RealQuantity = variation.AmazonRealQuantity ?? 0;
                            existListing.AmazonRealQuantity = variation.AmazonRealQuantity ?? 0;
                            existListing.AmazonRealQuantityUpdateDate = time.GetAppNowTime();

                            existListing.UpdateDate = time.GetAppNowTime();
                            existListing.IsRemoved = false;

                            db.Commit();

                            variation.ListingEntityId = existListing.Id;
                        }
                    }
                }
            }
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

                if (feature.ValuesType == (int)FeatureValuesType.TextBox)
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
                results.Add(new StyleFeatureValueDTO()
                {
                    FeatureId = StyleFeatureHelper.GENDER,
                    Value = gender
                });
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
                if (results.All(f => f.FeatureId != featureValue.FeatureId))
                {
                    if (productType.IndexOf(featureValue.Value, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        results.Add(new StyleFeatureValueDTO()
                        {
                            FeatureId = featureValue.FeatureId,
                            FeatureValueId = featureValue.Id,
                            Value = featureValue.Value
                        });
                    }
                }
            }
            
            return results;
        }
    }
}
