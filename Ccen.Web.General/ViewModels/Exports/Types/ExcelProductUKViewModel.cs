using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Amazon.Api.Exports;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Exports.Attributes;
using Amazon.Core.Models;
using Amazon.Core.Models.Items;
using Amazon.DTO.Inventory;
using Amazon.DTO.Users;
using Amazon.Web.Models.Exports;
using Amazon.Web.ViewModels.Inventory;

namespace Amazon.Web.ViewModels.ExcelToAmazon
{
    public class ExcelProductUKViewModel : IExcelProductViewModel
    {
        public static string UKTemplatePath = "~/App_Data/Flat.File.Clothing.OneSheet.UK.xls";


        public long? StyleId { get; set; }
        public long? StyleItemId { get; set; }


        [ExcelSerializable("SKU", Order = 0, Width = 25)]
        public string SKU { get; set; }
        
        //[ExcelSerializable("Product ID", Order = 1, Width = 25)]
        public string ASIN { get; set; }

        [ExcelSerializable("Product ID", Order = 1, Width = 25)]
        public string UPC { get; set; }
        
        [ExcelSerializable("Product ID Type", Order = 2, Width = 25)]
        public string ProductId { get; set; }//Hardcode to word “ASIN”

        [ExcelSerializable("Product Name", Order = 3, Width = 65)]
        public string Title { get; set; }
        
        [ExcelSerializable("Brand Name", Order = 4, Width = 25)]
        public string BrandName { get; set; }//Can be different for child elements

        [ExcelSerializable("Clothing Type", Order = 5, Width = 25)]
        public string ClothingType { get; set; }

        [ExcelSerializable("Product Description", Order = 6, Width = 75)]
        public string Description { get; set; }//U can put same as parent’s description for all elements
        
        [ExcelSerializable("Update", Order = 7, Width = 25)]
        public string Update { get; set; }//Hardcode to word “update”


        //[ExcelSerializable("Suggested Price", Order = 10, Width = 25)]
        //public string SuggestedPrice { get; set; }//Could be empty
        

        [ExcelSerializable("Standard Price", Order = 10, Width = 25)]
        public string StandardPrice { get; set; }

        //[ExcelSerializable("Currency", Order = 11, Width = 25)]
        public string Currency { get; set; }//If Suggested Price provided hardcode to “USD”

        [ExcelSerializable("Quantity", Order = 11, Width = 25)]
        public string Quantity { get; set; }


        


        [ExcelSerializable("Recommended Browse Nodes1", Order = 26, Width = 25)]
        public string RecommendedBrowseNodes1 { get; set; }


        [ExcelSerializable("SearchTerms", Order = 27, Width = 25)]
        public string SearchTerms1 { get; set; }

        //[ExcelSerializable("SearchTerms2", Order = 31, Width = 25)]
        //public string SearchTerms2 { get; set; }

        //[ExcelSerializable("SearchTerms3", Order = 32, Width = 25)]
        //public string SearchTerms3 { get; set; }

        //[ExcelSerializable("SearchTerms4", Order = 33, Width = 25)]
        //public string SearchTerms4 { get; set; }

        //[ExcelSerializable("SearchTerms5", Order = 34, Width = 25)]
        //public string SearchTerms5 { get; set; }


        [ExcelSerializable("KeyProductFeatures1", Order = 28, Width = 25)]
        public string KeyProductFeatures1 { get; set; }

        [ExcelSerializable("KeyProductFeatures2", Order = 29, Width = 25)]
        public string KeyProductFeatures2 { get; set; }

        [ExcelSerializable("KeyProductFeatures3", Order = 30, Width = 25)]
        public string KeyProductFeatures3 { get; set; }

        [ExcelSerializable("KeyProductFeatures4", Order = 31, Width = 25)]
        public string KeyProductFeatures4 { get; set; }

        [ExcelSerializable("KeyProductFeatures5", Order = 32, Width = 25)]
        public string KeyProductFeatures5 { get; set; }

        



        [ExcelSerializable("Main Image URL", Order = 33, Width = 57)]
        public string MainImageURL { get; set; }
        [ExcelSerializable("OtherImageUrl1", Order = 34, Width = 25)]
        public string OtherImageUrl1 { get; set; }

        [ExcelSerializable("OtherImageUrl2", Order = 35, Width = 25)]
        public string OtherImageUrl2 { get; set; }

        [ExcelSerializable("OtherImageUrl3", Order = 36, Width = 25)]
        public string OtherImageUrl3 { get; set; }
        


        [ExcelSerializable("Parentage", Order = 46, Width = 25)]
        public string Parentage { get; set; } //Hardcode to 4th row – “parent”, all other rows “Child” 

        [ExcelSerializable("Parent SKU", Order = 47, Width = 35)]
        public string ParentSKU { get; set; } //BS4 –empty, others copy A4 

        [ExcelSerializable("Relationship Type", Order = 48, Width = 25)]
        public string RelationshipType { get; set; } //“Variation”, BT4 – empty

        [ExcelSerializable("Variation Theme", Order = 49, Width = 25)]
        public string VariationTheme { get; set; } //Hardcode to “Size”



        [ExcelSerializable("Color", Order = 52, Width = 25)]
        public string Color { get; set; } //Can be different for child elements
        
        [ExcelSerializable("Size", Order = 54, Width = 25)]
        public string Size { get; set; } //DQ4 - empty

        [ExcelSerializable("Material Composition", Order = 55, Width = 25)]
        public string MaterialComposition { get; set; }


        [ExcelSerializable("Department", Order = 63, Width = 25)]
        public string Department { get; set; } //Usually: girls, boys, baby-boys, baby-girls


        public static string SizeConverter(string size, string material)
        {
            var lowerMaterial = (material ?? "").ToLower();
            var lowerSize = (size ?? "").ToLower();

            if (lowerMaterial.ToLower() == "cotton")
            {
                switch (lowerSize)
                {
                    case "2t":
                    case "2":
                        return "12M-24M";
                    case "3t":
                    case "3":
                        return "2-3 years";
                    case "4t":
                        return "3-4 years";
                    case "4":
                    case "4/5":
                    case "4-5":
                        return "4 years";
                    case "5":
                        return "5 years";
                    case "5/6":
                    case "5-6":
                        return "5-6 years";
                    //case "6x":
                    //return "6x"
                    case "6":
                    case "6/6x":
                    case "6-6x":
                        return "5-6 years";
                    case "8":
                    case "7":
                    case "7/8":
                    case "7-8":
                        return "7-8 years";
                    case "10":
                        return "9-10 years";
                    case "10/12":
                    case "10-12":
                        return "9-11 years";
                    case "12":
                        return "11 years";
                    default:
                        return size;
                }
            }
            else
            {
                switch (lowerSize)
                {
                    case "2t":
                    case "2":
                        return "2 years";
                    case "3t":
                    case "3":
                        return "3 years";
                    case "4t":
                        return "4 years";
                    case "4":
                    case "4/5":
                    case "4-5":
                        return "4-5 years";
                    case "5":
                        return "5 years";
                    case "5/6":
                    case "5-6":
                        return "5-6 years";
                    //case "6x":
                    //return "6x"
                    case "6":
                    case "6/6x":
                    case "6-6x":
                        return "6 years";
                    case "8":
                    case "7":
                    case "7/8":
                    case "7-8":
                        return "7-8 years";
                    case "10":
                        return "10 years";
                    case "10/12":
                    case "10-12":
                        return "10-12 years";
                    case "12":
                        return "12 years";
                    case "14/16":
                    case "14-16":
                        return "14-16 years";
                    default:
                        return size;
                }
            }
        }



        public static string GetRecommendedBrowseNodes1(string gender, string size, string type)
        {
            /*
             * Boys' Pyjama Sets	1730804031
                Girls' Pyjama Sets	1730894031
                Girls' Nightgown	1730892031
            */
            gender = gender ?? "";
            type = type ?? "";

            if (gender.Contains("boys"))
            {
                return "1730804031";
            }
            if (gender.Contains("girl"))
            {
                if (type.Contains("nightgowns"))
                {
                    return "1730892031";
                }
                return "1730894031";
            }
            return "";
        }
 

        public static MemoryStream ExportToExcelUK(ILogService log,
            ITime time,
            IMarketCategoryService categoryService,
            IHtmlScraperService htmlScraper,
            IMarketApi marketApi,
            IUnitOfWork db,
            CompanyDTO company,
            string asin,
            MarketType market,
            string marketplaceId,
            bool useStyleImage,
            out string filename)
        {
            var models = GetItemsFor(log, 
                time,
                categoryService,
                htmlScraper, 
                marketApi, 
                db, 
                company, 
                asin, 
                market,
                marketplaceId,
                useStyleImage ? UseStyleImageModes.StyleImage : UseStyleImageModes.ListingImage,
                out filename);

            return ExcelHelper.ExportIntoFile(HttpContext.Current.Server.MapPath(UKTemplatePath), 
                "Template",
                models);
        }

        public static IList<ExcelProductUKViewModel> GetItemsFor(ILogService log,
            ITime time,
            IMarketCategoryService categoryService,
            IHtmlScraperService htmlScraper,
            IMarketApi marketApi,
            IUnitOfWork db,
            CompanyDTO company,
            string asin,
            MarketType market,
            string marketplaceId,
            UseStyleImageModes useStyleImageMode,
            out string filename)
        {
            var gbpExchangeRate = PriceHelper.GBPtoUSD;
            var today = time.GetAppNowTime();

            var models = new List<ExcelProductUKViewModel>();
            var items = ItemExportHelper.GetParentItemAndChilds(log,
                time,
                htmlScraper,
                marketApi,
                db,
                company,
                asin,
                null,
                ExportToExcelMode.Normal,
                market,
                marketplaceId);

            var parent = items.Item1;// db.ParentItems.GetAsDTO(asin);
            var children = items.Item2;// db.Items.GetAllActualExAsDto().Where(i => i.ParentASIN == asin).ToList();
            var firstChild = children.FirstOrDefault();

            FeatureValueDTO subLicense = null;
            string material = null;
            string materialComposition = null;
            string itemStyle = null;
            if (firstChild != null && firstChild.StyleId.HasValue)
            {
                subLicense = db.FeatureValues.GetValueByStyleAndFeatureId(firstChild.StyleId.Value, StyleFeatureHelper.SUB_LICENSE1);
                itemStyle = ItemExportHelper.GetFeatureValue(db.FeatureValues.GetValueByStyleAndFeatureId(firstChild.StyleId.Value, StyleFeatureHelper.ITEMSTYLE));
                material = ItemExportHelper.GetFeatureValue(db.FeatureValues.GetValueByStyleAndFeatureId(firstChild.StyleId.Value, StyleFeatureHelper.MATERIAL));
                materialComposition = ItemExportHelper.GetFeatureValue(db.FeatureValues.GetValueByStyleAndFeatureId(firstChild.StyleId.Value, StyleFeatureHelper.MATERIAL_COMPOSITION));
            }

            if (parent != null)
            {
                var variationType = ItemExportHelper.GetVariationType(children != null ? children.Select(ch => ch.Color).ToList() : null);

                string childBrandName = null;
                decimal? childListPrice = null;
                string childDepartment = null;
                string childSize = null;
                string childStyleString = null;

                IList<string> childFeatures = new List<string>();
                //IList<string> searchTermList = new List<string>();

                //if (!String.IsNullOrEmpty(parent.SearchKeywords))
                //    searchTermList = parent.SearchKeywords.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                var searchTerms = (parent.SearchKeywords ?? "").Replace(";", ", ");
                searchTerms = ItemExportHelper.PrepareSearchTerms(searchTerms);

                if (firstChild != null)
                {
                    childBrandName = children.Select(c => c.BrandName).FirstOrDefault();
                    childFeatures = !String.IsNullOrEmpty(firstChild.Features)
                        ? firstChild.Features.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList()
                        : new List<string>();

                    childListPrice = firstChild.ListPrice;
                    childDepartment = firstChild.Department;
                    childSize = firstChild.Size;
                    childStyleString = firstChild.StyleString;
                }

                //For simplicity, w can just use for parent SKU (unless it exist) style-%parentASIN%.
                string parentSku = parent.SKU;
                if (String.IsNullOrEmpty(parentSku))
                    parentSku = childStyleString + "-" + parent.ASIN;

                var gender = childDepartment ?? parent.Department ?? "";
                var sizeType = ItemExportHelper.GetSizeType(childSize);
                var categoryInfo = categoryService.GetCategory(market, marketplaceId, itemStyle, gender, sizeType);

                //var itemType = ItemExportHelper.GetItemType(itemStyle);
                var newItemType = categoryInfo.Key1;// ItemExportHelper.ItemTypeConverter(childSize ?? "", itemType, itemStyle, gender);
                var newDepartment = StringHelper.GetFirstNotEmpty(categoryInfo.Key2, ItemExportHelper.DepartmentConverter(gender, newItemType, sizeType));
                string clothingType = categoryInfo.Key3;
                var parentImage = ItemExportHelper.ImageConverter(StringHelper.GetFirstNotEmpty(
                    parent.LargeImage != null ? parent.LargeImage.Image : null,
                    parent.ImageSource));

                var replaces = new Dictionary<string, string>()
                {
                    {"Nightgown", "Nighty"},
                    {"Nightys", "Nighties"},
                    {"nightgown", "nighty"},
                    {"nighty", "nighties"},
                    {"pajama", "pyjama"},
                    {"Pajama", "Pyjama"},
                };

                var hasCotton = StringHelper.GetInOneOfStrings("Cotton", new List<string>()
                {
                    searchTerms,
                    parent.AmazonName,
                    parent.Description,
                    firstChild != null ? firstChild.Features : null
                });

                if (String.IsNullOrEmpty(materialComposition))
                {
                    if (hasCotton)
                        materialComposition = "Cotton";
                    else
                        materialComposition = "Polyester";
                }

                models.Add(new ExcelProductUKViewModel
                {
                    SKU = parentSku,
                    Title = StringHelper.Replaces(parent.AmazonName, replaces),
                    ASIN = parent.ASIN,
                    UPC = "",
                    ProductId = "",// "ASIN",
                    BrandName = parent.BrandName ?? childBrandName,
                    Description = parent.Description,
                    ClothingType = clothingType,
                    Update = "Update",
                    StandardPrice = "",

                    //SuggestedPrice = childListPrice.HasValue ? PriceHelper.Convert(childListPrice.Value / 100, gbpExchangeRate, false).ToString("G") : "",
                    Currency = "", // childListPrice.HasValue ? "GBP" : "",

                    Quantity = "",

                    RecommendedBrowseNodes1 = ExcelProductUKViewModel.GetRecommendedBrowseNodes1(newDepartment, childSize, newItemType),

                    KeyProductFeatures1 = childFeatures.Count > 0 ? childFeatures[0] : String.Empty,
                    KeyProductFeatures2 = childFeatures.Count > 1 ? childFeatures[1] : String.Empty,
                    KeyProductFeatures3 = childFeatures.Count > 2 ? childFeatures[2] : String.Empty,
                    KeyProductFeatures4 = childFeatures.Count > 3 ? childFeatures[3] : String.Empty,
                    KeyProductFeatures5 = childFeatures.Count > 4 ? childFeatures[4] : String.Empty,

                    SearchTerms1 = searchTerms,
                    //SearchTerms2 = searchTermList.Count > 1 ? searchTermList[1] : String.Empty,
                    //SearchTerms3 = searchTermList.Count > 2 ? searchTermList[2] : String.Empty,
                    //SearchTerms4 = searchTermList.Count > 3 ? searchTermList[3] : String.Empty,
                    //SearchTerms5 = searchTermList.Count > 4 ? searchTermList[4] : String.Empty,

                    MainImageURL = parentImage,
                    Parentage = "Parent",
                    VariationTheme = variationType,

                    MaterialComposition = materialComposition,
                    Department = newDepartment,
                    Color = "",
                });

                foreach (var child in children)
                {
                    var styleImages = child.StyleId.HasValue ?
                        db.StyleImages
                        .GetAllAsDto()
                        .Where(im => im.StyleId == child.StyleId.Value && im.IsDefault)
                        .ToList()
                            : null;

                    IList<string> features = !String.IsNullOrEmpty(child.Features)
                        ? child.Features.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList()
                        : new List<string>();

                    var childImage = ItemExportHelper.ImageConverter(StringHelper.GetFirstNotEmpty(
                        child.LargeImageUrl,
                        child.ImageUrl));

                    if (useStyleImageMode == UseStyleImageModes.StyleImage)
                    {
                        if (styleImages != null && styleImages.Any())
                            childImage = styleImages.First().Image;
                    }

                    var itemPrice = child.SalePrice.HasValue && child.SaleStartDate < today
                        ? child.SalePrice.Value
                        : child.CurrentPrice;

                    models.Add(new ExcelProductUKViewModel
                    {
                        StyleId = child.StyleId,
                        StyleItemId = child.StyleItemId,

                        SKU = child.SKU,
                        Title = StringHelper.Replaces(child.Name, replaces),
                        ASIN = child.ASIN,
                        UPC = child.Barcode,
                        ProductId = "UPC",// "ASIN",
                        BrandName = child.BrandName,
                        Description = parent.Description,
                        ClothingType = clothingType,
                        Update = "Update",

                        StandardPrice = CorrectUKPrice(PriceHelper.Convert(itemPrice, gbpExchangeRate, true)).ToString("G"),
                        //SuggestedPrice = child.ListPrice.HasValue ? PriceHelper.Convert(child.ListPrice.Value / 100, gbpExchangeRate, false).ToString("G") : "",
                        Currency = "GBP",
                        Quantity = child.RealQuantity.ToString("G"),// "1", // i.Quantity.ToString("G"),

                        KeyProductFeatures1 = features.Count > 0 ? features[0] : String.Empty,
                        KeyProductFeatures2 = features.Count > 1 ? features[1] : String.Empty,
                        KeyProductFeatures3 = features.Count > 2 ? features[2] : String.Empty,
                        KeyProductFeatures4 = features.Count > 3 ? features[3] : String.Empty,
                        KeyProductFeatures5 = features.Count > 4 ? features[4] : String.Empty,

                        SearchTerms1 = searchTerms,
                        //SearchTerms2 = searchTermList.Count > 1 ? searchTermList[1] : String.Empty,
                        //SearchTerms3 = searchTermList.Count > 2 ? searchTermList[2] : String.Empty,
                        //SearchTerms4 = searchTermList.Count > 3 ? searchTermList[3] : String.Empty,
                        //SearchTerms5 = searchTermList.Count > 4 ? searchTermList[4] : String.Empty,


                        RecommendedBrowseNodes1 = ExcelProductUKViewModel.GetRecommendedBrowseNodes1(newDepartment, childSize, newItemType),

                        MainImageURL = childImage,
                        Parentage = "Child",
                        ParentSKU = parentSku,
                        RelationshipType = "Variation",
                        VariationTheme = variationType,
                        Department = newDepartment,
                        Color = child.Color,

                        MaterialComposition = materialComposition,
                        Size = ItemExportHelper.PrepareSize(ExcelProductUKViewModel.SizeConverter(child.Size, material), gender),
                    });
                }

                if (children.Count > 0
                    && useStyleImageMode == UseStyleImageModes.StyleImage)
                    models[0].MainImageURL = models[1].MainImageURL;
            }

            filename = models[0].SKU + "_" + (subLicense != null ? subLicense.Value : "none") + "_UK.xls";

            return models;
        }

        private static decimal CorrectUKPrice(decimal price)
        {
            if (price == 20.99M)
                return 19.99M;
            return price;
        }



        public static IList<ExcelProductUKViewModel> GenerateToExcelUK(IUnitOfWork db,
            IBarcodeService barcodeService,
            IMarketCategoryService categoryService,
            StyleViewModel model,
            DateTime when,
            out string filename)
        {
            var gbpExchangeRate = PriceHelper.GBPtoUSD;

            var models = new List<ExcelProductUKViewModel>();

            var parent = new ExcelProductUKViewModel();
            var childs = new List<ExcelProductUKViewModel>();

            var sizes = model.StyleItems.Items;

            //Size Group, Size Range
            var hasKids2 = sizes.Any(s => ItemExportHelper.GetSizeGroupByName(s.SizeGroupName, true) == ExportSizeGroup.Kids2);
            ExportSizeGroup? sizeGroup = sizes.Any() ? (ExportSizeGroup?)ItemExportHelper.GetSizeGroupByName(sizes.Last().SizeGroupName, false) : null;
            var sizeGroupName = ItemExportHelper.GetSizeGroupName(sizeGroup);

            var sizeRange = ItemExportHelper.GetSizeRangeName(sizes.Select(s => s.Size).ToList(), hasKids2);

            var firstSize = sizes.Any() ? sizes[0].Size : String.Empty;


            //Features Values
            var features = model.Features.Select(f => new FeatureValueDTO()
            {
                FeatureId = f.FeatureId,
                Value = f.Value,
            }).ToList();
            var allFeatureValues = db.FeatureValues.GetAllFeatureValueByItemType(1);

            var gender = ItemExportHelper.GetFeatureValue(features, allFeatureValues, StyleFeatureHelper.GENDER);
            var itemStyle = ItemExportHelper.GetFeatureValue(features, allFeatureValues, StyleFeatureHelper.ITEMSTYLE);
            var sleeve = ItemExportHelper.GetFeatureValue(features, allFeatureValues, StyleFeatureHelper.SLEEVE);
            var material = ItemExportHelper.GetFeatureValue(features, allFeatureValues, StyleFeatureHelper.MATERIAL);

            var color1 = ItemExportHelper.GetFeatureValue(features, allFeatureValues, StyleFeatureHelper.COLOR1);
            var mainLicense = ItemExportHelper.GetFeatureValue(features, allFeatureValues, StyleFeatureHelper.MAIN_LICENSE);
            var subLicense = ItemExportHelper.GetFeatureValue(features, allFeatureValues, StyleFeatureHelper.SUB_LICENSE1);
            //var shippingSize = ItemExportHelper.GetFeatureValue(features, allFeatureValues, StyleFeatureHelper.SHIPPING_SIZE);

            var materialComposition = ItemExportHelper.GetFeatureTextValue(features, StyleFeatureHelper.MATERIAL_COMPOSITION);

            var sizeType = ItemExportHelper.GetSizeType(firstSize);
            var categoryInfo = categoryService.GetCategory(MarketType.AmazonEU, MarketplaceKeeper.AmazonUkMarketplaceId, itemStyle, gender, sizeType);
            //var itemType = ItemExportHelper.GetItemType(itemStyle);

            var brandName = ItemExportHelper.GetBrandName(mainLicense, subLicense);

            var newItemType = categoryInfo.Key1;// ItemExportHelper.ItemTypeConverter(firstSize ?? "", itemType, itemStyle, gender);
            var newDepartment = StringHelper.GetFirstNotEmpty(categoryInfo.Key2, ItemExportHelper.DepartmentConverter(gender, newItemType, sizeType));


            var searchTerms = model.SearchTerms;// ItemExportHelper.BuildSearchTerms(itemStyle, material, sleeve);
            var keyFeatures = model.GetBulletPoints();// ItemExportHelper.BuildKeyFeatures(mainLicense, subLicense, material);
            

            var hasCotton = StringHelper.GetInOneOfStrings("Cotton", new List<string>()
                {
                    searchTerms,
                    model.Name,
                    model.Description,
                    material
                });

            if (String.IsNullOrEmpty(materialComposition))
            {
                if (hasCotton)
                    materialComposition = "Cotton";
                else
                    materialComposition = "Polyester";
            }


            //--------------------------
            //Parent item
            //--------------------------
            parent.SKU = model.StyleId;
            parent.ASIN = "";

            parent.Title = model.Name + "," + (!String.IsNullOrEmpty(sizeGroupName) ? " " + ItemExportHelper.FormatSizeGroupName(sizeGroupName) : String.Empty) + " " + sizeRange;
            //If it causes name length to exceed maximum allowed, please drop words “Kids” or Toddler”
            var inlcudeSizeGroup = true;
            if (parent.Title.Length > ItemExportHelper.MaxItemNameLength)
            {
                parent.Title = model.Name + ", " + sizeRange;
                inlcudeSizeGroup = false;
            }

            parent.ProductId = "";
            parent.BrandName = brandName;

            parent.ClothingType = "sleepwear"; // = newItemType;

            var images = new List<string>();
            var swatchImage = "";
            if (model.ImageSet != null)
            {
                images = model.ImageSet.Images
                    .Where(im => im.Category != (int)StyleImageCategories.Swatch)
                    .OrderByDescending(im => ImageHelper.GetSortIndex(im.Category))
                    .ThenByDescending(im => im.IsDefault)
                    .ThenBy(im => im.Id)
                    .Select(im => im.ImageUrl)
                    .ToList();
                swatchImage = model.ImageSet.Images
                    .FirstOrDefault(im => im.Category == (int)StyleImageCategories.Swatch)?.ImageUrl;

                parent.MainImageURL = images.Count > 0 ? images[0] : "";
                parent.OtherImageUrl1 = images.Count > 1 ? images[1] : "";
                parent.OtherImageUrl2 = images.Count > 2 ? images[2] : "";
                parent.OtherImageUrl3 = images.Count > 3 ? images[3] : "";
                //parent.SwatchImageUrl = swatchImage;
            }

            parent.Color = "";
            parent.Department = newDepartment;

            parent.Size = "";
            parent.MaterialComposition = materialComposition;
            parent.Description = model.Description;
            parent.Update = "Update";
            parent.StandardPrice = "";
            //parent.SuggestedPrice = model.MSRP.ToString("G");
            parent.Currency = "GBP";

            parent.Quantity = "";

            parent.RecommendedBrowseNodes1 = ExcelProductUKViewModel.GetRecommendedBrowseNodes1(newDepartment, firstSize, newItemType);

            parent.SearchTerms1 = searchTerms;

            parent.KeyProductFeatures1 = keyFeatures.Count > 0 ? keyFeatures[0] : "";
            parent.KeyProductFeatures2 = keyFeatures.Count > 1 ? keyFeatures[1] : "";
            parent.KeyProductFeatures3 = keyFeatures.Count > 2 ? keyFeatures[2] : "";
            parent.KeyProductFeatures4 = keyFeatures.Count > 3 ? keyFeatures[3] : "";
            parent.KeyProductFeatures5 = keyFeatures.Count > 4 ? keyFeatures[4] : "";


            parent.Parentage = "Parent";
            parent.ParentSKU = "";
            parent.RelationshipType = "";
            parent.VariationTheme = "Size";

            //--------------------------
            //Child items
            //--------------------------
            foreach (var size in sizes)
            {
                var child = new ExcelProductUKViewModel();
                child.SKU = model.StyleId + "-" + ItemExportHelper.ConvertSizeForStyleId(size.Size, hasKids2);
                if (size.AutoGeneratedBarcode)
                {
                    var newBarcode = BarcodeHelper.GenerateBarcode(barcodeService, child.SKU, when);
                    if (!String.IsNullOrEmpty(newBarcode))
                    {
                        if (size.Barcodes == null)
                            size.Barcodes = new List<BarcodeDTO>();
                        size.Barcodes.Insert(0, new BarcodeDTO()
                        {
                            Barcode = newBarcode
                        });
                    }
                }

                child.UPC = (size.Barcodes != null && size.Barcodes.Any()) ? size.Barcodes.FirstOrDefault().Barcode : String.Empty;

                child.Title = model.Name + "," + (inlcudeSizeGroup ? " " + ItemExportHelper.FormatSizeGroupName(size.SizeGroupName) : "") + " Size " + ItemExportHelper.ConvertSizeForItemName(size.Size, hasKids2);

                child.ProductId = "UPC";
                child.BrandName = brandName;

                child.ClothingType = "sleepwear";// newItemType;

                child.MainImageURL = images.Count > 0 ? images[0] : "";
                child.OtherImageUrl1 = images.Count > 1 ? images[1] : "";
                child.OtherImageUrl2 = images.Count > 2 ? images[2] : "";
                child.OtherImageUrl3 = images.Count > 3 ? images[3] : "";
                //child.SwatchImageUrl = swatchImage;

                child.Color = ItemExportHelper.PrepareColor(String.IsNullOrEmpty(size.Color) ? color1 : size.Color);
                child.Department = newDepartment;

                child.Size = ExcelProductUKViewModel.SizeConverter(size.Size, material);
                child.MaterialComposition = materialComposition;
                child.Description = model.Description;
                child.Update = "Update";
                child.StandardPrice = PriceHelper.Convert(model.Price, gbpExchangeRate, true).ToString("G");
                //child.SuggestedPrice = model.MSRP.ToString("G");
                child.Currency = "GBP";

                child.Quantity = size.Quantity.ToString();

                child.RecommendedBrowseNodes1 = ExcelProductUKViewModel.GetRecommendedBrowseNodes1(newDepartment, child.Size, newItemType);

                child.KeyProductFeatures1 = parent.KeyProductFeatures1;
                child.KeyProductFeatures2 = parent.KeyProductFeatures2;
                child.KeyProductFeatures3 = parent.KeyProductFeatures3;
                child.KeyProductFeatures4 = parent.KeyProductFeatures4;
                child.KeyProductFeatures5 = parent.KeyProductFeatures5;

                child.SearchTerms1 = parent.SearchTerms1;
                //child.SearchTerms2 = parent.SearchTerms2;
                //child.SearchTerms3 = parent.SearchTerms3;
                //child.SearchTerms4 = parent.SearchTerms4;
                //child.SearchTerms5 = parent.SearchTerms5;

                child.Parentage = "Child";
                child.ParentSKU = parent.SKU;
                child.RelationshipType = "Variation";
                child.VariationTheme = "Size";

                childs.Add(child);
            }


            models.Add(parent);
            models.AddRange(childs);

            filename = model.StyleId + "_" + subLicense;
            filename = filename.Replace(" ", "") + "_UK.xls";
            return models; //ExcelHelper.ExportIntoFile(HttpContext.Current.Server.MapPath(UKTemplatePath), models);
        }
    }
}