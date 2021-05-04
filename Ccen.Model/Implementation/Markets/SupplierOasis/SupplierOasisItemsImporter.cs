using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Features;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.Implementation.Markets.SupplierOasis
{
    public class SupplierOasisItemsImporter
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public SupplierOasisItemsImporter(ILogService log,
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

            var index = 0;

            var updatedListingIds = new List<long>();

            foreach (var item in items)
            {
                _log.Info("item: " + index);
                CreateOrUpdateListing(_dbFactory,
                    _time,
                    _log,
                    api.Market,
                    api.MarketplaceId,
                    item);

                updatedListingIds.AddRange(item.Variations.Where(v => v.ListingEntityId.HasValue).Select(v => v.ListingEntityId.Value).ToList());
                index++;
            }

            //Mark as deleted not exists
            using (var db = _dbFactory.GetRWDb())
            {
                var notExistListings = db.Listings.GetAll().Where(l => !updatedListingIds.Contains(l.Id)
                    && l.Market == (int)api.Market
                    && (l.MarketplaceId == api.MarketplaceId || String.IsNullOrEmpty(api.MarketplaceId))).ToList();
                foreach (var listing in notExistListings)
                {
                    _log.Info("Listing was removed, SKU=" + listing.SKU);
                    listing.IsRemoved = true;
                    listing.UpdateDate = _time.GetAppNowTime();
                }
                db.Commit();
            }
        }


        private void CreateOrUpdateListing(IDbFactory dbFactory,
            ITime time,
            ILogService log,
            MarketType market,
            string marketplaceId,
            ParentItemDTO product)
        {
            using (var db = dbFactory.GetRWDb())
            {
                //var productImageUrl = product.ImageSource;

                if (!product.Variations.Any())
                {
                    log.Debug("No variations, productId=" + product.SourceMarketId + ", handle=" + product.SourceMarketUrl);
                }

                var firstSKU = product.Variations.FirstOrDefault()?.SKU;
                var firstMRSP = product.Variations.FirstOrDefault()?.ListPrice;
                var firstBarcode = product.Variations.FirstOrDefault()?.Barcode;

                //Style

                Style existStyle = null;

                //Get by SKU/Name
                if (existStyle == null)
                {
                    existStyle = db.Styles.GetAll().FirstOrDefault(s => s.StyleID == firstSKU);
                    if (existStyle != null)
                        _log.Info("Style found by StyleID");
                }

                //Get by barcode
                if (existStyle == null && !String.IsNullOrEmpty(firstBarcode))
                {
                    var barcode = db.StyleItemBarcodes.GetAll().FirstOrDefault(b => b.Barcode == firstBarcode);
                    if (barcode != null)
                    {
                        var styleItem = db.StyleItems.GetAll().FirstOrDefault(si => si.Id == barcode.StyleItemId);
                        existStyle = db.Styles.GetAll().FirstOrDefault(s => s.Id == styleItem.StyleId);
                    }
                    if (existStyle != null)
                        _log.Info("Style found by barcode");
                }


                //ParentItem
                var existParentItem = db.ParentItems.GetAll().FirstOrDefault(pi => pi.Market == (int)market
                                             && (pi.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))
                                             && pi.SourceMarketId == product.SourceMarketId);
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
                existParentItem.SourceMarketUrl = product.SourceMarketUrl;
                existParentItem.UpdateDate = time.GetAppNowTime();
                existParentItem.IsAmazonUpdated = true;
                existParentItem.LastUpdateFromAmazon = time.GetAppNowTime();

                db.Commit();

                product.Id = existParentItem.Id;

                foreach (var variation in product.Variations)
                {
                    //StyleItem
                    var existStyleItem = existStyle != null ? db.StyleItems.GetAll()
                        .FirstOrDefault(si => si.StyleId == existStyle.Id) : null;

                    variation.StyleItemId = existStyleItem?.Id;

                    //StyleItem Barcode
                    if (!String.IsNullOrEmpty(variation.Barcode)
                        && existStyleItem != null)
                    {
                        var existBarcode = db.StyleItemBarcodes.GetAll().FirstOrDefault(b => b.StyleItemId == existStyleItem.Id
                            && b.Barcode == variation.Barcode);
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

                    //Item
                    var existItem = db.Items.GetAll().FirstOrDefault(v => v.SourceMarketId == variation.SourceMarketId
                        && v.Market == (int)market
                        && (v.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId)));
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

                    //existItem.Title = product.AmazonName;
                    existItem.Barcode = variation.Barcode;
                    //existItem.Color = variation.Color;

                    existItem.SourceMarketUrl = variation.SourceMarketUrl;

                    //existItem.PrimaryImage = variation.ImageUrl;
                    //existItem.ListPrice = (decimal?)variation.ListPrice;

                    existItem.UpdateDate = time.GetAppNowTime();
                    existItem.IsExistOnAmazon = true;
                    existItem.LastUpdateFromAmazon = time.GetAppNowTime();
                    existItem.ItemPublishedStatus = variation.PublishedStatus;
                    existItem.ItemPublishedStatusDate = variation.PuclishedStatusDate ?? time.GetAppNowTime();

                    db.Commit();

                    variation.Id = existItem.Id;

                    //Listing
                    var existListing = db.Listings.GetAll().FirstOrDefault(v => v.ListingId == variation.ListingId.ToString()
                        && v.Market == (int)market
                        && (v.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId)));
                    if (existListing == null)
                    {
                        existListing = new Listing()
                        {
                            ItemId = existItem.Id,

                            ASIN = variation.ASIN,
                            ListingId = variation.ListingId,

                            Market = (int)market,
                            MarketplaceId = marketplaceId,

                            CreateDate = time.GetAppNowTime(),
                        };
                        db.Listings.Add(existListing);
                    }

                    existListing.SKU = variation.SKU;
                    //existListing.CurrentPrice = (decimal)(variation.AmazonCurrentPrice ?? 0);
                    existListing.AmazonCurrentPrice = (decimal?)variation.AmazonCurrentPrice;
                    existListing.AmazonCurrentPriceUpdateDate = time.GetAmazonNowTime();
                    existListing.PriceFromMarketUpdatedDate = time.GetAppNowTime();

                    //existListing.RealQuantity = variation.AmazonRealQuantity ?? 0;
                    existListing.AmazonRealQuantity = variation.AmazonRealQuantity ?? 0;
                    existListing.AmazonRealQuantityUpdateDate = time.GetAppNowTime();

                    existListing.UpdateDate = time.GetAppNowTime();
                    existListing.IsRemoved = false;

                    db.Commit();

                    variation.ListingEntityId = existListing.Id;
                }
            }
        }

        public static IList<StyleFeatureValueDTO> ExtactFeatureValueList(string description)
        {
            var results = new List<StyleFeatureValueDTO>();

            if (String.IsNullOrEmpty(description))
                return results;

            var featureNameKey = @"<th class=""a-span5 a-size-base"">";
            var featureValueKey = @"<td class=""a-span7 a-size-base"">";
            var startIndex = 0;
            var nextNameIndex = description.IndexOf(featureNameKey, startIndex);
            var nextValueIndex = description.IndexOf(featureValueKey, startIndex);
            while (nextNameIndex >= 0)
            {
                nextValueIndex += featureValueKey.Length;
                nextNameIndex += featureNameKey.Length;

                var endValueIndex = description.IndexOf("</td>", nextValueIndex);
                var endNameIndex = description.IndexOf("</th>", nextNameIndex);

                var value = description.Substring(nextValueIndex, endValueIndex - nextValueIndex);
                var name = StringHelper.AddSpacesBeforeUpperCaseLetters(description.Substring(nextNameIndex, endNameIndex - nextNameIndex));
                value = StringHelper.TrimTags(StringHelper.TrimWhitespace(value));

                //Replaces
                value = value.Replace("Base MetalåÊ", "Base Metal")
                    .Replace("Stainless steel, Stainless Steel", "Stainless steel")
                    .Replace("STAINLESS STEEL", "Stainless steel")
                    .Replace("SiliconeåÊ", "Silicone")
                    .Replace("åÊ", "");

                if (name.Length < 25 && name.Length > 1)
                {
                    results.Add(new StyleFeatureValueDTO()
                    {
                        Value = value,
                        FeatureName = name
                    });
                }

                startIndex = Math.Max(endValueIndex, endNameIndex);
                nextValueIndex = description.IndexOf(featureValueKey, startIndex);
                nextNameIndex = description.IndexOf(featureNameKey, startIndex);
            }

            return results;
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
                        var existFeatureValue = existFeatureValues.FirstOrDefault(v => v.Value == featureValue);
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

        public IList<StyleFeatureValueDTO> ComposeFeatureList(IUnitOfWork db, string description)
        {
            var results = new List<StyleFeatureValueDTO>();

            if (String.IsNullOrEmpty(description))
                return results;

            var featureValueList = ExtactFeatureValueList(description);

            foreach (var featureValue in featureValueList)
            {
                var value = featureValue.Value;
                var name = featureValue.FeatureName;

                if (name.Length < 25 && name.Length > 1)
                {
                    var result = ComposeFeatureValue(db, name, value);
                    if (result != null)
                        results.Add(result);
                }
            }

            return results;
        }
    }
}
