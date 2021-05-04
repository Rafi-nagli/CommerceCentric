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

namespace Amazon.Web.ViewModels.ExcelToAmazon
{
    public class ExcelProductCAViewModel
    {
        public static string CATemplatePath = "~/App_Data/Flat.File.Clothing.OneSheet.CA.xls";

        public long? StyleId { get; set; }
        public long? StyleItemId { get; set; }


        [ExcelSerializable("SKU", Order = 6, Width = 25)]
        public string SKU { get; set; }
        
        [ExcelSerializable("Product Name", Order = 7, Width = 65)]
        public string Title { get; set; }
        
        [ExcelSerializable("Product ID", Order = 9, Width = 25)]
        public string ASIN { get; set; }
        
        [ExcelSerializable("Product ID Type", Order = 8, Width = 25)]
        public string ProductId { get; set; }//Hardcode to word “ASIN”


        [ExcelSerializable("Clothing Type", Order = 1, Width = 25)]
        public string ClothingType { get; set; }


        [ExcelSerializable("Product Description", Order = 2, Width = 75)]
        public string Description { get; set; }//U can put same as parent’s description for all elements

        //[ExcelSerializable("Style Number", Order = 70, Width = 25)]
        //public string StyleNumber { get; set; }//todo: no use

        [ExcelSerializable("Update", Order = 0, Width = 25)]
        public string Update { get; set; }//Hardcode to word “update”


        [ExcelSerializable("Brand Name", Order = 10, Width = 25)]
        public string BrandName { get; set; }//Can be different for child elements

        [ExcelSerializable("Manufacturer", Order = 5, Width = 25)]
        public string Manufacturer { get; set; }


        //PART II (RED)
        [ExcelSerializable("Standard Price", Order = 11, Width = 25)]
        public string StandardPrice { get; set; }

        [ExcelSerializable("Sale Price", Order = 12, Width = 25)]
        public string SalePrice { get; set; }

        [ExcelSerializable("Quantity", Order = 16, Width = 25)]
        public string Quantity { get; set; }

        [ExcelSerializable("Suggested Price", Order = 25, Width = 25)]
        public string SuggestedPrice { get; set; }//Could be empty

        //[ExcelSerializable("Currency", Order = 27, Width = 25)]
        public string Currency { get; set; }//If Suggested Price provided hardcode to “USD”

        //PART III (BLUE)


        //PART IV (GREEN)



        
        //[ExcelSerializable("Item Type Keyword", Order = 5, Width = 25)]
        public string Type { get; set; }//Usually “pajama-sets” for pajamas or “nightgown” for gowns. 
        
        


        [ExcelSerializable("Recommended Browse Nodes1", Order = 37, Width = 25)]
        public string RecommendedBrowseNodes1 { get; set; }


        //PART III (ORANGE)
        [ExcelSerializable("SearchTerms", Order = 38, Width = 25)]
        public string SearchTerms1 { get; set; }

        //[ExcelSerializable("SearchTerms2", Order = 40, Width = 25)]
        //public string SearchTerms2 { get; set; }

        //[ExcelSerializable("SearchTerms3", Order = 41, Width = 25)]
        //public string SearchTerms3 { get; set; }

        //[ExcelSerializable("SearchTerms4", Order = 42, Width = 25)]
        //public string SearchTerms4 { get; set; }

        //[ExcelSerializable("SearchTerms5", Order = 43, Width = 25)]
        //public string SearchTerms5 { get; set; }


        [ExcelSerializable("KeyProductFeatures1", Order = 39, Width = 25)]
        public string KeyProductFeatures1 { get; set; }

        [ExcelSerializable("KeyProductFeatures2", Order = 40, Width = 25)]
        public string KeyProductFeatures2 { get; set; }

        [ExcelSerializable("KeyProductFeatures3", Order = 41, Width = 25)]
        public string KeyProductFeatures3 { get; set; }

        [ExcelSerializable("KeyProductFeatures4", Order = 42, Width = 25)]
        public string KeyProductFeatures4 { get; set; }

        [ExcelSerializable("KeyProductFeatures5", Order = 43, Width = 25)]
        public string KeyProductFeatures5 { get; set; }


        //PART IV (YELLOW)
        [ExcelSerializable("OtherImageUrl1", Order = 45, Width = 25)]
        public string OtherImageUrl1 { get; set; }

        [ExcelSerializable("OtherImageUrl2", Order = 46, Width = 25)]
        public string OtherImageUrl2 { get; set; }

        [ExcelSerializable("OtherImageUrl3", Order = 47, Width = 25)]
        public string OtherImageUrl3 { get; set; }


        [ExcelSerializable("Main Image URL", Order = 48, Width = 57)]
        public string MainImageURL { get; set; }



        //PART (BROWN)

        [ExcelSerializable("Color", Order = 66, Width = 25)]
        public string Color { get; set; } //Can be different for child elements

        [ExcelSerializable("Department", Order = 68, Width = 25)]
        public string Department { get; set; } //Usually: girls, boys, baby-boys, baby-girls

        [ExcelSerializable("Size", Order = 81, Width = 25)]
        public string Size { get; set; } //DQ4 - empty

        [ExcelSerializable("SpecialSize", Order = 83, Width = 25)]
        public string SpecialSize { get; set; } 

        //PART II






        //[ExcelSerializable("Product Tax Code", Order = 120, Width = 25)]
        //public string ProductTaxCode { get; set; }//todo: no use
        //[ExcelSerializable("Fulfillment Latency", Order = 130, Width = 25)]
        //public string FulfillmentLatency { get; set; }//todo: no use
        //[ExcelSerializable("Launch Date", Order = 140, Width = 25)]
        //public string LaunchDate { get; set; }//todo: no use
        //[ExcelSerializable("Offering Release Date", Order = 150, Width = 25)]
        //public string OfferingReleaseDate { get; set; }//todo: no use
        //[ExcelSerializable("Restock Date", Order = 160, Width = 25)]
        //public string RestockDate { get; set; }//todo: no use




        //PART IV (GREEN)
        

        




        

        //PART (PURPURE)
        [ExcelSerializable("Parentage", Order = 59, Width = 25)]
        public string Parentage { get; set; } //Hardcode to 4th row – “parent”, all other rows “Child” 
        
        [ExcelSerializable("Parent SKU", Order = 58, Width = 35)]
        public string ParentSKU { get; set; } //BS4 –empty, others copy A4 

        [ExcelSerializable("Relationship Type", Order = 57, Width = 25)]
        public string RelationshipType { get; set; } //“Variation”, BT4 – empty

        [ExcelSerializable("Variation Theme", Order = 56, Width = 25)]
        public string VariationTheme { get; set; } //Hardcode to “Size”

        
        public static string GetRecommendedBrowseNodes1(string gender, string size, string type)
        {
            type = type ?? "";
            gender = gender ?? "";
            size = size ?? "";

            if (size.Contains("12M") || size.Contains("18M") || size.Contains("24M")
                || size.Contains("2T") || size.Contains("3T") || size.Contains("4T") || size.Contains("5T"))
            {
                if (gender.Contains("boys")) //boys/baby-boys
                {
                    if (type.Contains("pajama"))
                        return "10286889011";
                }

                if (gender.Contains("girl"))
                {
                    if (type.Contains("pajama"))
                        return "10286979011";
                    if (type.Contains("nightgowns"))
                        return "10286977011";
                }
            }
            else
            //if (SizeHelper.IsDigitSize(size))
            {
                if (gender.Contains("girls"))
                {
                    if (type.Contains("pajama"))
                        return "10287175011";
                    if (type.Contains("nightgowns"))
                        return "10287173011";
                }

                if (gender.Contains("boys"))
                {
                    if (type.Contains("pajama"))
                        return "10287070011";
                }
            }

            return "";

            /*
             *             if (size.Contains("12M") || size.Contains("18M") || size.Contains("24M")
                || size.Contains("2T") || size.Contains("3T") || size.Contains("4T") || size.Contains("5T"))
            {
                if (gender.Contains("boys")) //boys/baby-boys
                {
                    if (type == "pajama")
                        return "10286893011";
                    if (type == "robes")
                        return "10286891011";
                    if (type == "footed pajama")
                        return "10286887011";
                }

                if (gender.Contains("girl"))
                {
                    if (type == "pajama")
                        return "10286979011";
                    if (type == "robes")
                        return "10286984011";
                    if (type == "footed pajama")
                        return "10286976011";
                    if (type == "nightgowns")
                        return "10286977011";
                }
            }

            if (SizeHelper.IsDigitSize(size))
            {
                if (gender == "girls")
                {
                    if (type == "pajama")
                        return "10287175011";
                    if (type == "robes")
                        return "10287177011";
                    if (type == "nightgowns")
                        return "10287173011";
                }

                if (gender == "boys")
                {
                    if (type == "pajama")
                        return "10287070011";
                    if (type == "robes")
                        return "10287068011";
                }
            }
            return "";*/
        }


        public static MemoryStream ExportToExcelCA(ILogService log,
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

            return ExcelHelper.ExportIntoFile(HttpContext.Current.Server.MapPath(CATemplatePath),
                "Template",
                models);
        }

        private static decimal CorrectCAPrice(decimal price)
        {
            if (price == 20.99M)
                return 19.99M;
            return price;
        }

        public static IList<ExcelProductCAViewModel> GetItemsFor(ILogService log,
            ITime time,
            IMarketCategoryService categoryService,
            IHtmlScraperService htmlScraper,
            IMarketApi marketApi,
            IUnitOfWork db,
            CompanyDTO company,
            string parentAsin,
            MarketType market,
            string marketplaceId,
            UseStyleImageModes useStyleImageMode,
            out string filename)
        {
            var caExchangeRate = PriceHelper.CADtoUSD;
            var today = time.GetAppNowTime();

            var models = new List<ExcelProductCAViewModel>();
            var items = ItemExportHelper.GetParentItemAndChilds(log,
                time,
                htmlScraper,
                marketApi,
                db,
                company,
                parentAsin,
                null,
                ExportToExcelMode.Normal, 
                market,
                marketplaceId);

            var parent = items.Item1;// db.ParentItems.GetAsDTO(asin);
            var children = items.Item2;// db.Items.GetAllActualExAsDto().Where(i => i.ParentASIN == asin).ToList();
            var firstChild = children.FirstOrDefault();

            FeatureValueDTO subLicense = null;
            string itemStyle = null;
            if (firstChild != null && firstChild.StyleId.HasValue)
            {
                //style = db.Styles.Get(parent.StyleId.Value);
                subLicense = db.FeatureValues.GetValueByStyleAndFeatureId(firstChild.StyleId.Value, StyleFeatureHelper.SUB_LICENSE1);
                itemStyle = ItemExportHelper.GetFeatureValue(db.FeatureValues.GetValueByStyleAndFeatureId(firstChild.StyleId.Value, StyleFeatureHelper.ITEMSTYLE));
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

                var gender = (childDepartment ?? parent.Department ?? "");
                var sizeType = ItemExportHelper.GetSizeType(childSize);
                var categoryInfo = categoryService.GetCategory(market, marketplaceId, itemStyle, gender, sizeType);
                //var itemType = ItemExportHelper.GetItemType(itemStyle);                
                var newItemType = categoryInfo.Key1;// ItemExportHelper.ItemTypeConverter(childSize ?? "", itemType, itemStyle, gender);
                var newDepartment = StringHelper.GetFirstNotEmpty(categoryInfo.Key2, ItemExportHelper.DepartmentConverter(gender, newItemType, sizeType));
                var parentImage = ItemExportHelper.ImageConverter(StringHelper.GetFirstNotEmpty(
                    parent.LargeImage != null ? parent.LargeImage.Image : null,
                    parent.ImageSource));

                models.Add(new ExcelProductCAViewModel
                {
                    SKU = parentSku,// parent.StyleString,
                    Title = parent.AmazonName,
                    ASIN = parent.ASIN,
                    ProductId = "ASIN",
                    ClothingType = "sleepwear",
                    Manufacturer = parent.BrandName ?? childBrandName,
                    BrandName = parent.BrandName ?? childBrandName,
                    Description = parent.Description,
                    Type = newItemType,
                    Update = "Update",
                    StandardPrice = "",
                    
                    SuggestedPrice = childListPrice.HasValue ? Math.Round(PriceHelper.Convert(childListPrice.Value / 100, caExchangeRate, true)).ToString("G") : "",
                    Currency = childListPrice.HasValue ? "CAD" : "",

                    Quantity = "",

                    KeyProductFeatures1 = childFeatures.Count > 0 ? childFeatures[0] : String.Empty,
                    KeyProductFeatures2 = childFeatures.Count > 1 ? childFeatures[1] : String.Empty,
                    KeyProductFeatures3 = childFeatures.Count > 2 ? childFeatures[2] : String.Empty,
                    KeyProductFeatures4 = childFeatures.Count > 3 ? childFeatures[3] : String.Empty,
                    KeyProductFeatures5 = childFeatures.Count > 4 ? childFeatures[4] : String.Empty,

                    RecommendedBrowseNodes1 = ExcelProductCAViewModel.GetRecommendedBrowseNodes1(newDepartment, childSize, newItemType),

                    MainImageURL = parentImage,
                    Parentage = "Parent",
                    VariationTheme = variationType,

                    Department = newDepartment,
                    Color = "",
                });

                foreach (var child in children)
                {
                    var styleImages = child.StyleId.HasValue ?
                        db.StyleImages
                        .GetAllAsDto()
                        .Where(im => im.StyleId == child.StyleId.Value
                            && im.Type != (int)StyleImageType.Swatch)
                        .ToList()
                        .OrderByDescending(im => ImageHelper.GetSortIndex(im.Category))
                        .ThenByDescending(im => im.IsDefault)
                        .ThenBy(im => im.Id)
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
                    models.Add(new ExcelProductCAViewModel
                    {
                        StyleId = child.StyleId,
                        StyleItemId = child.StyleItemId,

                        SKU = child.SKU,
                        Title = child.Name,
                        ASIN = child.ASIN,
                        ProductId = "ASIN",
                        ClothingType = "sleepwear",
                        Manufacturer = child.BrandName,
                        BrandName = child.BrandName,
                        Description = parent.Description,
                        Type = newItemType,
                        Update = "Update",
                        //StandardPrice = child.Price.ToString("G"),
                        StandardPrice = CorrectCAPrice(PriceHelper.Convert(itemPrice, caExchangeRate, true)).ToString("G"),
                        //SalePrice = child.SalePrice.HasValue && child.SaleStartDate < today ? CorrectCAPrice(PriceHelper.Convert(child.SalePrice.Value, caExchangeRate, true)).ToString("G") : null,
                        SuggestedPrice = child.ListPrice.HasValue ? Math.Round(PriceHelper.Convert(child.ListPrice.Value / 100, caExchangeRate, true)).ToString("G") : "",
                        Currency = child.ListPrice.HasValue ? "CAD" : "",
                        Quantity = child.RealQuantity.ToString("G"),// "1", // i.Quantity.ToString("G"),

                        KeyProductFeatures1 = features.Count > 0 ? features[0] : String.Empty,
                        KeyProductFeatures2 = features.Count > 1 ? features[1] : String.Empty,
                        KeyProductFeatures3 = features.Count > 2 ? features[2] : String.Empty,
                        KeyProductFeatures4 = features.Count > 3 ? features[3] : String.Empty,
                        KeyProductFeatures5 = features.Count > 4 ? features[4] : String.Empty,

                        RecommendedBrowseNodes1 = ExcelProductCAViewModel.GetRecommendedBrowseNodes1(newDepartment, child.Size, newItemType),

                        MainImageURL = childImage,
                        Parentage = "Child",
                        ParentSKU = parentSku,
                        RelationshipType = "Variation",
                        VariationTheme = variationType,
                        Department = newDepartment,
                        Color = child.Color,
                        Size = child.Size,
                        SpecialSize = child.SpecialSize,
                    });
                }

                //Copy to parent child Type
                if (children.Count > 0)
                    models[0].Type = models[1].Type;

                if (children.Count > 0
                    && useStyleImageMode == UseStyleImageModes.StyleImage)
                    models[0].MainImageURL = models[1].MainImageURL;
            }

            filename = models[0].SKU + "_" + (subLicense != null ? subLicense.Value : "none") + "_CA.xls";

            return models;
        }
    }
}