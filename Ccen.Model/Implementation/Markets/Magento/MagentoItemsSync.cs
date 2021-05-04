using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DTO;
using Amazon.DTO.Categories;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;
using Amazon.Model.Implementation.Sync;
using Magento.Api.Wrapper;
using Magento.RestApi20.Jsons.Products;

namespace Amazon.Model.Implementation.Markets.Magento
{
    public class MagentoItemsSync : IItemsSync
    {
        private ILogService _log;
        private ITime _time;
        private Magento20MarketApi _api;
        private IDbFactory _dbFactory;

        public const int RootParentId = 2;

        public MagentoItemsSync(Magento20MarketApi api,
            IDbFactory dbFactory,
            ILogService log,
            ITime time)
        {
            _log = log;
            _time = time;
            _api = api;
            _dbFactory = dbFactory;
        }

        public void SyncAttributeOptions()
        {
            _log.Info("Begin SyncAttributes");

            _api.Connect();
            var attributes = _api.GetAllAttributes();

            IList<FeatureDTO> allFeatures;
            IList<FeatureValueDTO> allFeatureValues;
            IList<string> allSizes;
            IList<string> allColors;
            using (var db = _dbFactory.GetRDb())
            {
                allFeatures = db.Features.GetAllAsDto().ToList();
                allFeatureValues = db.FeatureValues.GetAllAsDto().ToList();
                allSizes = db.Items.GetAll()
                    .Where(i => i.Market == (int)_api.Market
                        && !String.IsNullOrEmpty(i.Size))
                    .Select(i => i.Size)
                    .Distinct()
                    .OrderBy(i => i)
                    .ToList();
                allColors = db.Items.GetAll()
                    .Where(i => i.Market == (int)_api.Market
                        && !String.IsNullOrEmpty(i.Color))
                    .Select(i => i.Color)
                    .Distinct()
                    .OrderBy(i => i)
                    .ToList();
            }

            //var nameMapping = new Dictionary<string, string>()
            //{
            //    { MagentoFeatures.ManufacturerFeatureName, "Brand Name" },
            //    //{ MagentoFeatures.CountryOfManufacture, "Country of Origin" }, //2-letter abbr
            //    //{ MagentoFeatures.GenderFeatureName, "Gender" },

            //    { MagentoFeatures.BandMaterialFeatureName, "Band Material" },
            //    { MagentoFeatures.BandColorFeatureName, "Band Color" }, //NOTE: not exist in ccen
            //    { MagentoFeatures.BandWidthFeatureName, "Band Width" },

            //    { MagentoFeatures.CaseMaterialFeatureName, "Case Material" },
            //    { MagentoFeatures.CaseShapeFeatureName, "Case Shape" },
            //    { MagentoFeatures.CaseColorFeatureName, "Case Color" },
            //    { MagentoFeatures.CaseWidthFeatureName, "Case Width" },
            //    { MagentoFeatures.CaseHeightFeatureName, "Case Height" },
            //    { MagentoFeatures.CaseThicknessFeatureName, "Case Thickness" },


            //    //{ MagentoFeatures.BandMaterialFeatureName, "" },
            //};

            //Removed duplicate Magento featureValues
            //foreach (var attribute in attributes)
            //{
            //    var featureName = _categoriesToMap.FirstOrDefault(c => c.MagentoName == attribute.Name)?.SystemName;
            //    if (String.IsNullOrEmpty(featureName))
            //        continue;

            //    var feature = allFeatures.FirstOrDefault(f => f.Name == featureName);
            //    var featureValues = allFeatureValues.Where(fv => fv.FeatureId == feature.Id).ToList();

            //    var existOptions = new List<string>();
            //    foreach (var option in attribute.FeatureValues)
            //    {
            //        if (String.IsNullOrEmpty((option.ExtendedValue ?? "").Trim()))
            //            continue;

            //        if (existOptions.Contains(option.Value.ToLower())
            //            || featureValues.All(fv => fv.Value != option.Value))
            //        {
            //            _log.Info("Removed option: " + attribute.Name + " <= " + option.Value);
            //            _api.RemoveAttributeOption(attribute.Name, long.Parse(option.ExtendedValue));
            //        }
            //        else
            //        {
            //            existOptions.Add(option.Value.ToLower());
            //        }
            //    }
            //}


            List<FeatureValueDTO> featureValues = null;
            foreach (var attribute in attributes)
            {
                featureValues = null;
                if (attribute.Name == PAMagentoFeatures.ColorAttributeName)
                {
                    featureValues = allColors.Where(c => !String.IsNullOrEmpty(c)).Select(c => new FeatureValueDTO() { Value = c, ExtendedValue = "#008800" }).ToList();
                }
                else if (attribute.Name == PAMagentoFeatures.SizeAttributeName)
                {
                    featureValues = allSizes.Where(c => !String.IsNullOrEmpty(c)).Select(c => new FeatureValueDTO() { Value = c }).ToList();
                }
                else
                {
                    var featureName = _valueFeatureMappings.FirstOrDefault(c => c.MagentoName == attribute.Name)?.SystemName;
                    if (!String.IsNullOrEmpty(featureName))
                    {
                        var feature = allFeatures.FirstOrDefault(f => f.Name == featureName);
                        if (feature == null)
                            continue;

                        featureValues = allFeatureValues.Where(fv => fv.FeatureId == feature.Id).ToList();
                    }
                }

                if (featureValues != null)
                {
                    foreach (var featureValue in featureValues)
                    {
                        var existFv = attribute.FeatureValues.FirstOrDefault(
                                av => String.Compare(av.Value, featureValue.Value, StringComparison.OrdinalIgnoreCase) == 0);
                        if (existFv == null)
                        {
                            _log.Info("Added option: " + attribute.Name + " <= " + featureValue.Value + "(" + featureValue.ExtendedValue + ")");
                            _api.AddAttributeOption(attribute.Name, featureValue.Value, featureValue.ExtendedValue);
                        }
                    }
                }
            }


            _log.Info("End SyncAttributes");
        }

        public class CategoryMappingInfo
        {
            public string MagentoName { get; set; }
            public string SystemName { get; set; }
            public string[] ValuePostfix { get; set; }
        }

        private static Dictionary<string, string> _textFeatureMappings = new Dictionary<string, string>()
        {
            //{"Clasp Type", MagentoFeatures.ClaspTypeFeatureName},
            //{"Movement", MagentoFeatures.MovementFeatureName},
            //{"Water Resistance", MagentoFeatures.WaterResistantFeatureName},
            //{"Crystal", MagentoFeatures.CrystalFeatureName},
            //{"Dial Color", MagentoFeatures.DialFeatureName },
            //{"Crown", MagentoFeatures.CrownFeatureName},
            //{"Calendar", MagentoFeatures.CalendarFeatureName},
            //{"Featured", MagentoFeatures.SwFeaturedAttributeName },

            ////Sunglasses
            //{DWSFeatureNames.Feature1, MagentoFeatures.Feature1Name },
            //{DWSFeatureNames.Feature2, MagentoFeatures.Feature2Name },
            //{DWSFeatureNames.Feature3, MagentoFeatures.Feature3Name },
        };

        public static IList<CategoryMappingInfo> _valueFeatureMappings = new List<CategoryMappingInfo>
        {
            new CategoryMappingInfo()
            {
                SystemName = "Color",
                MagentoName = PAMagentoFeatures.ColorAttributeName,
            },
            new CategoryMappingInfo()
            {
                SystemName = "Size",
                MagentoName = PAMagentoFeatures.SizeAttributeName,
            }
        };

        public void SyncCategories()
        {
            _log.Info("Begin SyncCategories");

            _api.Connect();
            var existCategories = RetryHelper.ActionWithRetries(() => CallHelper.ThrowIfFail(_api.GetAllCategories()), _log, throwException: true).Data;
            _log.Info("Exist categories: " + existCategories.Count);

            IList<FeatureDTO> allFeatures;
            IList<FeatureValueDTO> allFeatureValues;
            IList<CustomCategoryDTO> allCustomCategories;
            using (var db = _dbFactory.GetRDb())
            {
                allFeatures = db.Features.GetAllAsDto().ToList();
                allFeatureValues = db.FeatureValues.GetAllAsDto()
                    .OrderBy(v => v.Value)
                    .ToList();
                allCustomCategories = db.CustomCategories.GetAllAsDto().ToList();
            }

            foreach (var customCategory in allCustomCategories)
            {
                var defaultParentId = 2;
                var categoryName = customCategory.Name;
                var existCategory = existCategories.FirstOrDefault(c => MagentoCategoryHelper.CompareCategoryNames(c.Name, categoryName, null));
                if (existCategory == null)
                {
                    _log.Info("Creating category: " + categoryName);
                    var result = _api.AddCategory(categoryName, defaultParentId, null);
                    if (result.IsFail)
                    {
                        _log.Error("Fail: " + result.Message);
                    }
                }
            }


            //Removed duplicate Magento featureValues
            foreach (var name in _valueFeatureMappings)
            {
                _log.Info(name.SystemName);
                var feature = allFeatures.FirstOrDefault(f => f.Name == name.SystemName);
                var featureValues = allFeatureValues.Where(f => f.FeatureId == feature.Id).ToList();
                var parentId = 2;

                foreach (var featureValue in featureValues)
                {
                    if (String.IsNullOrEmpty(featureValue.Value))
                        continue;

                    var postfixes = (name.ValuePostfix ?? new string[] { "" }).Take(1).ToList(); //TEMP: use first prefix when create catgories
                    foreach (var postfix in postfixes)
                    {
                        var categoryName = StringHelper.JoinTwo(" ", featureValue.Value, postfix);

                        var existCategory = existCategories.FirstOrDefault(
                                c => MagentoCategoryHelper.CompareCategoryNames(c.Name, categoryName, null)
                                     && c.ParentCategoryId == parentId);
                        if (existCategory == null)
                        {
                            _log.Info("Creating category: " + categoryName);
                            var result = _api.AddCategory(categoryName,
                                parentId,
                                null);
                            if (result.IsSuccess)
                            {
                                existCategories.Add(new CategoryDTO()
                                {
                                    Name = categoryName,
                                    ParentCategoryId = parentId
                                });
                            }
                            else
                            {
                                _log.Info("Fail: " + result.Message);
                            }
                        }
                    }
                }
            }

            _log.Info("End SyncCategories");
        }


        public void SendItemCategoriesUpdate(IList<int> itemIds, int? initialIndex)
        {
            //FeatureList = BuildFeatures(item, style, styleFeatures, magentoAttributes)
            _log.Info("Begin SendItemCategoriesUpdate");

            var pageSize = 100;
            var index = initialIndex ?? 0;
            var processedCount = -1;

            var today = _time.GetAppNowTime().Date;
            var notUpdatedSKUs = new List<string>();

            _api.Connect();

            var magentoCategories = RetryHelper.ActionWithRetries(() => CallHelper.ThrowIfFail(_api.GetAllCategories()), _log, throwException: true).Data;

            IList<CustomCategoryDTO> customCategories = null;
            IList<CustomCategoryFilterDTO> customCategoryFilters = null;
            IList<CustomCategoryToStyleDTO> customCategoryToStyles = null;
            using (var db = _dbFactory.GetRDb())
            {
                customCategories = db.CustomCategories.GetAllAsDto().Where(c => c.Market == (int)_api.Market).ToList();
                var customCategoryIds = customCategories.Select(c => c.Id).ToList();
                customCategoryFilters = db.CustomCategoryFilters.GetAllAsDto().Where(f => customCategoryIds.Contains(f.CustomCategoryId)).ToList();
                customCategoryToStyles = db.CustomCategoryToStyles.GetAllAsDto().Where(f => customCategoryIds.Contains(f.CustomCategoryId)).ToList();
            }

            if (itemIds == null)
            {
                using (var db = _dbFactory.GetRWDb())
                {
                    itemIds = db.Items.GetAll()
                        .Where(i => i.Market == (int)_api.Market
                                    && i.ItemPublishedStatus == (int)PublishedStatuses.Published)
                        .Select(i => i.Id)
                        .OrderBy(i => i)
                        .ToList();
                }
            }

            while (processedCount != 0)
            {
                _log.Info("Index: " + index);
                using (var db = _dbFactory.GetRWDb())
                {
                    IList<Item> pageItems = null;

                    var pageItemIds = itemIds.Skip(index).Take(pageSize);

                    pageItems = db.Items.GetAll()
                        .Where(i => pageItemIds.Contains(i.Id))
                        .ToList();

                    var pageListings = db.Listings.GetAll()
                        .Where(l => pageItemIds.Contains(l.ItemId))
                        .ToList();

                    processedCount = pageItems.Count();
                    if (processedCount == 0)
                        continue;

                    var pageStyleIdList = pageItems.Select(i => i.StyleId).Where(i => i != null).ToList();
                    var pageStyleItemIdList = pageItems.Select(i => i.StyleItemId).Where(i => i != null).ToList();
                    var pageStyles = db.Styles.GetAll()
                        .Where(st => pageStyleIdList.Contains(st.Id))
                        .Select(st => new StyleEntireDto()
                        {
                            Id = st.Id,
                            Name = st.Name,
                            StyleID = st.StyleID,
                            MSRP = st.MSRP,
                            BulletPoint1 = st.BulletPoint1,
                            BulletPoint2 = st.BulletPoint2,
                            DropShipperId = st.DropShipperId,
                        })
                        .ToList();

                    var pageListingIds = pageListings.Select(l => l.Id).ToList();
                    var pageListingSales = db.StyleItemSaleToListings.GetAllListingSaleAsDTO()
                        .Where(s => pageListingIds.Contains(s.ListingId)
                            && (!s.SaleStartDate.HasValue || s.SaleStartDate <= today)
                            && (!s.SaleEndDate.HasValue || s.SaleEndDate > today))
                        .ToList();

                    var pageFeatures = db.StyleFeatureTextValues.GetAllWithFeature()
                        .Where(f => pageStyleIdList.Contains(f.StyleId)).ToList();
                    pageFeatures.AddRange(db.StyleFeatureValues.GetAllWithFeature()
                        .Where(f => pageStyleIdList.Contains(f.StyleId)).ToList());

                    foreach (var item in pageItems)
                    {
                        var style = pageStyles.FirstOrDefault(st => st.Id == item.StyleId);
                        if (style == null)
                        {
                            _log.Info("Item w/o style, ASIN=" + item.ASIN);
                            continue;
                        }

                        var listing = pageListings.FirstOrDefault(l => l.ItemId == item.Id);
                        if (listing == null)
                        {
                            _log.Info("Item w/o listing, ASIN=" + item.ASIN);
                            continue;
                        }

                        if (style.BulletPoint2 != "Watches")
                        {
                            _log.Info("Not watches, StyleId=" + style.StyleID);
                            continue;
                        }

                        var listingSale = pageListingSales.FirstOrDefault(s => s.ListingId == listing.Id);

                        var styleFeatures = pageFeatures.Where(f => f.StyleId == style.Id).ToList();
                        styleFeatures.Add(new StyleFeatureValueDTO()
                        {
                            FeatureName = "Name",
                            Value = style.Name,
                        });
                        styleFeatures.Add(new StyleFeatureValueDTO()
                        {
                            FeatureName = "BulletPoint1",
                            Value = style.BulletPoint1,
                        });
                        styleFeatures.Add(new StyleFeatureValueDTO()
                        {
                            FeatureName = "CurrentPrice",
                            Value = listing.CurrentPrice.ToString(),
                        });
                        styleFeatures.Add(new StyleFeatureValueDTO()
                        {
                            FeatureName = "SalePrice",
                            Value = listingSale != null ? listingSale.SalePrice.ToString() : null,
                        });

                        var categoryAttribute = BuildCategoryAttribute(style.Id,
                            styleFeatures,
                            magentoCategories,
                            _valueFeatureMappings,
                            customCategories,
                            customCategoryFilters,
                            customCategoryToStyles,
                            _time.GetAppNowTime());
                        _log.Info("Begin send product categories, SKU=" + listing.SKU);

                        var errorMessage = "";
                        CallResult<bool> result;
                        var retryNumber = 0;
                        var maxRetryCount = 20;
                        do
                        {
                            result = _api.UpdateProductAttributes(listing.SKU,
                                new List<StyleFeatureValueDTO>() { categoryAttribute });
                            if (result.IsFail)
                            {
                                _log.Error("Failed: " + listing.SKU + ": " + result.Message);
                                retryNumber++;
                            }
                        } while (result.IsFail
                            && ((result.Message ?? "").Contains("Unable to save")
                            || (result.Message ?? "").Contains("Database deadlock"))
                            && retryNumber < maxRetryCount);

                        if (retryNumber >= maxRetryCount)
                            notUpdatedSKUs.Add(listing.SKU);
                    }

                    index += pageSize;
                }
            }

            _log.Info("Not updated SKUs: " + String.Join(", ", notUpdatedSKUs));
        }

        public void SendItemAttributesUpdate(IList<int> itemIds, bool includeDesc, bool includeCategory)
        {
            //FeatureList = BuildFeatures(item, style, styleFeatures, magentoAttributes)
            _log.Info("Begin SendItemAttributesUpdate");

            var pageSize = 100;
            var index = 0;
            var processedCount = -1;
            var today = _time.GetAppNowTime().Date;

            _api.Connect();

            var magentoAttributes = _api.GetAllAttributes();
            var magentoCategories = RetryHelper.ActionWithRetries(() => CallHelper.ThrowIfFail(_api.GetAllCategories()), _log, throwException: true).Data;

            IList<CustomCategoryDTO> customCategories = null;
            IList<CustomCategoryFilterDTO> customCategoryFilters = null;
            IList<CustomCategoryToStyleDTO> customCategoryToStyles = null;
            using (var db = _dbFactory.GetRDb())
            {
                customCategories = db.CustomCategories.GetAllAsDto().Where(c => c.Market == (int)_api.Market).ToList();
                var customCategoryIds = customCategories.Select(c => c.Id).ToList();
                customCategoryFilters = db.CustomCategoryFilters.GetAllAsDto().Where(f => customCategoryIds.Contains(f.CustomCategoryId)).ToList();
                customCategoryToStyles = db.CustomCategoryToStyles.GetAllAsDto().Where(f => customCategoryIds.Contains(f.CustomCategoryId)).ToList();
            }

            if (itemIds == null)
            {
                using (var db = _dbFactory.GetRWDb())
                {
                    itemIds = db.Items.GetAll()
                        .Where(i => i.Market == (int)_api.Market
                                    && i.ItemPublishedStatus == (int)PublishedStatuses.Published)
                        .Select(i => i.Id)
                        .OrderBy(i => i)
                        .ToList();
                }
            }

            while (processedCount != 0)
            {
                _log.Info("Index: " + index);
                using (var db = _dbFactory.GetRWDb())
                {
                    IList<Item> pageItems = null;

                    var pageItemIds = itemIds.Skip(index).Take(pageSize);

                    pageItems = db.Items.GetAll()
                        .Where(i => pageItemIds.Contains(i.Id))
                        .ToList();

                    var pageListings = db.Listings.GetAll()
                        .Where(l => pageItemIds.Contains(l.ItemId))
                        .ToList();

                    var pageListingIds = pageListings.Select(l => l.Id).ToList();
                    var pageListingSales = db.StyleItemSaleToListings.GetAllListingSaleAsDTO()
                        .Where(s => pageListingIds.Contains(s.ListingId)
                            && (!s.SaleStartDate.HasValue || s.SaleStartDate <= today)
                            && (!s.SaleEndDate.HasValue || s.SaleEndDate > today))
                        .ToList();

                    processedCount = pageItems.Count();
                    if (processedCount == 0)
                        continue;

                    var pageStyleIdList = pageItems.Select(i => i.StyleId).ToList();
                    IList<StyleEntireDto> pageStyles = null;
                    if (includeDesc)
                    {
                        pageStyles = db.Styles.GetAll()
                            .Where(st => pageStyleIdList.Contains(st.Id))
                            .Select(st => new StyleEntireDto()
                            {
                                Id = st.Id,
                                Name = st.Name,
                                BulletPoint2 = st.BulletPoint2,
                                Description = st.Description,
                                MSRP = st.MSRP,
                                StyleID = st.StyleID,
                                DropShipperId = st.DropShipperId,
                                Manufacturer = st.Manufacturer,
                                CreateDate = st.CreateDate,
                            })
                            .ToList();
                    }
                    else
                    {
                        pageStyles = db.Styles.GetAll()
                            .Where(st => pageStyleIdList.Contains(st.Id))
                            .Select(st => new StyleEntireDto()
                            {
                                Id = st.Id,
                                Name = st.Name,
                                BulletPoint1 = st.BulletPoint1,
                                BulletPoint2 = st.BulletPoint2,
                                MSRP = st.MSRP,
                                StyleID = st.StyleID,
                                DropShipperId = st.DropShipperId,
                                Manufacturer = st.Manufacturer,
                                CreateDate = st.CreateDate,
                            })
                            .ToList();
                    }

                    var pageStyleItemIds = pageItems.Select(i => i.StyleItemId).ToList();
                    var pageDsItems = db.DSItems.GetAllWithCostInfoAsDto()
                        .Where(i => pageStyleItemIds.Contains(i.StyleItemId))
                        .ToList();

                    var pageFeatures = db.StyleFeatureTextValues.GetAllWithFeature()
                        .Where(f => pageStyleIdList.Contains(f.StyleId)).ToList();
                    pageFeatures.AddRange(db.StyleFeatureValues.GetAllWithFeature()
                        .Where(f => pageStyleIdList.Contains(f.StyleId)).ToList());

                    foreach (var item in pageItems)
                    {
                        var style = pageStyles.FirstOrDefault(st => st.Id == item.StyleId);
                        if (style == null)
                        {
                            _log.Info("Item w/o style, ASIN=" + item.ASIN);
                            continue;
                        }

                        var listing = pageListings.FirstOrDefault(l => l.ItemId == item.Id);
                        if (listing == null)
                        {
                            _log.Info("Item w/o listing, ASIN=" + item.ASIN);
                            continue;
                        }

                        if (style.BulletPoint2 != "Watches")
                        {
                            _log.Info("Not watches, StyleId=" + style.StyleID);
                            continue;
                        }

                        var listingSale = pageListingSales.FirstOrDefault(s => s.ListingId == listing.Id);
                        var dsItem = pageDsItems.FirstOrDefault(i => i.StyleItemId == item.StyleItemId
                                                                     && i.DropShipperId == style.DropShipperId);

                        var styleFeatures = pageFeatures.Where(f => f.StyleId == style.Id).ToList();
                        styleFeatures.Add(new StyleFeatureValueDTO()
                        {
                            FeatureName = "Name",
                            Value = style.Name,
                        });
                        styleFeatures.Add(new StyleFeatureValueDTO()
                        {
                            FeatureName = "BulletPoint1",
                            Value = style.BulletPoint1,
                        });
                        styleFeatures.Add(new StyleFeatureValueDTO()
                        {
                            FeatureName = "CurrentPrice",
                            Value = listing.CurrentPrice.ToString(),
                        });
                        styleFeatures.Add(new StyleFeatureValueDTO()
                        {
                            FeatureName = "SalePrice",
                            Value = listingSale != null ? listingSale.SalePrice.ToString() : null,
                        });

                        var isSunglass = false;
                        var productTypeFeature = styleFeatures.FirstOrDefault(f => f.FeatureName == StyleFeatureHelper.ProductType);
                        if (productTypeFeature != null)
                        {
                            isSunglass = StringHelper.IsEqualNoCase(productTypeFeature.Value, "Sunglasses");
                        }

                        if (isSunglass)
                        {
                            var features = db.Features.GetAll();
                            foreach (var feature in features)
                            {
                                var isExist = styleFeatures.Any(sf => sf.FeatureId == feature.Id);
                                if (!isExist)
                                {
                                    styleFeatures.Add(new StyleFeatureValueDTO()
                                    {
                                        FeatureId = feature.Id,
                                        FeatureName = feature.Name,
                                        Value = null
                                    });
                                }
                            }
                        }

                        var attributes = BuildFeaturesAttributes(styleFeatures, magentoAttributes).ToList();
                        attributes.AddRange(BuildBaseAttributes(item, listing, listingSale, style, magentoAttributes));
                        if (includeDesc)
                            attributes.AddRange(BuildMetaAttributes(_log, item, style));
                        if (includeCategory)
                            attributes.Add(BuildCategoryAttribute(style.Id,
                                styleFeatures,
                                magentoCategories,
                                _valueFeatureMappings,
                                customCategories,
                                customCategoryFilters,
                                customCategoryToStyles,
                                _time.GetAppNowTime()));

                        _log.Info("Begin send product attributes, SKU=" + listing.SKU);

                        _api.UpdateProductAttributes(listing.SKU, attributes);
                    }

                    index += pageSize;
                }
            }
        }

        public void SendItemImagesUpdate(IList<int> itemIds, int? initialIndex)
        {
            //FeatureList = BuildFeatures(item, style, styleFeatures, magentoAttributes)
            _log.Info("Begin SendItemImagesUpdate");

            var pageSize = 100;
            var index = initialIndex ?? 0;
            var processedCount = -1;
            var threadCount = 1;

            _api.Connect();

            if (itemIds == null)
            {
                using (var db = _dbFactory.GetRWDb())
                {
                    itemIds = db.Items.GetAll()
                        .Where(i => i.Market == (int)_api.Market
                                    && i.ItemPublishedStatus == (int)PublishedStatuses.Published)
                        .Select(i => i.Id)
                        .OrderBy(i => i)
                        .ToList();
                }
            }

            _log.Info("Total items: " + itemIds.Count());

            while (processedCount != 0)
            {
                _log.Info("Index: " + index);
                using (var db = _dbFactory.GetRWDb())
                {
                    IList<Item> pageItems = null;

                    var pageItemIds = itemIds.Skip(index).Take(pageSize);

                    pageItems = db.Items.GetAll()
                        .Where(i => pageItemIds.Contains(i.Id))
                        .ToList();

                    var pageListings = db.Listings.GetAll()
                        .Where(l => pageItemIds.Contains(l.ItemId))
                        .ToList();

                    processedCount = pageItems.Count();
                    if (processedCount == 0)
                        continue;

                    var pageStyleIdList = pageItems.Select(i => i.StyleId).ToList();
                    var pageStyles = db.Styles.GetAll().
                        Select(st => new StyleEntireDto()
                        {
                            Id = st.Id,
                            Name = st.Name,
                        })
                        .Where(st => pageStyleIdList.Contains(st.Id))
                        .ToList();
                    var pageStyleImages = db.StyleImages.GetAllAsDto()
                        .Where(stim => pageStyleIdList.Contains(stim.StyleId))
                        .OrderBy(sim => sim.Id).ToList();

                    IList<List<Item>> threadItemList = new List<List<Item>>();
                    var take = Math.Max(1, pageItems.Count / threadCount);
                    var skip = 0;
                    for (int i = 0; i < threadCount; i++)
                    {
                        if (skip >= pageItems.Count)
                            break;

                        var threadItems = pageItems.Skip(skip).Take(take).ToList();
                        if (threadItems.Count == 0)
                            break;

                        threadItemList.Add(threadItems);
                        skip = skip + take;
                    }

                    var threads = new List<Thread>();
                    foreach (var threadItems in threadItemList)
                    {
                        var newThread = new Thread((items)
                            => SendItemCallback(_api, pageStyles, pageListings, pageStyleImages, (IList<Item>)items));
                        threads.Add(newThread);
                        newThread.Start(threadItems);
                    }

                    threads.ForEach(t => t.Join()); //Wait all

                    index += pageSize;
                }
            }
        }

        private void SendItemCallback(Magento20MarketApi api,
            IList<StyleEntireDto> pageStyles,
            IList<Listing> pageListings,
            IList<StyleImageDTO> pageStyleImages,
            IList<Item> threadItems)
        {
            foreach (var item in threadItems)
            {
                var style = pageStyles.FirstOrDefault(st => st.Id == item.StyleId);
                if (style == null)
                {
                    _log.Info("Item w/o style, ASIN=" + item.ASIN);
                    continue;
                }

                var listing = pageListings.FirstOrDefault(l => l.ItemId == item.Id);
                if (listing == null)
                {
                    _log.Info("Item w/o listing, ASIN=" + item.ASIN);
                    continue;
                }

                var styleImages = pageStyleImages.Where(sim => sim.StyleId == item.StyleId);
                var images = styleImages.Select(i => BuildImage(i.Image)).Where(i => i != null).ToList();
                images.ForEach(im => im.Label = style.Name);

                _log.Info("Begin send product images, SKU=" + listing.SKU + ", Count=" + images.Count());

                _api.UpdateProductImages(listing.SKU, images);

                _log.Info("End send");
            }
        }


        public void SendParentItemUpdates(IList<int> parentItemIds)
        {
            _log.Info("Begin SyncParentItems");

            var pageSize = 50;
            var index = 0;
            var processedCount = -1;
            var today = _time.GetAppNowTime().Date;

            _api.Connect();
            var magentoAttributes = _api.GetAllAttributes();
            var magentoCategories = RetryHelper.ActionWithRetries(() => CallHelper.ThrowIfFail(_api.GetAllCategories()), _log, throwException: true).Data;

            IList<CustomCategoryDTO> customCategories = null;
            IList<CustomCategoryFilterDTO> customCategoryFilters = null;
            IList<CustomCategoryToStyleDTO> customCategoryToStyles = null;
            using (var db = _dbFactory.GetRDb())
            {
                customCategories = db.CustomCategories.GetAllAsDto().Where(c => c.Market == (int)_api.Market).ToList();
                var customCategoryIds = customCategories.Select(c => c.Id).ToList();
                customCategoryFilters = db.CustomCategoryFilters.GetAllAsDto().Where(f => customCategoryIds.Contains(f.CustomCategoryId)).ToList();
                customCategoryToStyles = db.CustomCategoryToStyles.GetAllAsDto().Where(f => customCategoryIds.Contains(f.CustomCategoryId)).ToList();
            }

            _log.Info("Total parent items: " + parentItemIds.Count());

            while (processedCount != 0)
            {
                _log.Info("index: " + index);
                using (var db = _dbFactory.GetRWDb())
                {
                    //IList<Item> pageItems = null;

                    var pageParentItemIds = parentItemIds.Skip(index).Take(pageSize);

                    var pageParentItems = db.ParentItems.GetAll()
                        .Where(pi => pi.Market == (int)_api.Market
                            && pageParentItemIds.Contains(pi.Id))
                        .ToList();

                    processedCount = pageParentItems.Count();
                    if (processedCount == 0)
                        continue;

                    var parentItemAsinList = pageParentItems.Select(i => i.ASIN).ToList();
                    
                    var pageItems = db.Items.GetAll()
                        .Where(i => parentItemAsinList.Contains(i.ParentASIN)
                            && i.Market == (int)_api.Market)
                        .ToList();

                    var pageItemIdList = pageItems.Select(i => i.Id).ToList();
                    var pageListings = db.Listings.GetAll().Where(l => l.Market == (int)_api.Market
                        && pageItemIdList.Contains(l.ItemId)).ToList();

                    var pageListingIds = pageListings.Select(l => l.Id).ToList();
                    var pageListingSales = db.StyleItemSaleToListings.GetAllListingSaleAsDTO()
                        .Where(s => pageListingIds.Contains(s.ListingId)
                            && (!s.SaleStartDate.HasValue || s.SaleStartDate <= today)
                            && (!s.SaleEndDate.HasValue || s.SaleEndDate > today))
                        .ToList();

                    var pageStyleIdList = pageItems.Select(i => i.StyleId).ToList();
                    var pageStyles = db.Styles.GetAllAsDtoEx().Where(st => pageStyleIdList.Contains(st.Id)).ToList();
                    var pageStyleImages = db.StyleImages.GetAll()
                        .Where(stim => pageStyleIdList.Contains(stim.StyleId))
                        .OrderByDescending(i => i.IsDefault)
                        .ThenBy(i => i.Id)
                        .ToList();

                    var pageStyleItemIds = pageItems.Select(i => i.StyleItemId).ToList();

                    var pageFeatures = db.StyleFeatureTextValues.GetAllWithFeature()
                        .Where(f => pageStyleIdList.Contains(f.StyleId)).ToList();
                    pageFeatures.AddRange(db.StyleFeatureValues.GetAllWithFeature()
                        .Where(f => pageStyleIdList.Contains(f.StyleId)).ToList());
                    pageFeatures = pageFeatures.GroupBy(f => new { f.FeatureId, f.StyleId }, f => f)
                        .Select(f => f.FirstOrDefault())
                        .ToList();
                                       
                    if (pageParentItems.Any())
                    {
                        _api.Connect();
                        foreach (var parentItem in pageParentItems)
                        {
                            try
                            {
                                var items = pageItems.Where(i => i.ParentASIN == parentItem.ASIN
                                    && !String.IsNullOrEmpty(i.SourceMarketId)).ToList();
                                var itemIds = items.Select(i => i.Id).ToList();

                                var listings = pageListings.Where(l => itemIds.Contains(l.ItemId)).ToList();
                                var mainListing = listings.OrderByDescending(l => l.IsDefault).ThenBy(l => l.Id).FirstOrDefault();

                                if (mainListing == null)
                                    throw new ArgumentNullException("mainListing");

                                var mainItem = items.FirstOrDefault(i => i.Id == mainListing.ItemId);
                                if (mainItem == null)
                                    throw new ArgumentNullException("mainItem");

                                var mainStyle = pageStyles.FirstOrDefault(st => st.Id == mainItem.StyleId);
                                if (mainStyle == null)
                                    throw new ArgumentNullException("style");

                                if (mainListing.CurrentPrice <= 0.01M)
                                {
                                    _log.Info("No prices, StyleId=" + mainStyle.StyleID);
                                    continue;
                                }

                                var listingSale = pageListingSales.FirstOrDefault(s => s.ListingId == mainListing.Id);
                                var styleFeatures = pageFeatures.Where(f => f.StyleId == mainStyle.Id).ToList();
                                var name = mainStyle.Name;

                                styleFeatures.Add(new StyleFeatureValueDTO()
                                {
                                    FeatureName = "Name",
                                    Value = name,
                                });
                                styleFeatures.Add(new StyleFeatureValueDTO()
                                {
                                    FeatureName = "BulletPoint1",
                                    Value = mainStyle.BulletPoint1,
                                });

                                var attributeSetId = 4; //4 - Default

                                var styleImages = pageStyleImages.Where(im => im.StyleId == mainStyle.Id).ToList();

                                var styleImageContents = styleImages.Select(i => BuildImage(i.Image)).Where(i => i != null).ToList();
                                if (styleImageContents.Count == 0)
                                {
                                    _log.Info("No images, StyleId=" + mainStyle.StyleID);
                                    continue;
                                }

                                _log.Info("Begin send product, ASIN=" + parentItem.ASIN + ", SKU=" + mainListing.SKU +
                                          ", styleId=" + mainStyle.StyleID +
                                          ", featureCount=" + styleFeatures.Count() +
                                          ", imageCount=" + styleImages.Count());


                                var model = new ParentItemDTO()
                                {
                                    SKU = parentItem.SKU,
                                    AmazonName = parentItem.AmazonName,

                                    Variations = new List<ItemDTO>()
                                    {
                                        new ItemDTO()
                                        {
                                            SKU = mainListing.SKU,

                                            CurrentPrice = mainListing.CurrentPrice,
                                            ListPrice = mainStyle.MSRP,

                                            RealQuantity = mainListing.RealQuantity,

                                            BrandName = mainStyle.Manufacturer,

                                            Name = name,
                                            Description = mainStyle.Description,
                                            ImageUrl = mainStyle.Image,

                                            SearchKeywords = mainStyle.BulletPoint1,

                                            Images = styleImageContents,

                                            FeatureList = BuildAllAttributes(mainItem,
                                                mainListing,
                                                listingSale,
                                                mainStyle,
                                                styleFeatures,
                                                magentoAttributes,
                                                magentoCategories,
                                                _valueFeatureMappings,
                                                customCategories,
                                                customCategoryFilters,
                                                customCategoryToStyles,
                                                _time.GetAppNowTime())
                                            //TODO:
                                            //Weight = 
                                        }
                                    }
                                };

                                IList<ConfigurableProductOption> options = new List<ConfigurableProductOption>();
                                var sizes = items.Where(i => !String.IsNullOrEmpty(i.Size)).Select(i => i.Size).ToList();
                                if (sizes.Distinct().Count() > 1)
                                {
                                    var sizeAttribute = magentoAttributes.FirstOrDefault(a => a.Name == PAMagentoFeatures.SizeAttributeName);
                                    if (sizeAttribute != null)
                                    {
                                        options.Add(new ConfigurableProductOption()
                                        {
                                            AttributeId = sizeAttribute.Id.ToString(),
                                            Label = "Size",
                                            Position = 0,
                                            Values = sizes.Select(s => new ValueIndexItem()
                                            {
                                                ValueIndex = StringHelper.TryGetInt(sizeAttribute.FeatureValues.FirstOrDefault(fv => fv.Value == s)?.ExtendedValue) ?? 0,
                                            }).ToList()
                                        });
                                    }
                                }

                                var colors = items.Where(i => !String.IsNullOrEmpty(i.Color)).Select(i => i.Color).ToList();
                                if (colors.Distinct().Count() > 1)
                                {
                                    var colorAttribute = magentoAttributes.FirstOrDefault(a => a.Name == PAMagentoFeatures.ColorAttributeName);
                                    if (colorAttribute != null)
                                    {
                                        options.Add(new ConfigurableProductOption()
                                        {
                                            AttributeId = colorAttribute.Id.ToString(),
                                            Label = "Color",
                                            Position = 1,
                                            Values = colors.Select(s => new ValueIndexItem()
                                            {
                                                ValueIndex = colorAttribute.FeatureValues.FirstOrDefault(fv => fv.Value == s)?.Id ?? 0,
                                            }).ToList()
                                        });
                                    }
                                }

                                var childItemMarketIds = items.Select(i => StringHelper.ToInt(i.SourceMarketId))
                                    .Where(i => i.HasValue)
                                    .ToList()
                                    .Select(i => (long)i.Value)
                                    .ToList();

                                var result = _api.CreateOrUpdateConfigurableProduct(model,
                                    attributeSetId,
                                    configurableOptions: options,
                                    childItemMarketIds: childItemMarketIds);

                                //Keep productId, productUrl
                                if (result.IsSuccess)
                                {
                                    mainItem.SourceMarketId = result.Data.ToString();
                                    mainItem.ItemPublishedStatus = (int)PublishedStatuses.Published;
                                    mainItem.ItemPublishedStatusDate = _time.GetAppNowTime();
                                    parentItem.SourceMarketId = result.Data.ToString();
                                    db.Commit();
                                }
                                else
                                {
                                    _log.Error("Create or update was failed: " + result.Message);
                                    mainItem.ItemPublishedStatus = (int)PublishedStatuses.PublishingErrors;
                                    mainItem.ItemPublishedStatusDate = _time.GetAppNowTime();
                                }

                                _log.Info("End send product");
                            }
                            catch (Exception ex)
                            {
                                _log.Info("Error send product", ex);
                            }
                        }
                        db.Commit();

                        _api.Disconnect();
                    }

                    index += pageSize;
                }
            }

            _log.Info("End SyncParentItems");
        }



        public void SendItemUpdates()
        {
            SendItemUpdates(null);
        }

        public void SendItemUpdates(IList<int> itemIds)
        {
            _log.Info("Begin SyncItems");

            var pageSize = 50;
            var index = 0;
            var processedCount = -1;
            var today = _time.GetAppNowTime().Date;

            _api.Connect();
            var magentoAttributes = _api.GetAllAttributes();
            var magentoCategories = RetryHelper.ActionWithRetries(() => CallHelper.ThrowIfFail(_api.GetAllCategories()), _log, throwException: true).Data;

            IList<CustomCategoryDTO> customCategories = null;
            IList<CustomCategoryFilterDTO> customCategoryFilters = null;
            IList<CustomCategoryToStyleDTO> customCategoryToStyles = null;
            using (var db = _dbFactory.GetRDb())
            {
                customCategories = db.CustomCategories.GetAllAsDto().Where(c => c.Market == (int)_api.Market).ToList();
                var customCategoryIds = customCategories.Select(c => c.Id).ToList();
                customCategoryFilters = db.CustomCategoryFilters.GetAllAsDto().Where(f => customCategoryIds.Contains(f.CustomCategoryId)).ToList();
                customCategoryToStyles = db.CustomCategoryToStyles.GetAllAsDto().Where(f => customCategoryIds.Contains(f.CustomCategoryId)).ToList();
            }

            if (itemIds == null)
            {
                var toErrorDate = _time.GetAppNowTime().AddHours(-30);
                var toInProgressDate = _time.GetAppNowTime().AddHours(-30);
                using (var db = _dbFactory.GetRDb())
                {
                    itemIds = db.Items.GetAll()
                        .Where(i => i.Market == (int)_api.Market
                             && (i.ItemPublishedStatus == (int)PublishedStatuses.None
                             || i.ItemPublishedStatus == (int)PublishedStatuses.New
                             || i.ItemPublishedStatus == (int)PublishedStatuses.HasChanges
                             || i.ItemPublishedStatus == (int)PublishedStatuses.HasChangesWithProductId
                             || i.ItemPublishedStatus == (int)PublishedStatuses.HasChangesWithSKU
                             || (i.ItemPublishedStatus == (int)PublishedStatuses.PublishingErrors
                                && i.ItemPublishedStatusDate < toErrorDate)
                             || ((i.ItemPublishedStatus == (int)PublishedStatuses.PublishedInProgress
                                  || i.ItemPublishedStatus == (int)PublishedStatuses.ChangesSubmited)
                                  && i.ItemPublishedStatusDate < toInProgressDate)))
                        .OrderBy(i => i.Id)
                        .Select(i => i.Id)
                        .ToList();
                }
            }

            _log.Info("Total items: " + itemIds.Count());

            while (processedCount != 0)
            {
                _log.Info("index: " + index);
                using (var db = _dbFactory.GetRWDb())
                {
                    //IList<Item> pageItems = null;

                    var pageItemIds = itemIds.Skip(index).Take(pageSize);

                    var previewPageItems = db.Items.GetAll()
                        .Where(i => pageItemIds.Contains(i.Id))
                        .ToList();

                    processedCount = previewPageItems.Count();
                    if (processedCount == 0)
                        continue;

                    var parentItemAsinList = previewPageItems.Select(i => i.ParentASIN).Distinct().ToList();
                    var pageParentItems = db.ParentItems.GetAll()
                        .Where(pi => pi.Market == (int)_api.Market
                            && parentItemAsinList.Contains(pi.ASIN))
                        .ToList();

                    var pageItems = db.Items.GetAll()
                        .Where(i => parentItemAsinList.Contains(i.ParentASIN)
                            && i.Market == (int)_api.Market)
                        .ToList();

                    var pageItemIdList = pageItems.Select(i => i.Id).ToList();
                    var pageListings = db.Listings.GetAll().Where(l => l.Market == (int)_api.Market
                        && pageItemIdList.Contains(l.ItemId)).ToList();

                    var pageListingIds = pageListings.Select(l => l.Id).ToList();
                    var pageListingSales = db.StyleItemSaleToListings.GetAllListingSaleAsDTO()
                        .Where(s => pageListingIds.Contains(s.ListingId)
                            && (!s.SaleStartDate.HasValue || s.SaleStartDate <= today)
                            && (!s.SaleEndDate.HasValue || s.SaleEndDate > today))
                        .ToList();

                    var pageStyleIdList = pageItems.Select(i => i.StyleId).ToList();
                    var pageStyles = db.Styles.GetAllAsDtoEx().Where(st => pageStyleIdList.Contains(st.Id)).ToList();
                    var pageStyleImages = db.StyleImages.GetAll()
                        .Where(stim => pageStyleIdList.Contains(stim.StyleId))
                        .OrderByDescending(i => i.IsDefault)
                        .ThenBy(i => i.Id)
                        .ToList();

                    var pageStyleItemIds = pageItems.Select(i => i.StyleItemId).ToList();

                    var pageFeatures = db.StyleFeatureTextValues.GetAllWithFeature()
                        .Where(f => pageStyleIdList.Contains(f.StyleId)).ToList();
                    pageFeatures.AddRange(db.StyleFeatureValues.GetAllWithFeature()
                        .Where(f => pageStyleIdList.Contains(f.StyleId)).ToList());
                    pageFeatures = pageFeatures.GroupBy(f => new { f.FeatureId, f.StyleId }, f => f)
                        .Select(f => f.FirstOrDefault())
                        .ToList();

                    #region Items Update
                    if (pageItems.Any())
                    {
                        _api.Connect();
                        foreach (var item in pageItems)
                        {
                            try
                            {
                                var parentItem = pageParentItems.FirstOrDefault(i => i.ASIN == item.ParentASIN);
                                if (parentItem == null)
                                    throw new ArgumentNullException("parentItem");
                                //var item = pageItems.FirstOrDefault(i => i.ParentASIN == parentItem.ASIN);
                                //if (item == null)
                                //    throw new ArgumentNullException("item");

                                var listing = pageListings.FirstOrDefault(l => l.ItemId == item.Id);
                                if (listing == null)
                                    throw new ArgumentNullException("listing");

                                var style = pageStyles.FirstOrDefault(st => st.Id == item.StyleId);
                                if (style == null)
                                    throw new ArgumentNullException("style");

                                if (listing.CurrentPrice <= 0.01M)
                                {
                                    _log.Info("No prices, StyleId=" + style.StyleID);
                                    continue;
                                }

                                var listingSale = pageListingSales.FirstOrDefault(s => s.ListingId == listing.Id);
                                var styleFeatures = pageFeatures.Where(f => f.StyleId == style.Id).ToList();
                                var name = StringHelper.Join(", ", StringHelper.Trim(style.Name, ",.!? ".ToCharArray()), item.Color, "Size " + item.Size);

                                styleFeatures.Add(new StyleFeatureValueDTO()
                                {
                                    FeatureName = "Name",
                                    Value = name,
                                });
                                styleFeatures.Add(new StyleFeatureValueDTO()
                                {
                                    FeatureName = "BulletPoint1",
                                    Value = style.BulletPoint1,
                                });
                                styleFeatures.Add(new StyleFeatureValueDTO()
                                {
                                    FeatureName = "CurrentPrice",
                                    Value = listing.CurrentPrice.ToString(),
                                });
                                styleFeatures.Add(new StyleFeatureValueDTO()
                                {
                                    FeatureName = "SalePrice",
                                    Value = listingSale != null ? listingSale.SalePrice.ToString() : null,
                                });

                                var attributeSetId = 4; //4 - Default

                                var styleImages = pageStyleImages.Where(im => im.StyleId == style.Id).ToList();

                                var styleImageContents = styleImages.Select(i => BuildImage(i.Image)).Where(i => i != null).ToList();
                                if (styleImageContents.Count == 0)
                                {
                                    _log.Info("No images, StyleId=" + style.StyleID);
                                    continue;
                                }

                                _log.Info("Begin send product, ASIN=" + parentItem.ASIN + ", SKU=" + listing.SKU +
                                          ", styleId=" + style.StyleID +
                                          ", featureCount=" + styleFeatures.Count() +
                                          ", imageCount=" + styleImages.Count());


                                var model = new ParentItemDTO()
                                {
                                    Variations = new List<ItemDTO>()
                                    {
                                        new ItemDTO()
                                        {
                                            SKU = listing.SKU,
                                            Barcode = item.Barcode,

                                            CurrentPrice = listing.CurrentPrice,
                                            ListPrice = style.MSRP,

                                            RealQuantity = listing.RealQuantity,

                                            BrandName = style.Manufacturer,

                                            Name = name,
                                            Description = style.Description,
                                            ImageUrl = style.Image,

                                            SearchKeywords = style.BulletPoint1,

                                            Images = styleImageContents,

                                            FeatureList = BuildAllAttributes(item,
                                                listing,
                                                listingSale,
                                                style,
                                                styleFeatures,
                                                magentoAttributes,
                                                magentoCategories,
                                                _valueFeatureMappings,
                                                customCategories,
                                                customCategoryFilters,
                                                customCategoryToStyles,
                                                _time.GetAppNowTime())
                                            //TODO:
                                            //Weight = 
                                        }
                                    }
                                };

                                var result = _api.CreateOrUpdateProduct(model, 
                                    attributeSetId,
                                    null,
                                    isHidden: true);

                                //Keep productId, productUrl
                                if (result.IsSuccess)
                                {
                                    item.SourceMarketId = result.Data.ToString();
                                    item.ItemPublishedStatus = (int)PublishedStatuses.Published;
                                    item.ItemPublishedStatusDate = _time.GetAppNowTime();
                                    parentItem.SourceMarketId = result.Data.ToString();
                                    db.Commit();
                                }
                                else
                                {
                                    _log.Error("Create or update was failed: " + result.Message);
                                    item.ItemPublishedStatus = (int)PublishedStatuses.PublishingErrors;
                                    item.ItemPublishedStatusDate = _time.GetAppNowTime();
                                }

                                _log.Info("End send product");
                            }
                            catch (Exception ex)
                            {
                                _log.Info("Error send product", ex);
                            }
                        }
                        db.Commit();

                        _api.Disconnect();
                    }
                    #endregion

                    #region Parent Items Update
                    if (pageParentItems.Any())
                    {
                        _api.Connect();
                        foreach (var parentItem in pageParentItems)
                        {
                            try
                            {
                                var items = pageItems.Where(i => i.ParentASIN == parentItem.ASIN
                                    && !String.IsNullOrEmpty(i.SourceMarketId)).ToList();
                                var item = items.FirstOrDefault();

                                var listing = pageListings.FirstOrDefault(l => l.ItemId == item.Id);
                                if (listing == null)
                                    throw new ArgumentNullException("listing");

                                var style = pageStyles.FirstOrDefault(st => st.Id == item.StyleId);
                                if (style == null)
                                    throw new ArgumentNullException("style");

                                if (listing.CurrentPrice <= 0.01M)
                                {
                                    _log.Info("No prices, StyleId=" + style.StyleID);
                                    continue;
                                }

                                var listingSale = pageListingSales.FirstOrDefault(s => s.ListingId == listing.Id);
                                var styleFeatures = pageFeatures.Where(f => f.StyleId == style.Id).ToList();
                                var name = style.Name;

                                styleFeatures.Add(new StyleFeatureValueDTO()
                                {
                                    FeatureName = "Name",
                                    Value = name,
                                });
                                styleFeatures.Add(new StyleFeatureValueDTO()
                                {
                                    FeatureName = "BulletPoint1",
                                    Value = style.BulletPoint1,
                                });

                                var attributeSetId = 4; //4 - Default

                                var styleImages = pageStyleImages.Where(im => im.StyleId == style.Id).ToList();

                                var styleImageContents = styleImages.Select(i => BuildImage(i.Image)).Where(i => i != null).ToList();
                                if (styleImageContents.Count == 0)
                                {
                                    _log.Info("No images, StyleId=" + style.StyleID);
                                    continue;
                                }

                                _log.Info("Begin send product, ASIN=" + parentItem.ASIN + ", SKU=" + listing.SKU +
                                          ", styleId=" + style.StyleID +
                                          ", featureCount=" + styleFeatures.Count() +
                                          ", imageCount=" + styleImages.Count());


                                var model = new ParentItemDTO()
                                {
                                    Variations = new List<ItemDTO>()
                                    {
                                        new ItemDTO()
                                        {
                                            SKU = listing.SKU,

                                            //CurrentPrice = listing.CurrentPrice,
                                            //ListPrice = style.MSRP,

                                            //RealQuantity = listing.RealQuantity,

                                            BrandName = style.Manufacturer,

                                            Name = name,
                                            Description = style.Description,
                                            ImageUrl = style.Image,

                                            SearchKeywords = style.BulletPoint1,

                                            Images = styleImageContents,

                                            FeatureList = BuildAllAttributes(item,
                                                listing,
                                                listingSale,
                                                style,
                                                styleFeatures,
                                                magentoAttributes,
                                                magentoCategories,
                                                _valueFeatureMappings,
                                                customCategories,
                                                customCategoryFilters,
                                                customCategoryToStyles,
                                                _time.GetAppNowTime())
                                            //TODO:
                                            //Weight = 
                                        }
                                    }
                                };

                                IList<ConfigurableProductOption> options = new List<ConfigurableProductOption>();
                                var sizes = items.Where(i => !String.IsNullOrEmpty(i.Size)).Select(i => i.Size).ToList();
                                if (sizes.Distinct().Count() > 1)
                                {
                                    var sizeAttribute = magentoAttributes.FirstOrDefault(a => a.Name == PAMagentoFeatures.SizeAttributeName);
                                    if (sizeAttribute != null)
                                    {
                                        options.Add(new ConfigurableProductOption()
                                        {
                                            AttributeId = sizeAttribute.Id.ToString(),
                                            Label = "Size",
                                            Position = 0,
                                            Values = sizes.Select(s => new ValueIndexItem()
                                            {
                                                ValueIndex = sizeAttribute.FeatureValues.FirstOrDefault(fv => fv.Value == s)?.Id ?? 0,
                                            }).ToList()
                                        });
                                    }
                                }

                                var colors = items.Where(i => !String.IsNullOrEmpty(i.Color)).Select(i => i.Color).ToList();
                                if (colors.Distinct().Count() > 1)
                                {
                                    var colorAttribute = magentoAttributes.FirstOrDefault(a => a.Name == PAMagentoFeatures.ColorAttributeName);
                                    if (colorAttribute != null)
                                    {
                                        options.Add(new ConfigurableProductOption()
                                        {
                                            AttributeId = colorAttribute.Id.ToString(),
                                            Label = "Color",
                                            Position = 1,
                                            Values = colors.Select(s => new ValueIndexItem()
                                            {
                                                ValueIndex = colorAttribute.FeatureValues.FirstOrDefault(fv => fv.Value == s)?.Id ?? 0,
                                            }).ToList()
                                        });
                                    }
                                }

                                var childItemMarketIds = items.Select(i => StringHelper.ToInt(i.SourceMarketId))
                                    .Where(i => i.HasValue)
                                    .ToList()
                                    .Select(i => (long)i.Value)
                                    .ToList();

                                var result = _api.CreateOrUpdateProduct(model, 
                                    attributeSetId,
                                    null,
                                    isHidden: false);

                                //Keep productId, productUrl
                                if (result.IsSuccess)
                                {
                                    item.SourceMarketId = result.Data.ToString();
                                    item.ItemPublishedStatus = (int)PublishedStatuses.Published;
                                    item.ItemPublishedStatusDate = _time.GetAppNowTime();
                                    parentItem.SourceMarketId = result.Data.ToString();
                                    db.Commit();
                                }
                                else
                                {
                                    _log.Error("Create or update was failed: " + result.Message);
                                    item.ItemPublishedStatus = (int)PublishedStatuses.PublishingErrors;
                                    item.ItemPublishedStatusDate = _time.GetAppNowTime();
                                }

                                _log.Info("End send product");
                            }
                            catch (Exception ex)
                            {
                                _log.Info("Error send product", ex);
                            }
                        }
                        db.Commit();

                        _api.Disconnect();
                    }

                    #endregion

                    index += pageSize;
                }
            }

            _log.Info("End SyncItems");
        }

        public IList<StyleFeatureValueDTO> BuildAllAttributes(Item item,
            Listing listing,
            ViewListingSaleDTO listingSale,
            StyleEntireDto style,
            IList<StyleFeatureValueDTO> styleFeatures,
            IList<FeatureDTO> magentoAttributes,
            IList<CategoryDTO> magentoCategories,
            IList<CategoryMappingInfo> categoryMaps,
            IList<CustomCategoryDTO> customCategories,
            IList<CustomCategoryFilterDTO> customCategoryFilters,
            IList<CustomCategoryToStyleDTO> customCategoryToStyles,
            DateTime when)
        {
            var results = (List<StyleFeatureValueDTO>)BuildBaseAttributes(item, listing, listingSale, style, magentoAttributes);
            results.AddRange(BuildMetaAttributes(_log, item, style));
            results.AddRange(BuildFeaturesAttributes(styleFeatures, magentoAttributes));
            results.Add(BuildCategoryAttribute(style.Id, styleFeatures, magentoCategories, categoryMaps, customCategories, customCategoryFilters, customCategoryToStyles, when));

            return results;
        }

        public static StyleFeatureValueDTO BuildCategoryAttribute(long styleId,
            IList<StyleFeatureValueDTO> styleFeatures,
            IList<CategoryDTO> magentoCategories,
            IList<CategoryMappingInfo> categoryMaps,
            IList<CustomCategoryDTO> customCategories,
            IList<CustomCategoryFilterDTO> customCategoryFilters,
            IList<CustomCategoryToStyleDTO> customCategoryToStyles,
            DateTime when)
        {
            var categories = new List<CategoryDTO>();// BuildFeaturesCategories(styleFeatures, magentoCategories, categoryMaps).ToList();
            categories.AddRange(BuildCustomCategories(styleId, styleFeatures, magentoCategories, categoryMaps, customCategories, customCategoryFilters, customCategoryToStyles, when));
            var result = new StyleFeatureValueDTO()
            {
                FeatureName = PAMagentoFeatures.CategoryAttributeName,
                ObjValue = categories.Select(c => c.CategoryId.ToString()).ToArray()
            };

            return result;
        }

        public static IList<CategoryDTO> BuildCustomCategories(long styleId,
            IList<StyleFeatureValueDTO> styleFeatures,
            IList<CategoryDTO> magentoCategories,
            IList<CategoryMappingInfo> categoryMaps,
            IList<CustomCategoryDTO> customCategories,
            IList<CustomCategoryFilterDTO> customCateogryFilters,
            IList<CustomCategoryToStyleDTO> customCategoryToStyles,
            DateTime when)
        {
            var results = new List<CategoryDTO>();

            var exCategories = styleFeatures.FirstOrDefault(fv => fv.FeatureId == StyleFeatureHelper.ADDITIONAL_CATEGORIES);
            if (exCategories != null && !String.IsNullOrEmpty(exCategories.Value))
            {
                var categoryNames = exCategories.Value.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var categoryName in categoryNames)
                {
                    var magentoCategory = magentoCategories.FirstOrDefault(c => MagentoCategoryHelper.CompareCategoryNames(c.Name, categoryName, null));
                    if (magentoCategory != null)
                    {
                        results.Add(new CategoryDTO()
                        {
                            CategoryId = magentoCategory.CategoryId
                        });
                    }
                }
            }

            foreach (var customCategory in customCategories)
            {
                CategoryDTO magentoCategory = null;
                if (customCategory.CategoryId.HasValue)
                {
                    magentoCategory = magentoCategories.FirstOrDefault(m => m.CategoryId == customCategory.CategoryId.Value);                    
                }
                else
                {
                    if (!String.IsNullOrEmpty(customCategory.CategoryPath))
                    {
                        string[] parts = customCategory.CategoryPath.Split("/\\".ToCharArray()).Select(i => i.Trim()).ToArray();

                        int parentCategoryId = RootParentId;
                        foreach (var part in parts)
                        {
                            magentoCategory = magentoCategories.FirstOrDefault(c => c.ParentCategoryId == parentCategoryId
                                && MagentoCategoryHelper.CompareCategoryNames(c.Name, part, null));
                            if (magentoCategory == null)
                                break;

                            parentCategoryId = magentoCategory.CategoryId ?? 0;
                        }
                    }
                    else
                    {
                        magentoCategory = magentoCategories.FirstOrDefault(c => MagentoCategoryHelper.CompareCategoryNames(c.Name, customCategory.Name, null));
                    }
                }

                if (magentoCategory == null)
                {
                    //_log.Warn("Unable to find Magento custom cateogry: " + customCategory.Name);
                    continue;
                }

                var passed = true;
                if (customCategory.Mode == (int)CustomCategoryModes.Temp)
                {
                    passed = false;
                }

                if (customCategory.Mode == (int)CustomCategoryModes.Rule)
                {
                    var categoryFilters =
                        customCateogryFilters.Where(f => f.CustomCategoryId == customCategory.Id).ToList();
                    if (!categoryFilters.Any())
                    {
                        continue;
                    }

                    foreach (var filter in categoryFilters)
                    {
                        var attributePassed = false;
                        var attribute = styleFeatures.FirstOrDefault(sf => sf.FeatureName == filter.AttributeName);
                        if (attribute != null)
                        {
                            decimal? attributeValue = null;
                            decimal? intValue = null;
                            var values = (filter.AttributeValues ?? "").Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                            switch (filter.Operation)
                            {
                                case "Equals":
                                    attributePassed =
                                        String.Compare(attribute.Value, values.FirstOrDefault(), StringComparison.OrdinalIgnoreCase) == 0;
                                    break;
                                case "Contains":
                                    attributePassed =
                                        (attribute.Value ?? "").IndexOf(values.FirstOrDefault(), StringComparison.OrdinalIgnoreCase) >= 0;
                                    break;
                                case "In":
                                    attributePassed = StringHelper.EqualWithOneOfStrings(attribute.Value, values);
                                    break;
                                case "NotContains":
                                    attributePassed = (attribute.Value ?? "").IndexOf(values.FirstOrDefault(), StringComparison.OrdinalIgnoreCase) < 0;
                                    break;
                                case "MoreThan":
                                    //NOTE: Should be first, in case of value: 100 m (10 ATM)
                                    attributeValue = StringHelper.TryGetDecimal(attribute.Value);
                                    if (attributeValue == null)
                                        attributeValue = StringHelper.GetFirstDigitSequences(attribute.Value);
                                    intValue = StringHelper.TryGetDecimal(values.FirstOrDefault());
                                    if (attributeValue.HasValue && intValue.HasValue)
                                    {
                                        attributePassed = attributeValue > intValue;
                                    }
                                    else
                                    {
                                        attributePassed = false;
                                    }
                                    break;
                                case "LessThen":
                                    //NOTE: Should be first, in case of value: 100 m (10 ATM)
                                    attributeValue = StringHelper.TryGetDecimal(attribute.Value);
                                    if (attributeValue == null)
                                        attributeValue = StringHelper.GetFirstDigitSequences(attribute.Value);
                                    intValue = StringHelper.TryGetDecimal(values.FirstOrDefault());
                                    if (attributeValue.HasValue && intValue.HasValue)
                                    {
                                        attributePassed = attributeValue < intValue;
                                    }
                                    else
                                    {
                                        attributePassed = false;
                                    }
                                    break;
                            }
                        }
                        passed = passed && attributePassed;
                    }
                }
                if (customCategory.Mode == (int)CustomCategoryModes.List)
                {
                    var now = when;
                    var hasMapping = customCategoryToStyles
                        .Any(f => f.CustomCategoryId == customCategory.Id
                            && f.StyleId == styleId
                            && (!f.StartDate.HasValue || f.StartDate <= now)
                            && (!f.EndDate.HasValue || now <= f.EndDate));
                    passed = passed && hasMapping;
                }

                if (passed)
                {
                    if (results.All(r => r.CategoryId != magentoCategory.CategoryId))
                    {
                        results.Add(new CategoryDTO()
                        {
                            CategoryId = magentoCategory.CategoryId
                        });
                    }
                }
            }


            return results;
        }

        //public static IList<CategoryDTO> BuildFeaturesCategories(IList<StyleFeatureValueDTO> styleFeatures,
        //    IList<CategoryDTO> magentoCategories,
        //    IList<CategoryMappingInfo> categoryMaps)
        //{
        //    var results = new List<CategoryDTO>();

        //    foreach (var category in categoryMaps)
        //    {
        //        var feature = styleFeatures.FirstOrDefault(f => f.FeatureName == category.SystemName);
        //        if (feature != null)
        //        {
        //            var postfixes = category.ValuePostfix ?? new string[] { "" };

        //            foreach (var postfix in postfixes)
        //            {
        //                var categoryName = StringHelper.JoinTwo(" ", feature.Value, postfix);
        //                //categoryName = PrepareCategoryName(categoryName);

        //                //tough compare
        //                var magentoCategory = magentoCategories.FirstOrDefault(
        //                    c => String.Compare(c.Name, categoryName, StringComparison.InvariantCultureIgnoreCase) == 0
        //                         && c.ParentCategoryId == MagentoCategoryHelper.RootParentId);

        //                //rough caompare
        //                if (magentoCategory == null)
        //                    magentoCategory = magentoCategories.FirstOrDefault(
        //                        c => MagentoCategoryHelper.CompareCategoryNames(c.Name, categoryName)
        //                             && c.ParentCategoryId == MagentoCategoryHelper.RootParentId);
        //                if (magentoCategory != null)
        //                {
        //                    results.Add(new CategoryDTO()
        //                    {
        //                        CategoryId = magentoCategory.CategoryId
        //                    });
        //                }
        //            }
        //        }
        //    }

        //    var productTypeFeature = styleFeatures.FirstOrDefault(f => f.FeatureName == StyleFeatureHelper.ProductType);
        //    if (productTypeFeature != null)
        //    {
        //        var sunglassesMagentoCategory = magentoCategories.FirstOrDefault(
        //                    c => String.Compare(c.Name, "sunglasses", StringComparison.InvariantCultureIgnoreCase) == 0
        //                         && c.ParentCategoryId == MagentoCategoryHelper.RootParentId);
        //        if (sunglassesMagentoCategory != null
        //            && StringHelper.IsEqualNoCase(productTypeFeature.Value, "Sunglasses"))
        //        {
        //            results.Add(new CategoryDTO()
        //            {
        //                CategoryId = sunglassesMagentoCategory.CategoryId
        //            });
        //        }
        //    }

        //    var genderFeature = styleFeatures.FirstOrDefault(f => f.FeatureName == "Gender");

        //    if (genderFeature != null)
        //    {
        //        if (genderFeature.Value == "Men's")
        //        {
        //            results.Add(new CategoryDTO()
        //            {
        //                CategoryId = 4
        //            });
        //            results.Add(new CategoryDTO()
        //            {
        //                CategoryId = 421
        //            });
        //        }
        //        if (genderFeature.Value == "Women's")
        //        {
        //            results.Add(new CategoryDTO()
        //            {
        //                CategoryId = 5
        //            });
        //            results.Add(new CategoryDTO()
        //            {
        //                CategoryId = 422
        //            });
        //        }
        //    }

        //    return results;
        //}

        public static IList<StyleFeatureValueDTO> BuildFeaturesAttributes(IList<StyleFeatureValueDTO> styleFeatures,
            IList<FeatureDTO> magentoAttributes)
        {
            var results = new List<StyleFeatureValueDTO>();

            if (!styleFeatures.Any(f => f.FeatureName == "Gender"))
            {
                styleFeatures.Add(new StyleFeatureValueDTO()
                {
                    FeatureName = "Gender",
                    ObjValue = "Unisex"
                });
            }

            foreach (var sourceFeature in styleFeatures)
            {
                var attribute = new StyleFeatureValueDTO()
                {
                    Value = sourceFeature.Value ?? "",
                    FeatureName = sourceFeature.FeatureName,
                };

                //if (attribute.FeatureName == "Gender")
                //{
                //    attribute.FeatureName = MagentoFeatures.GenderFeatureName;
                //    if (attribute.Value == "Men's")
                //        attribute.ObjValue = "4";
                //    else if (attribute.Value == "Women's")
                //        attribute.ObjValue = "5";
                //    else //if (feature.Value == "Unisex")
                //        attribute.ObjValue = "114";

                //    results.Add(attribute);
                //}

                var optionAttribute = BuildWithMagentoOptionValue(magentoAttributes, attribute.FeatureName, attribute.Value);
                if (optionAttribute != null)
                {
                    attribute = optionAttribute;
                    if (attribute != null && !String.IsNullOrEmpty(attribute.ObjValue.ToString()))
                        results.Add(attribute);
                }

                var attributeName = _textFeatureMappings.ContainsKey(attribute.FeatureName) ? _textFeatureMappings[attribute.FeatureName] : null;
                if (!String.IsNullOrEmpty(attributeName))
                {
                    attribute.FeatureName = attributeName;
                }

                if (attribute != null && !String.IsNullOrEmpty(attribute.ObjValue?.ToString()))
                    results.Add(attribute);
            }

            return results;
        }

        private static StyleFeatureValueDTO BuildWithMagentoOptionValue(IList<FeatureDTO> magentoAttributes,
            string featureName, 
            string featureValue)
        {
            StyleFeatureValueDTO result = null;
            var listFeatureName = _valueFeatureMappings.FirstOrDefault(c => c.SystemName == featureName)?.MagentoName;

            if (!String.IsNullOrEmpty(listFeatureName))
            {
                result = new StyleFeatureValueDTO()
                {
                    FeatureName = listFeatureName
                };
                var magentoFeature =
                    magentoAttributes.FirstOrDefault(a => a.Name == listFeatureName);
                var optionValue = magentoFeature.FeatureValues.FirstOrDefault(
                        fv => String.Compare(fv.Value, featureValue, StringComparison.OrdinalIgnoreCase) == 0);
                result.ObjValue = optionValue != null ? optionValue.ExtendedValue : "";
            }

            return result;
        }

        public IList<StyleFeatureValueDTO> BuildBaseAttributes(Item item,
            Listing listing,
            ViewListingSaleDTO listingSale,
            StyleEntireDto style,
            IList<FeatureDTO> magentoAttributes)
        {
            var results = new List<StyleFeatureValueDTO>();

            var minPrice = listing.CurrentPrice;
            if (listingSale != null && listingSale.SalePrice.HasValue)
                minPrice = listingSale.SalePrice.Value;

            if (style.MSRP.HasValue && style.MSRP.Value > 0)
            {
                results.Add(new StyleFeatureValueDTO()
                {
                    FeatureName = PAMagentoFeatures.ListPriceAttributeName,
                    ObjValue = style.MSRP.ToString(),
                });

                results.Add(new StyleFeatureValueDTO()
                {
                    FeatureName = PAMagentoFeatures.BestDialAttributeName,
                    ObjValue = (int)Math.Round((style.MSRP.Value - minPrice) / style.MSRP.Value * 100M)
                });
            }

            var sizeAttribute = BuildWithMagentoOptionValue(magentoAttributes, "Size", !String.IsNullOrEmpty(item.Size) ? item.Size : null);
            if (sizeAttribute != null)
                results.Add(sizeAttribute);

            var colorAttribute = BuildWithMagentoOptionValue(magentoAttributes, "Color", !String.IsNullOrEmpty(item.Color) ? item.Color : null);
            if (colorAttribute != null)
                results.Add(colorAttribute);

            //results.Add(new StyleFeatureValueDTO()
            //{
            //    FeatureName = MagentoFeatures.SizeAttributeName,
            //    ObjValue = !String.IsNullOrEmpty(item.Size) ? item.Size : null,
            //});

            //results.Add(new StyleFeatureValueDTO()
            //{
            //    FeatureName = MagentoFeatures.ColorAttributeName,
            //    ObjValue = !String.IsNullOrEmpty(item.Color) ? item.Color : null,
            //});

            results.Add(new StyleFeatureValueDTO()
            {
                FeatureName = PAMagentoFeatures.MfnFeatureName,
                ObjValue = PrepareMfn(style.StyleID, style.Manufacturer)
            });

            results.Add(new StyleFeatureValueDTO()
            {
                FeatureName = PAMagentoFeatures.UpcAttributeName,
                ObjValue = item.Barcode ?? ""
            });

            if (style.CreateDate.HasValue)
            {
                results.Add(new StyleFeatureValueDTO()
                {
                    FeatureName = PAMagentoFeatures.NewArrivalAttributeName,
                    ObjValue = style.CreateDate.Value.ToString("yyyyMMdd")
                });
            }

            return results;
        }

        public static IList<StyleFeatureValueDTO> BuildMetaAttributes(ILogService log,
            Item item,
            StyleEntireDto style)
        {
            var results = new List<StyleFeatureValueDTO>();

            results.Add(new StyleFeatureValueDTO()
            {
                FeatureName = PAMagentoFeatures.StoreIdAttributeName,
                ObjValue = "1",
            });
            //results.Add(new StyleFeatureValueDTO()
            //{
            //    FeatureName = MagentoFeatures.WebSiteIdAttributeName,
            //    ObjValue = "0",
            //});
            results.Add(new StyleFeatureValueDTO()
            {
                FeatureName = PAMagentoFeatures.TaxClassIdAttributeName,
                ObjValue = "2",
            });

            //NOTE: already exists product.Visibility
            //styleFeatures.Add(new StyleFeatureValueDTO()
            //{
            // //   {
            // //      "label": "Not Visible Individually",
            // //      "value": "1"
            // //   },
            // //               {
            // //      "label": "Catalog",
            // //      "value": "2"
            // //   },
            // //               {
            // //      "label": "Search",
            // //      "value": "3"
            // //   },
            // //               {
            // //      "label": "Catalog, Search",
            // //      "value": "4"
            // //   }
            // //]
            //    FeatureName = MagentoFeatures.VisibilityAttributeName,
            //    Value = "4",
            //});
            results.Add(new StyleFeatureValueDTO()
            {
                FeatureName = PAMagentoFeatures.GiftMessageAvailableAttributeName,
                ObjValue = "2",
                //[{
                //    "label": "Yes",
                //    "value": "1"
                //},
                //{
                //    "label": "No",
                //    "value": "0"
                //},
                //{
                //    "label": "Use config",
                //    "value": "2"
                //}}
            });

            if (!String.IsNullOrEmpty(style.Description))
            {
                var desc = DescriptionHelper.PrepareForPublishDescription(style.Description);
                log.Info("Description: " + desc);

                results.Add(new StyleFeatureValueDTO()
                {
                    FeatureName = PAMagentoFeatures.DescriptionAttributeName,
                    ObjValue = desc,
                });
                results.Add(new StyleFeatureValueDTO()
                {
                    FeatureName = PAMagentoFeatures.MetaDescriptionAttributeName,
                    ObjValue = desc,
                });
            }

            results.Add(new StyleFeatureValueDTO()
            {
                FeatureName = PAMagentoFeatures.BulletPoint1AttributeName,
                ObjValue = style.BulletPoint1
            });
            results.Add(new StyleFeatureValueDTO()
            {
                FeatureName = PAMagentoFeatures.BulletPoint2AttributeName,
                ObjValue = style.BulletPoint2
            });
            results.Add(new StyleFeatureValueDTO()
            {
                FeatureName = PAMagentoFeatures.BulletPoint3AttributeName,
                ObjValue = style.BulletPoint3
            });
            results.Add(new StyleFeatureValueDTO()
            {
                FeatureName = PAMagentoFeatures.BulletPoint4AttributeName,
                ObjValue = style.BulletPoint4
            });
            results.Add(new StyleFeatureValueDTO()
            {
                FeatureName = PAMagentoFeatures.BulletPoint5AttributeName,
                ObjValue = style.BulletPoint5
            });


            results.Add(new StyleFeatureValueDTO()
            {
                FeatureName = PAMagentoFeatures.MetaTitleAttributeName,
                ObjValue = style.Name
            });

            return results;
        }

        private ImageDTO BuildImage(string imageUrl)
        {
            if (!String.IsNullOrEmpty(imageUrl))
            {
                try
                {
                    var filename = Path.GetFileName(new Uri(imageUrl).AbsolutePath);
                    var imageStream = ImageHelper.DownloadRemoteImageFileAsStream(imageUrl);
                    //imageStream.Seek(0, SeekOrigin.Begin);
                    var base64image = StringHelper.ToBase64(imageStream.ToArray());

                    return new ImageDTO()
                    {
                        ImageData = base64image,
                        MimeType = FileHelper.GetMimeTypeByExt(Path.GetExtension(filename)),
                        FileName = FileHelper.PrepareFileName(filename),
                    };
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message, ex);
                }
            }
            return null;
        }

        private static string PrepareMfn(string styleString, string manufacture)
        {
            if (String.IsNullOrEmpty(styleString))
                return styleString;

            if (!String.IsNullOrEmpty(manufacture))
            {
                var index = styleString.IndexOf(manufacture);
                if (index >= 0)
                {
                    styleString = styleString.Replace(manufacture, "").Trim(" /-_".ToCharArray());
                    return styleString;
                }
            }
            var parts = styleString.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (parts.Count() > 1)
            {
                return String.Join(" ", parts.Skip(1));
            }
            return styleString;
        }

        public void ReadItemsFast(DateTime? lastSync)
        {
            var asinWithErrors = new List<string>();
            var items = _api.GetItems(_log, _time, null, ItemFillMode.Defualt, out asinWithErrors);

            var missingItems = new List<ParentItemDTO>();

            _log.Info("Items count: " + items.Count());

            var index = 0;
            var pageSize = 100;
            while (index < items.Count())
            {
                _log.Info("Index: " + index);
                var pageItems = items.Skip(index).Take(pageSize).ToList();
                var skuList = pageItems.Select(i => i.SKU).ToList();
                using (var db = _dbFactory.GetRWDb())
                {
                    var dtoListings = db.Listings.GetAll().Where(l => skuList.Contains(l.SKU)
                            && l.Market == (int)MarketType.Magento)
                        .Select(i => new ItemDTO()
                        {
                            Id = (int)i.Id,
                            SKU = i.SKU,
                            AmazonRealQuantity = i.AmazonRealQuantity,
                            AmazonCurrentPrice = i.AmazonCurrentPrice,
                        })
                        .OrderBy(i => i.Id)
                        .ToList();

                    foreach (var item in pageItems)
                    {
                        var existDtoListing = dtoListings.FirstOrDefault(i => i.SKU == item.SKU);
                        if (existDtoListing == null)
                        {
                            _log.Info("No exists dbListing, SKU=" + item.SKU);
                            missingItems.Add(item);
                        }
                    }
                }
                index += pageSize;
            }

            _log.Info("Missing items: ");
            foreach (var item in missingItems)
            {
                _log.Info("SKU: " + item.SKU + " - market qty: " + item.Variations.FirstOrDefault()?.AmazonRealQuantity);
            }
        }

        public void ReadItems(DateTime? lastSync)
        {
            var asinWithErrors = new List<string>();
            var items = _api.GetItems(_log, _time, null, ItemFillMode.Defualt, out asinWithErrors);

            _log.Info("Items count: " + items.Count());

            var missingItems = new List<ParentItemDTO>();

            var index = 0;
            var pageSize = 100;
            while (index < items.Count())
            {
                _log.Info("Index: " + index);
                var pageItems = items.Skip(index).Take(pageSize).ToList();
                var skuList = pageItems.Select(i => i.SKU).ToList();
                using (var db = _dbFactory.GetRWDb())
                {
                    var dtoListings = db.Listings.GetAll().Where(l => skuList.Contains(l.SKU)
                            && l.Market == (int)MarketType.Magento)
                        .Select(i => new ItemDTO()
                        {
                            Id = (int)i.Id,
                            SKU = i.SKU,
                            AmazonRealQuantity = i.AmazonRealQuantity,
                            AmazonCurrentPrice = i.AmazonCurrentPrice,
                        })
                        .OrderBy(i => i.Id)
                        .ToList();

                    foreach (var item in pageItems)
                    {
                        var itemInfo = _api.GetStockItem(item.SKU);
                        if (itemInfo != null)
                        {
                            var qtyChanged = false;
                            var priceChanged = false;
                            var existDtoListing = dtoListings.FirstOrDefault(i => i.SKU == item.SKU);
                            if (existDtoListing != null)
                            {
                                if (existDtoListing.AmazonRealQuantity != itemInfo.RealQuantity)
                                {
                                    existDtoListing.AmazonRealQuantity = itemInfo.RealQuantity;
                                    qtyChanged = true;
                                }
                                if (existDtoListing.AmazonCurrentPrice != item.Price)
                                {
                                    existDtoListing.AmazonCurrentPrice = item.Price;
                                    priceChanged = true;
                                }

                                var modifiedFieldList = new List<Expression<Func<Listing, object>>>();
                                //if (qtyChanged) NOTE: always update UpdateDate, to know which SKU actually published
                                {
                                    modifiedFieldList.Add(l => l.AmazonRealQuantity);
                                    modifiedFieldList.Add(l => l.AmazonRealQuantityUpdateDate);
                                }
                                //if (priceChanged) NOTE: always update UpdateDate, to know which SKU actually published
                                {
                                    modifiedFieldList.Add(l => l.AmazonCurrentPrice);
                                    modifiedFieldList.Add(l => l.AmazonCurrentPriceUpdateDate);
                                }

                                if (modifiedFieldList.Any())
                                {
                                    db.Listings.TrackItem(new Listing()
                                    {
                                        Id = existDtoListing.Id,
                                        AmazonCurrentPrice = existDtoListing.AmazonCurrentPrice,
                                        AmazonCurrentPriceUpdateDate = _time.GetAppNowTime(),
                                        AmazonRealQuantity = existDtoListing.AmazonRealQuantity,
                                        AmazonRealQuantityUpdateDate = _time.GetAppNowTime()
                                    },
                                    modifiedFieldList);
                                }
                            }
                            else
                            {
                                _log.Info("No exists dbListing, SKU=" + item.SKU + ", qty: " + itemInfo.RealQuantity);
                                missingItems.Add(item);
                            }
                        }
                    }
                    db.Commit();
                }
                index += pageSize;
            }

            _log.Info("Missing items: ");
            foreach (var item in missingItems)
            {
                _log.Info("SKU: " + item.SKU + " - market qty: " + item.Variations.FirstOrDefault()?.AmazonRealQuantity);
            }
        }

        public void SendInventoryUpdates()
        {
            _log.Info("Begin SendInventoryUpdates");

            var pageSize = 200;
            var index = 0;
            IList<long> allListingIds = null;
            var listingWithErrorList = new List<Listing>();

            using (var db = _dbFactory.GetRWDb())
            {
                var listings = db.Listings.GetQuantityUpdateRequiredList(_api.Market, _api.MarketplaceId);
                allListingIds = listings.Select(l => l.Id).ToList();
            }

            _api.Connect();
            try
            {
                while (index < allListingIds.Count)
                {
                    _log.Info("Index: " + index);
                    var pageListingIds = allListingIds.Skip(index).Take(pageSize).Select(l => l).ToList();
                    using (var db = _dbFactory.GetRWDb())
                    {
                        var dbPageListings = db.Listings.GetAll().Where(l => pageListingIds.Contains(l.Id)).ToList();

                        foreach (var listing in dbPageListings)
                        {
                            var result = _api.SetItemQuantity(listing.SKU, listing.RealQuantity);
                            if (result.IsSuccess)
                            {
                                listing.QuantityUpdateRequested = false;
                                listing.AmazonRealQuantity = listing.RealQuantity;
                                _log.Info("Qty updated, sku=" + listing.SKU + ", sendQty=" +
                                          listing.RealQuantity);
                            }
                            else
                            {
                                listingWithErrorList.Add(listing);
                                _log.Info("Can't update qty, sku=" + listing.SKU + ", status: " + result.StatusCode + ", details: " + result.Message.ToString());
                            }
                        }
                        db.Commit();
                    }

                    index += pageSize;
                }
            }
            finally
            {
                _api.Disconnect();
            }

            _log.Info("End SendInventoryUpdates");
        }

        public void SendPriceUpdates()
        {
            SendPriceUpdates(null);
        }

        public void SendPriceUpdates(IList<string> skuList)
        {
            _log.Info("Begin SendPriceUpdates");
            var today = _time.GetAppNowTime().Date;

            var items = new List<ItemDTO>();
            var listingWithErrorList = new List<ItemDTO>();

            using (var db = _dbFactory.GetRWDb())
            {
                if (skuList == null)
                {
                    var itemQuery = from l in db.Listings.GetAll()
                                    join i in db.Items.GetAll() on l.ItemId equals i.Id
                                    join s in db.Styles.GetAll() on i.StyleId equals s.Id
                                    join sale in db.StyleItemSaleToListings.GetAllListingSaleAsDTO() on l.Id equals sale.ListingId
                                        into withSale
                                    from sale in withSale.DefaultIfEmpty()
                                    where l.PriceUpdateRequested &&
                                          (i.ItemPublishedStatus == (int)PublishedStatuses.Published
                                           || i.ItemPublishedStatus == (int)PublishedStatuses.HasChanges
                                           || i.ItemPublishedStatus == (int)PublishedStatuses.ChangesSubmited)
                                          && i.Market == (int)_api.Market
                                          && l.CurrentPrice > 0.01M
                                          && !l.IsRemoved
                                    select new ItemDTO()
                                    {
                                        ListingEntityId = l.Id,
                                        SourceMarketId = i.SourceMarketId,
                                        SKU = l.SKU,
                                        StyleId = i.StyleId,
                                        CurrentPrice = l.CurrentPrice,
                                        ListPrice = s.MSRP,
                                        SalePrice = sale != null ? sale.SalePrice : null,
                                        SaleStartDate = sale != null ? sale.SaleStartDate : null,
                                        SaleEndDate = sale != null ? sale.SaleEndDate : null,
                                    };

                    items = itemQuery.ToList();
                }
                else
                {
                    var itemQuery = from l in db.Listings.GetAll()
                                    join i in db.Items.GetAll() on l.ItemId equals i.Id
                                    join s in db.Styles.GetAll() on i.StyleId equals s.Id
                                    join sale in db.StyleItemSaleToListings.GetAllListingSaleAsDTO() on l.Id equals sale.ListingId
                                        into withSale
                                    from sale in withSale.DefaultIfEmpty()
                                    where skuList.Contains(l.SKU)
                                          && i.Market == (int)_api.Market
                                          && l.CurrentPrice > 0.01M
                                          && !l.IsRemoved
                                    select new ItemDTO()
                                    {
                                        ListingEntityId = l.Id,
                                        SourceMarketId = i.SourceMarketId,
                                        SKU = l.SKU,
                                        StyleId = i.StyleId,
                                        CurrentPrice = l.CurrentPrice,
                                        ListPrice = s.MSRP,
                                        SalePrice = sale != null ? sale.SalePrice : null,
                                        SaleStartDate = sale != null ? sale.SaleStartDate : null,
                                        SaleEndDate = sale != null ? sale.SaleEndDate : null,
                                    };

                    items = itemQuery.ToList();
                }
            }

            _api.Connect();
            try
            {
                var pageSize = 200;
                var index = 0;
                while (index < items.Count)
                {
                    using (var db = _dbFactory.GetRWDb())
                    {
                        var pageItems = items.Skip(index).Take(pageSize).ToList();
                        var listingIds = pageItems.Select(i => i.ListingEntityId).ToList();
                        var dbPageListings = db.Listings.GetAll().Where(l => listingIds.Contains(l.Id)).ToList();

                        foreach (var item in pageItems)
                        {
                            if (item.CurrentPrice == 0)
                            {
                                _log.Info("Trying to send $0 price, skipped. SKU=" + item.SKU);
                                continue;
                            }

                            //var item = items.FirstOrDefault(i => i.Id == listing.ItemId);
                            if (!String.IsNullOrEmpty(item.SourceMarketId))
                            {
                                _log.Info("Sent updates for: " + item.SKU
                                    + ", price: " + item.CurrentPrice
                                    + ", msrp: " + item.ListPrice
                                    + ", sale price: " + item.SalePrice);

                                var result = _api.SetItemPrice(item.SKU,
                                    item.CurrentPrice,
                                    item.ListPrice,
                                    item.SalePrice,
                                    item.SaleStartDate,
                                    item.SaleEndDate);

                                if (result.Status == CallStatus.Success)
                                {
                                    var dbListing = dbPageListings.FirstOrDefault(i => i.Id == item.ListingEntityId);
                                    if (dbListing != null)
                                    {
                                        dbListing.PriceUpdateRequested = false;
                                        dbListing.AmazonCurrentPrice = item.CurrentPrice;
                                        db.Commit();
                                        _log.Info("Price updated, listingId=" + dbListing.ListingId + ", newPrice=" +
                                                  dbListing.CurrentPrice);
                                    }
                                    else
                                    {
                                        _log.Info("DbListing is empty");
                                    }
                                }
                                else
                                {
                                    listingWithErrorList.Add(item);
                                    _log.Info("Can't update prices, result status: " + result.Status + ", details: " + result.Message);
                                }
                            }
                            else
                            {
                                if (String.IsNullOrEmpty(item.SourceMarketId))
                                    _log.Warn("Item hasn't sourceMarketId, itemId=" + item.Id);
                            }
                        }

                        index += pageSize;
                    }
                }
            }
            finally
            {
                _api.Disconnect();
            }

            _log.Info("End SendPriceUpdates");
        }
    }
}





//public class MagentoItemsSync : IItemsSync
//    {
//        private ILogService _log;
//        private ITime _time;
//        private MagentoMarketApi _api;
//        private IDbFactory _dbFactory;

//        public MagentoItemsSync(MagentoMarketApi api,
//            IDbFactory dbFactory,
//            ILogService log,
//            ITime time)
//        {
//            _log = log;
//            _time = time;
//            _api = api;
//            _dbFactory = dbFactory;
//        }

//        public void SendItemUpdates()
//        {
//            _log.Info("Begin SyncItems");
            
//            using (var db = _dbFactory.GetRWDb())
//            {
//                var allParentItems = db.ParentItems.GetAllAsDto().Where(pi => pi.Market == (int) MarketType.Magento).ToList();
//                var allItems = db.Items.GetAllActualExAsDto().Where(pi => pi.Market == (int) MarketType.Magento).ToList();

//                if (allParentItems.Any())
//                {
//                    _api.Connect();
//                    foreach (var parentItem in allParentItems)
//                    {
//                        try
//                        {
//                            var items = allItems.Where(i => i.ParentASIN == parentItem.ASIN).ToList();

//                            _log.Info("Begin send product, asin=" + parentItem.ASIN + ", items=" + items.Count);

//                            var result = _api.CreateOrUpdateProduct(parentItem, items);
                            
//                            //Keep productId, productUrl
//                            if (result.Data != null)
//                            {
//                                foreach (var item in items)
//                                {
//                                    var itemResult = result.Data.FirstOrDefault(i => i.SKU == item.SKU);

//                                    if (itemResult != null)
//                                    {
//                                        if (item.SourceMarketId != itemResult.ProductId
//                                            || item.SourceMarketUrl != itemResult.ProductUrl)
//                                        {
//                                            var dbItem = db.Items.Get(item.Id);
//                                            dbItem.SourceMarketId = itemResult.ProductId;
//                                            dbItem.SourceMarketUrl = itemResult.ProductUrl;

//                                            db.Commit();
//                                            _log.Info("Upated item fields=" + item.SKU);
//                                        }
//                                        else
//                                        {
//                                            _log.Info("No changes for item fields=" + item.SKU);
//                                        }
//                                    }
//                                    else
//                                    {
//                                        _log.Info("Doesn't create or update Item from Magento=" + item.SKU);
//                                    }
//                                }

//                                var parentItemResult = result.Data.FirstOrDefault(i => i.SKU == parentItem.SKU);
//                                if (parentItemResult != null)
//                                {
//                                    if (parentItem.SourceMarketId != parentItemResult.ProductId
//                                        || parentItem.SourceMarketUrl != parentItemResult.ProductUrl)
//                                    {
//                                        var dbParentItem = db.ParentItems.Get(parentItem.Id);
//                                        dbParentItem.SourceMarketId = parentItemResult.ProductId;
//                                        dbParentItem.SourceMarketUrl = parentItemResult.ProductUrl;

//                                        _log.Info("Upated Parent item fields=" + parentItem.SKU);
//                                    }
//                                    else
//                                    {
//                                        _log.Info("No changes for Parent item fields=" + parentItem.SKU);
//                                    }
//                                }
//                                else
//                                {
//                                    _log.Info("Doesn't create or update Parent Item from Magento=" + parentItem.SKU);
//                                }

//                                db.Commit();
//                            }

//                            _log.Info("End send product");
//                        }
//                        catch (Exception ex)
//                        {
//                            _log.Info("Error send product", ex);
//                        }
//                    }
//                    _api.Disconnect();
//                }
//            }

//            _log.Info("End SyncItems");
//        }

//        public void ReadItems(DateTime? lastSync)
//        {
//            //TODO: get market price/qty
//        }
        
//        public void SendInventoryUpdates()
//        {
//            _log.Info("Begin SendInventoryUpdates");
//            using (var db = _dbFactory.GetRWDb())
//            {
//                var listings = db.Listings.GetQuantityUpdateRequiredList(_api.Market, _api.MarketplaceId);
//                var itemIdList = listings.Select(l => l.ItemId).ToList();
//                var items = db.Items.GetAllViewAsDto().Where(i => itemIdList.Contains(i.Id)).ToList();

//                var listingWithErrorList = new List<Listing>();

//                _api.Connect();
//                try
//                {
//                    foreach (var listing in listings)
//                    {
//                        var item = items.FirstOrDefault(i => i.Id == listing.ItemId);
//                        if (item != null && !String.IsNullOrEmpty(item.SourceMarketId))
//                        {
//                            var result = _api.SetItemQuantity(item.SourceMarketId,
//                                listing.RealQuantity);

//                            if (result.Status == CallStatus.Success)
//                            {
//                                listing.QuantityUpdateRequested = false;
//                                listing.AmazonRealQuantity = listing.RealQuantity;
//                                db.Commit();
//                                _log.Info("Qty updated, listingId=" + listing.ListingId + ", sendQty=" +
//                                          listing.RealQuantity);
//                            }
//                            else
//                            {
//                                listingWithErrorList.Add(listing);
//                                _log.Info("Can't update qty, result=" + result.ToString());
//                            }
//                        }
//                        else
//                        {
//                            if (item == null)
//                                _log.Warn("Can't find item, for listing=" + listing.ListingId);
//                            if (String.IsNullOrEmpty(item.SourceMarketId))
//                                _log.Warn("Item hasn't sourceMarketId, itemId=" + item.Id);
//                        }
//                    }
//                }
//                finally
//                {
//                    _api.Disconnect();
//                }
//            }
//            _log.Info("End SendInventoryUpdates");
//        }

//        public void SendPriceUpdates()
//        {
//            _log.Info("Begin SendPriceUpdates");
//            var today = _time.GetAppNowTime().Date;

//            using (var db = _dbFactory.GetRWDb())
//            {
//                var itemQuery = from l in db.Listings.GetAll()
//                                   join i in db.Items.GetAll() on l.ItemId equals i.Id
//                                   join s in db.Styles.GetAll() on i.StyleId equals s.Id
//                                   join sale in db.StyleItemSaleToListings.GetAllListingSaleAsDTO() on l.Id equals sale.ListingId into withSale
//                                   from sale in withSale.DefaultIfEmpty()
//                                   where l.PriceUpdateRequested
//                                       && i.ItemPublishedStatus == (int)PublishedStatuses.Published
//                                       && i.Market == (int)_api.Market
//                                       && !l.IsRemoved
//                                   select new ItemDTO()
//                                   {
//                                       ListingEntityId = l.Id,
//                                       SourceMarketId = i.SourceMarketId,
//                                       SKU = l.SKU,
//                                       StyleId = i.StyleId,
//                                       CurrentPrice = l.CurrentPrice,
//                                       ListPrice = s.MSRP,
//                                       SalePrice = sale != null ? sale.SalePrice : null,
//                                       SaleStartDate = sale != null ? sale.SaleStartDate : null,
//                                       SaleEndDate = sale != null ? sale.SaleEndDate : null,
//                                   };

//                var listingWithErrorList = new List<ItemDTO>();

//                var items = itemQuery.ToList();

//                _api.Connect();
//                try
//                {
//                    foreach (var item in items)
//                    {
//                        //var item = items.FirstOrDefault(i => i.Id == listing.ItemId);
//                        if (!String.IsNullOrEmpty(item.SourceMarketId))
//                        {
//                            var result = _api.SetItemPrice(item.SourceMarketId,
//                                item.CurrentPrice,
//                                item.SalePrice,
//                                item.SaleStartDate,
//                                item.SaleEndDate);

//                            if (result.Status == CallStatus.Success)
//                            {
//                                var dbListing = db.Listings.GetAll().FirstOrDefault(i => i.Id == item.ListingEntityId);
//                                dbListing.QuantityUpdateRequested = false;
//                                dbListing.AmazonCurrentPrice = item.CurrentPrice;
//                                db.Commit();
//                                _log.Info("Price updated, listingId=" + dbListing.ListingId + ", newPrice=" +
//                                          dbListing.CurrentPrice);
//                            }
//                            else
//                            {
//                                listingWithErrorList.Add(item);
//                                _log.Info("Can't update qty, result=" + result.ToString());
//                            }
//                        }
//                        else
//                        {
//                            if (String.IsNullOrEmpty(item.SourceMarketId))
//                                _log.Warn("Item hasn't sourceMarketId, itemId=" + item.Id);
//                        }
//                    }
//                }
//                finally
//                {
//                    _api.Disconnect();
//                }
//            }
//            _log.Info("End SendPriceUpdates");
//        }
//    }
//}
