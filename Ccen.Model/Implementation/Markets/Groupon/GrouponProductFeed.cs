using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Groupons;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.Implementation.Markets.Groupon
{
    public class GrouponProductFeed
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private IWeightService _weightService;
        private string _grouponImageBaseUrl;
        private string _grouponImageDirectory;

        public GrouponProductFeed(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            IWeightService weightService,
            string grouponImageBaseUrl,
            string grouponImageDirectory)
        {
            _time = time;
            _log = log;
            _dbFactory = dbFactory;
            _weightService = weightService;
            _grouponImageBaseUrl = grouponImageBaseUrl;
            _grouponImageDirectory = grouponImageDirectory;
        }

        public MemoryStream Export(IList<string> skuList,
            MarketType market,
            string marketplaceId)
        {
            IList<GrouponGoodLine> results = new List<GrouponGoodLine>();

            using (var db = _dbFactory.GetRWDb())
            {
                var parentASINs = db.Items.GetAllViewActual()
                    .Where(i => i.Market == (int)market
                        && (i.MarketplaceId == marketplaceId
                            || String.IsNullOrEmpty(marketplaceId))
                        && skuList.Contains(i.SKU))
                    .Select(i => i.ParentASIN)
                    .ToList();

                var itemDtoList = db.Items.GetAllActualExAsDto()
                    .Where(i => i.Market == (int)market
                        && (i.MarketplaceId == marketplaceId
                            || String.IsNullOrEmpty(marketplaceId))
                        && parentASINs.Contains(i.ParentASIN))
                    .ToList();

                var styleIdList = itemDtoList.Where(i => i.StyleId.HasValue).Select(i => i.StyleId.Value).ToList();
                var styleItemIdList = itemDtoList.Where(i => i.StyleItemId.HasValue).Select(i => i.StyleItemId.Value).ToList();

                var itemLinkDtoList = db.Items.GetAllActualExAsDto()
                    .Where(i => i.Market == (int)MarketType.Shopify
                        && i.MarketplaceId == MarketplaceKeeper.ShopifyEveryCh
                        && i.StyleId.HasValue
                        && styleIdList.Contains(i.StyleId.Value))
                    .ToList();

                var parentItemDtoList = db.ParentItems.GetAllAsDto()
                    .Where(i => i.Market == (int)market
                        && (i.MarketplaceId == marketplaceId
                            || String.IsNullOrEmpty(marketplaceId))
                        && parentASINs.Contains(i.ASIN))
                    .ToList();
                                
                var styleList = db.Styles.GetAllAsDtoEx().Where(s => styleIdList.Contains(s.Id)).ToList();
                var styleItemList = db.StyleItems.GetAllAsDto().Where(si => styleItemIdList.Contains(si.StyleItemId)).ToList();
                var allStyleImageList = db.StyleImages.GetAllAsDto().Where(sim => styleIdList.Contains(sim.StyleId)).ToList();
                var allFeatures = db.FeatureValues.GetValuesByStyleIds(styleIdList) as List<FeatureValueDTO>;
                allFeatures.AddRange(db.StyleFeatureTextValues.GetAllWithFeature().Where(ft => styleIdList.Contains(ft.StyleId))
                    .Select(f => new FeatureValueDTO()
                    {
                        Id = f.Id,
                        FeatureId = f.FeatureId,
                        FeatureName = f.FeatureName,
                        StyleId = f.StyleId,
                        Value = f.Value
                    }).ToList());

                if (!String.IsNullOrEmpty(_grouponImageDirectory))
                {
                    foreach (var styleImage in allStyleImageList)
                    {
                        try
                        {
                            var styleString = styleList.FirstOrDefault(s => s.Id == styleImage.StyleId)?.StyleID;

                            var filepath = ImageHelper.BuildGrouponImage(_grouponImageDirectory,
                                styleImage.Image,
                                styleString + "_" + MD5Utils.GetMD5HashAsString(styleImage.Image));
                            var filename = Path.GetFileName(filepath);
                            styleImage.Image = UrlUtils.CombinePath(_grouponImageBaseUrl, filename);
                        }
                        catch (Exception ex)
                        {
                            _log.Info("BuildGrouponImage error, image=" + styleImage.Image, ex);
                        }
                    }
                }

                foreach (var item in itemDtoList)
                {
                    var itemLink = itemLinkDtoList.FirstOrDefault(i => i.StyleItemId == item.StyleItemId);
                    if (itemLink != null)
                    {
                        item.SourceMarketUrl = itemLink.SourceMarketUrl;
                    }
                    else
                    {
                        item.SourceMarketUrl = null;
                    }

                    var parent = parentItemDtoList.FirstOrDefault(p => p.ASIN == item.ParentASIN);
                    if (parent != null)
                        item.OnHold = parent.OnHold;
                }

                //itemDtoList = itemDtoList.Where(i => !i.OnHold).ToList();

                if (itemDtoList.Any())
                {
                    results = BuildProductLines(itemDtoList,
                        parentItemDtoList,
                        styleList,
                        allStyleImageList,
                        styleItemList,
                        allFeatures);
                }
            }

            var stream = CsvExport.Export(results, null, new List<ExcelColumnOverrideInfo>()
            {
                new ExcelColumnOverrideInfo()
                {
                    Title = "DWS Cost",
                    RemoveIt = true,
                }
            });

            return stream;
        }

        public void ExportToFile(string outFilepath,
            IList<string> skuList,
            MarketType market,
            string marketplaceId)
        {
            var stream = Export(skuList, market, marketplaceId);

            var filePath = outFilepath;
            using (var file = File.Open(filePath, FileMode.Create))
            {
                stream.WriteTo(file);
            }
        }

        private IList<GrouponGoodLine> BuildProductLines(IList<ItemExDTO> allItems,
            IList<ParentItemDTO> allParentItems,
            IList<StyleEntireDto> allStyles,
            IList<StyleImageDTO> allStyleImages,
            IList<StyleItemDTO> allStyleItems,
            IList<FeatureValueDTO> allFeatures)
        {
            var results = new List<GrouponGoodLine>();

            foreach (var parentItem in allParentItems)
            {
                var items = allItems.Where(i => i.ParentASIN == parentItem.ASIN).ToList();

                foreach (var item in items)
                {
                    var style = allStyles.FirstOrDefault(s => s.Id == item.StyleId);
                    var styleItem = allStyleItems.FirstOrDefault(si => si.StyleItemId == item.StyleItemId);
                    var styleImages = allStyleImages.Where(si => si.StyleId == style.Id && !si.IsSystem).ToList();
                    var styleFeatures = allFeatures.Where(f => f.StyleId == item.StyleId).ToList();                    

                    var gender = styleFeatures.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.GENDER)?.Value;
                    var subLicense = styleFeatures.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.SUB_LICENSE1)?.Value;
                    var mainLicense = StyleFeatureHelper.PrepareMainLicense(styleFeatures.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.MAIN_LICENSE)?.Value, subLicense);
                    var material = styleFeatures.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.MATERIAL)?.Value;
                    var sleeve = styleFeatures.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.SLEEVE)?.Value;
                    var itemStyleValue = styleFeatures.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.ITEMSTYLE)?.Value;
                    var color1 = styleFeatures.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.COLOR1)?.Value;
                    var shippingSize = styleFeatures.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.SHIPPING_SIZE)?.Value;
                    var isSmallShippingSize = shippingSize == "XS" || shippingSize == "S";

                    var searchTerms = style != null ? style.SearchTerms : null;
                    var isToddler = items.All(i => SizeHelper.IsToddlers(i.StyleSize));
                    var description = style != null ? style.Description : null;
                    description = StringHelper.TrimTags(description);

                    var category = GrouponUtils.GetCategories(gender, itemStyleValue);

                    if (category == null || !category.Any())
                    {
                        _log.Info("No category info");
                        continue;
                    }
                    if (!style.MSRP.HasValue)
                    {
                        _log.Info("No MSRP");
                        //continue;
                    }

                    var packageSize = GrouponUtils.GetSizes(shippingSize);

                    var attributes = FillAttributesByCategory(category,
                        styleFeatures,
                        description,
                        style.Name,
                        item.Color,
                        styleItem.Size ?? item.Size);

                    if (attributes == null || !attributes.Any())
                    {
                        _log.Info("No attributes, category=" + category);
                        continue;
                    }

                    var model = new GrouponGoodLine()
                    {
                        CategoryID = category,
                        VendorSKU = item.SKU,
                        Title = GrouponUtils.PrepareText(StringHelper.Substring(style.Name, 80)),
                        Description = GrouponUtils.PrepareText(StringHelper.GetFirstNotEmpty(description, style.Name)),
                        Manufacturer = mainLicense,
                        ModelNumber = style.StyleID,
                        Brand = mainLicense,
                        IsBundle = "No",
                        ProductIdentifierType = "UPC",
                        ProductIdentifier = item.Barcode,
                        VariationGroupingID = StringHelper.GetFirstNotEmpty(parentItem.GroupId, parentItem.ASIN),

                        UnitPrice = PriceHelper.RoundRoundToTwoPrecision(item.CurrentPrice).ToString(),
                        ShippingCost = PriceHelper.RoundRoundToTwoPrecision(0).ToString(), //NOTE: Free
                        ReferencePrice = style.MSRP.HasValue ? PriceHelper.RoundRoundToTwoPrecision(style.MSRP.Value).ToString() : "",
                        ReferencePriceURL = item.SourceMarketUrl,

                        ProductWeight = (decimal)(item.Weight ?? 1.0),
                        ProductWeightUnit = "Ounces",
                        ProductHeight = packageSize[0],
                        ProductLength = packageSize[1],
                        ProductWidth = packageSize[2],
                        ProductDimensionsUnit = "Inches",
                        IsLTLShippingRequired = "No",
                        PackageWeight = (decimal)(item.Weight ?? 1.0),
                        PackageWeightUnit = "Ounces",
                        PackageHeight = packageSize[0],
                        PackageLength = packageSize[1],
                        PackageWidth = packageSize[2],
                        PackageDimensionsUnit = "Inches",
                        CountryofOrigin = "US",

                        Quantity = Math.Min(5, item.RealQuantity),

                        MainImage = styleImages.Count > 0 ? styleImages[0]?.Image : "",
                        AlternateImage1 = styleImages.Count > 1 ? styleImages[1]?.Image : "",
                        AlternateImage2 = styleImages.Count > 2 ? styleImages[2]?.Image : "",
                        AlternateImage3 = styleImages.Count > 3 ? styleImages[3]?.Image : "",

                        Bullet1Description = GrouponUtils.PrepareText(StringHelper.Substring(StringHelper.TrimTags(style.BulletPoint1), 128)),
                        Bullet2Description = GrouponUtils.PrepareText(StringHelper.Substring(StringHelper.TrimTags(style.BulletPoint2), 128)),
                        Bullet3Description = GrouponUtils.PrepareText(StringHelper.Substring(StringHelper.TrimTags(style.BulletPoint3), 128)),
                        Bullet4Description = GrouponUtils.PrepareText(StringHelper.Substring(StringHelper.TrimTags(style.BulletPoint4), 128)),
                        Bullet5Description = GrouponUtils.PrepareText(StringHelper.Substring(StringHelper.TrimTags(style.BulletPoint5), 128)),

                        Attributes = attributes,
                    };

                    results.Add(model);
                }

                //Extract attributes
                var attributeNames = new Dictionary<string, int>();
                var lastIndex = 1;
                foreach (var model in results)
                {
                    foreach (var attribte in model.Attributes)
                    {
                        if (!attributeNames.ContainsKey(attribte.Name))
                            attributeNames.Add(attribte.Name, lastIndex++);
                    }
                }

                foreach (var model in results)
                {
                    foreach (var attribute in model.Attributes)
                    {
                        var index = attributeNames[attribute.Name];
                        SetAttribute(model, index, attribute);
                    }
                }
            }

           return results;
        }

        private void SetAttribute(GrouponGoodLine model, int attributeIndex, GrouponAttribute attributeValue)
        {
            if (attributeIndex == 1)
            {
                model.Attribute1Name = attributeValue.Name;
                model.Attribute1Value = attributeValue.Value;
            }
            if (attributeIndex == 2)
            {
                model.Attribute2Name = attributeValue.Name;
                model.Attribute2Value = attributeValue.Value;
            }
            if (attributeIndex == 3)
            {
                model.Attribute3Name = attributeValue.Name;
                model.Attribute3Value = attributeValue.Value;
            }
            if (attributeIndex == 4)
            {
                model.Attribute4Name = attributeValue.Name;
                model.Attribute4Value = attributeValue.Value;
            }
            if (attributeIndex == 5)
            {
                model.Attribute5Name = attributeValue.Name;
                model.Attribute5Value = attributeValue.Value;
            }
            if (attributeIndex == 6)
            {
                model.Attribute6Name = attributeValue.Name;
                model.Attribute6Value = attributeValue.Value;
            }
            if (attributeIndex == 7)
            {
                model.Attribute7Name = attributeValue.Name;
                model.Attribute7Value = attributeValue.Value;
            }
            if (attributeIndex == 8)
            {
                model.Attribute8Name = attributeValue.Name;
                model.Attribute8Value = attributeValue.Value;
            }
            if (attributeIndex == 9)
            {
                model.Attribute9Name = attributeValue.Name;
                model.Attribute9Value = attributeValue.Value;
            }
            if (attributeIndex == 10)
            {
                model.Attribute10Name = attributeValue.Name;
                model.Attribute10Value = attributeValue.Value;
            }
            if (attributeIndex == 11)
            {
                model.Attribute11Name = attributeValue.Name;
                model.Attribute11Value = attributeValue.Value;
            }
            if (attributeIndex == 12)
            {
                model.Attribute12Name = attributeValue.Name;
                model.Attribute12Value = attributeValue.Value;
            }
            if (attributeIndex == 13)
            {
                model.Attribute13Name = attributeValue.Name;
                model.Attribute13Value = attributeValue.Value;
            }
            if (attributeIndex == 14)
            {
                model.Attribute14Name = attributeValue.Name;
                model.Attribute14Value = attributeValue.Value;
            }
            if (attributeIndex == 15)
            {
                model.Attribute15Name = attributeValue.Name;
                model.Attribute15Value = attributeValue.Value;
            }
            if (attributeIndex == 16)
            {
                model.Attribute16Name = attributeValue.Name;
                model.Attribute16Value = attributeValue.Value;
            }
            if (attributeIndex == 17)
            {
                model.Attribute17Name = attributeValue.Name;
                model.Attribute17Value = attributeValue.Value;
            }
            if (attributeIndex == 18)
            {
                model.Attribute18Name = attributeValue.Name;
                model.Attribute18Value = attributeValue.Value;
            }
            if (attributeIndex == 19)
            {
                model.Attribute19Name = attributeValue.Name;
                model.Attribute19Value = attributeValue.Value;
            }
            if (attributeIndex == 20)
            {
                model.Attribute20Name = attributeValue.Name;
                model.Attribute20Value = attributeValue.Value;
            }
            if (attributeIndex == 21)
            {
                model.Attribute21Name = attributeValue.Name;
                model.Attribute21Value = attributeValue.Value;
            }
            if (attributeIndex == 22)
            {
                model.Attribute22Name = attributeValue.Name;
                model.Attribute22Value = attributeValue.Value;
            }
            if (attributeIndex == 23)
            {
                model.Attribute23Name = attributeValue.Name;
                model.Attribute23Value = attributeValue.Value;
            }
            if (attributeIndex == 24)
            {
                model.Attribute24Name = attributeValue.Name;
                model.Attribute24Value = attributeValue.Value;
            }
            if (attributeIndex == 25)
            {
                model.Attribute25Name = attributeValue.Name;
                model.Attribute25Value = attributeValue.Value;
            }
        }


        private IList<GrouponAttribute> FillAttributesByCategory(string category,
            IList<FeatureValueDTO> features,
            string description,
            string title,
            string color,
            string size)
        {
            var results = new List<GrouponAttribute>();
            if (StringHelper.IsEqualNoCase(category, "Apparel_Boys'_Underwear"))
            {
                results.Add(new GrouponAttribute()
                {
                    Name = "CARE INSTRUCTIONS",
                    Value = "Label",
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "COLOR",
                    Value = GrouponUtils.PrepareColor(StringHelper.GetFirstNotEmpty(color, features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.COLOR1)?.Value)),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "FABRIC/MATERIAL",
                    Value = StringHelper.IfEmpty(features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.MATERIAL)?.Value, "n/a"),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "SIZING CHART",
                    Value = "Standard",
                });
            }
            if (StringHelper.IsEqualNoCase(category, "Apparel_Girls'_Underwear"))
            {
                results.Add(new GrouponAttribute()
                {
                    Name = "CARE INSTRUCTIONS",
                    Value = "Label",
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "COLOR",
                    Value = GrouponUtils.PrepareColor(StringHelper.GetFirstNotEmpty(color, features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.COLOR1)?.Value)),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "FABRIC/MATERIAL",
                    Value = StringHelper.IfEmpty(features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.MATERIAL)?.Value, "n/a"),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "SIZING CHART",
                    Value = "Standard",
                });
            }

            if (StringHelper.IsEqualNoCase(category, "Apparel_Men's_Shorts_Denim")
                || StringHelper.IsEqualNoCase(category, "Apparel_Men's_Shorts_Denims"))
            {
                results.Add(new GrouponAttribute()
                {
                    Name = "CARE INSTRUCTIONS",
                    Value = "Label",
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "COLOR",
                    Value = GrouponUtils.PrepareColor(StringHelper.GetFirstNotEmpty(color, features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.COLOR1)?.Value)),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "FABRIC/MATERIAL",
                    Value = StringHelper.IfEmpty(features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.MATERIAL)?.Value, "n/a"),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "Men's Apparel Size",
                    Value = GrouponUtils.PrepareMensApparelSize(size),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "SIZING CHART",
                    Value = "Standard",
                });
            }

            if (StringHelper.IsEqualNoCase(category, "Apparel_Men's_Shirts_Dress Shirts"))
            {
                /*Classic
                Modern
                Relaxed
                Slim
                */
                results.Add(new GrouponAttribute()
                {
                    Name = "BUTTON DOWN SHIRT FIT",
                    Value = "Classic",
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "CARE INSTRUCTIONS",
                    Value = "Label",
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "COLOR",
                    Value = GrouponUtils.PrepareColor(StringHelper.GetFirstNotEmpty(color, features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.COLOR1)?.Value)),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "FABRIC/MATERIAL",
                    Value = StringHelper.IfEmpty(features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.MATERIAL)?.Value, "n/a"),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "Men's Apparel Size",
                    Value = GrouponUtils.PrepareMensApparelSize(size),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "SIZING CHART",
                    Value = "Standard",
                });
            }


            if (StringHelper.IsEqualNoCase(category, "Apparel_Women's_Intimates_Slips"))
            {
                results.Add(new GrouponAttribute()
                {
                    Name = "BRA AND UNDERGARMENT MATERIALS",
                    Value = StringHelper.IfEmpty(features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.MATERIAL)?.Value, "n/a"),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "CARE INSTRUCTIONS",
                    Value = "Label",
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "COLOR",
                    Value = GrouponUtils.PrepareColor(StringHelper.GetFirstNotEmpty(color, features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.COLOR1)?.Value)),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "INTIMATES TYPE",
                    Value = "Pajamas",
                });                
                results.Add(new GrouponAttribute()
                {
                    Name = "SIZING CHART",
                    Value = "Standard",
                });
            }
            if (StringHelper.IsEqualNoCase(category, "Apparel_Men's_Underwear_Collections & Sets"))
            {
                results.Add(new GrouponAttribute()
                {
                    Name = "BRA AND UNDERGARMENT MATERIALS",
                    Value = StringHelper.IfEmpty(features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.MATERIAL)?.Value, "n/a"),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "CARE INSTRUCTIONS",
                    Value = "Label",
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "COLOR",
                    Value = GrouponUtils.PrepareColor(StringHelper.GetFirstNotEmpty(color, features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.COLOR1)?.Value)),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "Men's Apparel Size",
                    Value = GrouponUtils.PrepareMensApparelSize(size),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "SIZING CHART",
                    Value = "Standard",
                });
            }

            if (StringHelper.IsEqualNoCase(category, "Apparel_Women's_Sleep & Lounge_Onesies"))
            {
                results.Add(new GrouponAttribute()
                {
                    Name = "CARE INSTRUCTIONS",
                    Value = "Label",
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "COLOR",
                    Value = GrouponUtils.PrepareColor(StringHelper.GetFirstNotEmpty(color, features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.COLOR1)?.Value)),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "FABRIC/MATERIAL",
                    Value = StringHelper.IfEmpty(features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.MATERIAL)?.Value, "n/a"),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "INTIMATES TYPE",
                    Value = "Pajamas",
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "LOUNGE & SLEEPWEAR TYPE",
                    Value = "Pajama Sets"
                    /*Loungewear
                    Nightshirts
                    Pajama Bottoms (Shorts & Pants)
                    Pajama Sets
                    Pajamas Bottoms (Shorts & Pants)
                    */
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "Women's Apparel Size",
                    Value = GrouponUtils.PrepareSize(size),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "SIZING CHART",
                    Value = "Standard",
                });
            }

            if (StringHelper.IsEqualNoCase(category, "Apparel_Women's_Sleep & Lounge_Nightgowns & Sleepshirts")
                || StringHelper.IsEqualNoCase(category, "Apparel_Women's_Sleep & Lounge_Sleep Sets")
                || StringHelper.IsEqualNoCase(category, "Apparel_Women's_Sleep & Lounge_Sleep Bottoms")
                || StringHelper.IsEqualNoCase(category, "Apparel_Women's_Sleep & Lounge_Robes"))
            {
                results.Add(new GrouponAttribute()
                {
                    Name = "APPAREL AND FOOTWEAR MATERIAL",
                    Value = StringHelper.IfEmpty(features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.MATERIAL)?.Value, "n/a"),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "CARE INSTRUCTIONS",
                    Value = "Label",
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "COLOR",
                    Value = GrouponUtils.PrepareColor(StringHelper.GetFirstNotEmpty(color, features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.COLOR1)?.Value)),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "INTIMATES TYPE",
                    Value = "Pajamas",
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "LOUNGE & SLEEPWEAR TYPE",
                    Value = "Pajama Sets"
                    /*Loungewear
                    Nightshirts
                    Pajama Bottoms (Shorts & Pants)
                    Pajama Sets
                    Pajamas Bottoms (Shorts & Pants)
                    */
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "Women's Apparel Size",
                    Value = GrouponUtils.PrepareMensApparelSize(size),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "SIZING CHART",
                    Value = "Standard",
                });
            }

            if (StringHelper.IsEqualNoCase(category, "Apparel_Men's_Sleep & Lounge_Sleep Sets")
                || StringHelper.IsEqualNoCase(category, "Apparel_Men's_Sleep & Lounge_Sleep Tops")
                || StringHelper.IsEqualNoCase(category, "Apparel_Men's_Sleep & Lounge_Sleep Bottoms")
                || StringHelper.IsEqualNoCase(category, "Apparel_Men's_Sleep & Lounge_Robes"))
            {
                results.Add(new GrouponAttribute()
                {
                    Name = "APPAREL AND FOOTWEAR MATERIAL",
                    Value = StringHelper.IfEmpty(features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.MATERIAL)?.Value, "n/a"),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "CARE INSTRUCTIONS",
                    Value = "Label",
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "COLOR",
                    Value = GrouponUtils.PrepareColor(StringHelper.GetFirstNotEmpty(color, features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.COLOR1)?.Value)),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "LOUNGE & SLEEPWEAR TYPE",
                    Value = "Pajama Sets"
                    /*Loungewear
                    Nightshirts
                    Pajama Bottoms (Shorts & Pants)
                    Pajama Sets
                    Pajamas Bottoms (Shorts & Pants)
                    */
                });

                results.Add(new GrouponAttribute()
                {
                    Name = "Men's Apparel Size",
                    Value = GrouponUtils.PrepareMensApparelSize(size),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "SIZING CHART",
                    Value = "Standard",
                });
            }

            if (StringHelper.IsEqualNoCase(category, "Apparel_Boys'_Sleepwear_Gowns"))
            {
                results.Add(new GrouponAttribute()
                {
                    Name = "CARE INSTRUCTIONS",
                    Value = "Label",
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "COLOR",
                    Value = GrouponUtils.PrepareColor(StringHelper.GetFirstNotEmpty(color, features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.COLOR1)?.Value)),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "FABRIC/MATERIAL",
                    Value = StringHelper.IfEmpty(features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.MATERIAL)?.Value, "n/a"),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "MADE IN USA",
                    Value = "N"
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "SIZING CHART",
                    Value = "Standard",
                });
            }
            if (StringHelper.IsEqualNoCase(category, "Apparel_Girls'_Sleepwear_Gowns"))
            {
                results.Add(new GrouponAttribute()
                {
                    Name = "CARE INSTRUCTIONS",
                    Value = "Label",
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "COLOR",
                    Value = GrouponUtils.PrepareColor(StringHelper.GetFirstNotEmpty(color, features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.COLOR1)?.Value)),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "FABRIC/MATERIAL",
                    Value = StringHelper.IfEmpty(features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.MATERIAL)?.Value, "n/a"),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "SIZING CHART",
                    Value = "Standard",
                });
            }

            if (StringHelper.IsEqualNoCase(category, "Apparel_Boys'_Pajamas")
                || StringHelper.IsEqualNoCase(category, "Apparel_Girls'_Pajamas"))
            {
                results.Add(new GrouponAttribute()
                {
                    Name = "BABY CLOTHING AND ACCESSORY MATERIALS",
                    Value = StringHelper.IfEmpty(features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.MATERIAL)?.Value, "n/a"),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "CARE INSTRUCTIONS",
                    Value = "Label",
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "Children's Apparel Size",
                    Value = size,
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "Color",
                    Value = GrouponUtils.PrepareColor(StringHelper.GetFirstNotEmpty(color, features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.COLOR1)?.Value)),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "MADE IN USA",
                    Value = "N"
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "SIZING CHART",
                    Value = "Standard",
                });
            }

            if (StringHelper.IsEqualNoCase(category, "Apparel_Boys'_Bath_Robes")
                || StringHelper.IsEqualNoCase(category, "Apparel_Girls'_Bath_Robes"))
            {                
                results.Add(new GrouponAttribute()
                {
                    Name = "CARE INSTRUCTIONS",
                    Value = "Label",
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "Children's Apparel Size",
                    Value = size,
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "COLOR",
                    Value = GrouponUtils.PrepareColor(StringHelper.GetFirstNotEmpty(color, features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.COLOR1)?.Value)),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "FABRIC/MATERIAL",
                    Value = StringHelper.IfEmpty(features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.MATERIAL)?.Value, "n/a"),
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "MADE IN USA",
                    Value = "N"
                });
                results.Add(new GrouponAttribute()
                {
                    Name = "SIZING CHART",
                    Value = "Standard",
                });
            }

            return results;
        }
    }
}
