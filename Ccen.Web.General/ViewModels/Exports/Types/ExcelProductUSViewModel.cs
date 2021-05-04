using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Amazon.Api;
using Amazon.Api.Exports;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Exports.Attributes;
using Amazon.Core.Models;
using Amazon.Core.Models.Items;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Users;
using Amazon.Web.Models.Exports;
using Amazon.Web.ViewModels.Inventory;

namespace Amazon.Web.ViewModels.ExcelToAmazon
{
    public class ExcelProductUSViewModel : IExcelProductViewModel
    {
        public static string USTemplatePath = "~/App_Data/Flat.File.Clothing-full.OneSheet.US.xls";

        public int? Id { get; set; }
        public long? StyleId { get; set; }
        public long? StyleItemId { get; set; }
        public bool IsExistOnAmazon { get; set; }


        [ExcelSerializable("SKU", Order = 1, Width = 25)]
        public string SKU { get; set; }

        [ExcelSerializable("Product Name", Order = 3, Width = 65)]
        public string Title { get; set; }

        [ExcelSerializable("Product ID", Order = 4, Width = 25)]
        public string ASIN { get; set; }

        [ExcelSerializable("Product ID Type", Order = 5, Width = 25)]
        public string ProductId { get; set; }//Hardcode to word “ASIN”

        [ExcelSerializable("Brand Name", Order = 2, Width = 25)]
        public string BrandName { get; set; }//Can be different for child elements
        [ExcelSerializable("Product Description", Order = 29, Width = 75)]
        public string Description { get; set; }//U can put same as parent’s description for all elements

        [ExcelSerializable("Item Type Keyword", Order = 8, Width = 25)]
        public string Type { get; set; }//Usually “pajama-sets” for pajamas or “nightgown” for gowns. 

        //[ExcelSerializable("Style Number", Order = 7, Width = 25)]
        //public string StyleNumber { get; set; }//todo: no use

        [ExcelSerializable("Update", Order = 28, Width = 25)]
        public string Update { get; set; }//Hardcode to word “update”


        //PART II
        [ExcelSerializable("Standard Price", Order = 12, Width = 25)]
        public string StandardPrice { get; set; }

        [ExcelSerializable("Suggested Price", Order = 187, Width = 25)]
        public string SuggestedPrice { get; set; }//Could be empty

        //[ExcelSerializable("Currency", Order = 11, Width = 25)]
        public string Currency { get; set; }//If Suggested Price provided hardcode to “USD”

        [ExcelSerializable("Quantity", Order = 13, Width = 25)]
        public string Quantity { get; set; }



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




        //GREEN
        [ExcelSerializable("KeyProductFeatures1", Order = 34, Width = 25)]
        public string KeyProductFeatures1 { get; set; }

        [ExcelSerializable("KeyProductFeatures2", Order = 35, Width = 25)]
        public string KeyProductFeatures2 { get; set; }

        [ExcelSerializable("KeyProductFeatures3", Order = 36, Width = 25)]
        public string KeyProductFeatures3 { get; set; }

        [ExcelSerializable("KeyProductFeatures4", Order = 37, Width = 25)]
        public string KeyProductFeatures4 { get; set; }

        [ExcelSerializable("KeyProductFeatures5", Order = 38, Width = 25)]
        public string KeyProductFeatures5 { get; set; }

        [ExcelSerializable("SearchTerms", Order = 43, Width = 25)]
        public string SearchTerms1 { get; set; }

        //[ExcelSerializable("SearchTerms2", Order = 43, Width = 25)]
        //public string SearchTerms2 { get; set; }

        //[ExcelSerializable("SearchTerms3", Order = 44, Width = 25)]
        //public string SearchTerms3 { get; set; }

        //[ExcelSerializable("SearchTerms4", Order = 45, Width = 25)]
        //public string SearchTerms4 { get; set; }

        //[ExcelSerializable("SearchTerms5", Order = 46, Width = 25)]
        //public string SearchTerms5 { get; set; }


        [ExcelSerializable("Main Image URL", Order = 14, Width = 57)]
        public string MainImageURL { get; set; }
        [ExcelSerializable("OtherImageUrl1", Order = 15, Width = 25)]
        public string OtherImageUrl1 { get; set; }

        [ExcelSerializable("OtherImageUrl2", Order = 16, Width = 25)]
        public string OtherImageUrl2 { get; set; }

        [ExcelSerializable("OtherImageUrl3", Order = 17, Width = 25)]
        public string OtherImageUrl3 { get; set; }

        [ExcelSerializable("SwatchImageUrl", Order = 18, Width = 25)]
        public string SwatchImageUrl { get; set; }

        //PINK
        [ExcelSerializable("Parentage", Order = 24, Width = 25)]
        public string Parentage { get; set; } //Hardcode to 4th row – “parent”, all other rows “Child” 

        [ExcelSerializable("Parent SKU", Order = 25, Width = 35)]
        public string ParentSKU { get; set; } //BS4 –empty, others copy A4 

        [ExcelSerializable("Relationship Type", Order = 26, Width = 25)]
        public string RelationshipType { get; set; } //“Variation”, BT4 – empty

        [ExcelSerializable("Variation Theme", Order = 27, Width = 25)]
        public string VariationTheme { get; set; } //Hardcode to “Size”


        //BROWN

        [ExcelSerializable("Color", Order = 52, Width = 25)]
        public string Color { get; set; } //Can be different for child elements

        [ExcelSerializable("Department", Order = 9, Width = 25)]
        public string Department { get; set; } //Usually: girls, boys, baby-boys, baby-girls

        [ExcelSerializable("Size", Order = 10, Width = 25)]
        public string Size { get; set; } //DQ4 - empty

        //[ExcelSerializable("SpecialSize", Order = 114, Width = 25)]
        //public string SpecialSize { get; set; } //DQ4 - empty


        [ExcelSerializable("fulfillment_center_id", Order = 123, Width = 25)]
        public string FulfillmentCenterID { get; set; }

        [ExcelSerializable("package_height", Order = 124, Width = 25)]
        public string PackageHeight { get; set; }

        [ExcelSerializable("package_width", Order = 125, Width = 25)]
        public string PackageWidth { get; set; }

        [ExcelSerializable("package_length", Order = 126, Width = 25)]
        public string PackageLength { get; set; }

        [ExcelSerializable("package_length_unit_of_measure", Order = 127, Width = 25)]
        public string PackageLengthUnitOfMeasure { get; set; }

        [ExcelSerializable("package_weight", Order = 128, Width = 25)]
        public string PackageWeight { get; set; }
        [ExcelSerializable("package_weight_unit_of_measure", Order = 129, Width = 25)]
        public string PackageWeightUnitOfMeasure { get; set; }

        [ExcelSerializable("supplier_declared_dg_hz_regulation1", Order = 159, Width = 25)]
        public string SupplierDeclaredDgHzRegulation1 { get; set; }

        [ExcelSerializable("batteries_required", Order = 139, Width = 25)]
        public string BatteriesRequired { get; set; }

        [ExcelSerializable("feed_product_type ", Order = 0, Width = 25)]
        public string FeedProductType { get; set; }

        [ExcelSerializable("merchant_shipping_group_name", Order = 207)]
        public string MerchantShippingGroupName { get; set; }

        [ExcelSerializable("condition_type", Order = 197)]
        public string ConditionType { get; set; }

        public static MemoryStream ExportToExcelUS(ILogService log,
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
                ExportToExcelMode.Normal, 
                null, 
                market, 
                marketplaceId,
                useStyleImage ? UseStyleImageModes.StyleImage : UseStyleImageModes.ListingImage,
                out filename);

            return ExcelHelper.ExportIntoFile(HttpContext.Current.Server.MapPath(USTemplatePath), 
                "Template",
                models);
        }

        public static IList<ExcelProductUSViewModel> GetItemsFor(ILogService log,
            ITime time,
            IMarketCategoryService categoryService,
            IHtmlScraperService htmlScraper,
            IMarketApi marketApi,
            IUnitOfWork db,
            CompanyDTO company,
            string asin,
            ExportToExcelMode exportMode,
            IList<FBAItemInfo> fbaItems,
            MarketType market,
            string marketplaceId,
            UseStyleImageModes useStyleImageMode,
            out string filename)
        {
            filename = null;

            var items = ItemExportHelper.GetParentItemAndChilds(log,
                time,
                htmlScraper,
                marketApi,
                db,
                company,
                asin,
                fbaItems,
                exportMode,
                market,
                marketplaceId);

            var parent = items.Item1;// db.ParentItems.GetAsDTO(asin);
            var children = items.Item2;// db.Items.GetAllActualExAsDto().Where(i => i.ParentASIN == asin).ToList();

            var models = GetItemsFor(db, categoryService, exportMode, fbaItems, parent, children, useStyleImageMode, out filename);

            return models;
        }

        public static IList<ExcelProductUSViewModel> GetItemsFor(IUnitOfWork db,
            IMarketCategoryService categoryService,
            ExportToExcelMode exportMode,
            IList<FBAItemInfo> fbaItems,
            ParentItemDTO parent,
            IList<ItemExDTO> children,
            UseStyleImageModes useStyleImageMode,
            out string filename)
        {
            var models = new List<ExcelProductUSViewModel>();
            filename = null;

            var defaultChild = children.OrderByDescending(ch => ch.IsExistOnAmazon).ThenBy(ch => ch.Id).FirstOrDefault();

            if (!children.Any() && exportMode != ExportToExcelMode.Normal)
                return models;

            FeatureValueDTO defaultChildSubLicense = null;
            var defaultChildItemStyle = String.Empty;
            var defaultChildGender = String.Empty;
            var defaultChildBrand = String.Empty;
            StyleEntireDto firstChildStyle = null;
            string mainBrandName = null;
            decimal? childListPrice = null;
            string childDepartment = null;
            string childSize = null;
            string childStyleString = null;
            IList<string> defaultChildFeatures = new List<string>();

            if (defaultChild != null && defaultChild.StyleId.HasValue)
            {
                defaultChildSubLicense = db.FeatureValues.GetValueByStyleAndFeatureId(defaultChild.StyleId.Value, StyleFeatureHelper.SUB_LICENSE1);
                defaultChildItemStyle = ItemExportHelper.GetFeatureValue(db.FeatureValues.GetValueByStyleAndFeatureId(defaultChild.StyleId.Value, StyleFeatureHelper.ITEMSTYLE));
                defaultChildGender = ItemExportHelper.GetFeatureValue(db.FeatureValues.GetValueByStyleAndFeatureId(defaultChild.StyleId.Value, StyleFeatureHelper.GENDER));
                defaultChildBrand = ItemExportHelper.GetFeatureValue(db.FeatureValues.GetValueByStyleAndFeatureId(defaultChild.StyleId.Value, StyleFeatureHelper.BRAND));

                mainBrandName = StringHelper.GetFirstNotEmpty(parent.BrandName, defaultChild.BrandName, defaultChildBrand);

                defaultChildFeatures = !String.IsNullOrEmpty(defaultChild.Features)
                    ? defaultChild.Features.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList()
                    : new List<string>();
                childListPrice = defaultChild.ListPrice;
                childDepartment = defaultChild.Department;
                childSize = defaultChild.Size;
                childStyleString = defaultChild.StyleString;

                if (defaultChild.StyleId.HasValue)
                    firstChildStyle = db.Styles.GetByStyleIdAsDto(defaultChild.StyleId.Value);
            }

            if (parent != null)
            {
                var variationType = ItemExportHelper.GetVariationType(children != null ? children.Select(ch => ch.Color).ToList() : null);

                //For simplicity, w can just use for parent SKU (unless it exist) style-%parentASIN%.
                string parentSku = parent.SKU;
                if (String.IsNullOrEmpty(parentSku))
                    //NOTE: removed ASIN part (I guess it could be used for preventing listings mix)
                    parentSku = childStyleString;// + "-" + parent.ASIN;

                var mainGender = StringHelper.GetFirstNotEmpty(childDepartment, parent.Department, defaultChildGender);
                var sizeType = ItemExportHelper.GetSizeType(childSize);
                var categoryInfo = categoryService.GetCategory((MarketType)parent.Market, parent.MarketplaceId, defaultChildItemStyle, mainGender, sizeType);

                //var type = ItemExportHelper.GetItemType(itemStyle);

                var mainItemType = categoryInfo.Key1;// ItemExportHelper.ItemTypeConverter(childSize ?? "", type, itemStyle, gender);
                var mainDepartment = StringHelper.GetFirstNotEmpty(categoryInfo.Key2, ItemExportHelper.DepartmentConverter(mainGender, mainItemType, sizeType));
                var parentImage = ItemExportHelper.ImageConverter(StringHelper.GetFirstNotEmpty(
                    parent.LargeImage != null ? parent.LargeImage.Image : null,
                    parent.ImageSource));
                string clothingType = categoryInfo.Key3;

                var searchTerms = (parent.SearchKeywords ?? "").Replace(";", ", ");
                searchTerms = ItemExportHelper.PrepareSearchTerms(searchTerms);

                var mainMsrp = firstChildStyle != null && firstChildStyle.MSRP.HasValue
                    ? firstChildStyle.MSRP.Value.ToString("G")
                    : (childListPrice.HasValue ? Math.Round(childListPrice.Value/100).ToString("G") : "");

                var mainDescription = firstChildStyle?.Description ?? parent.Description;

                models.Add(new ExcelProductUSViewModel
                {
                    SKU = parentSku,
                    Title = parent.AmazonName,
                    ASIN = parent.ASIN,
                    ProductId = "ASIN",
                    BrandName = mainBrandName,
                    Description = mainDescription,
                    Type = mainItemType,
                    Update = "Update",
                    StandardPrice = "",

                    SuggestedPrice = mainMsrp,
                    Currency = childListPrice.HasValue ? "USD" : "",

                    Quantity = "",

                    KeyProductFeatures1 = defaultChildFeatures.Count > 0 ? defaultChildFeatures[0] : String.Empty,
                    KeyProductFeatures2 = defaultChildFeatures.Count > 1 ? defaultChildFeatures[1] : String.Empty,
                    KeyProductFeatures3 = defaultChildFeatures.Count > 2 ? defaultChildFeatures[2] : String.Empty,
                    KeyProductFeatures4 = defaultChildFeatures.Count > 3 ? defaultChildFeatures[3] : String.Empty,
                    KeyProductFeatures5 = defaultChildFeatures.Count > 4 ? defaultChildFeatures[4] : String.Empty,

                    SearchTerms1 = searchTerms,
                    //SearchTerms1 = searchTermList.Count > 0 ? searchTermList[0] : String.Empty,
                    //SearchTerms2 = searchTermList.Count > 1 ? searchTermList[1] : String.Empty,
                    //SearchTerms3 = searchTermList.Count > 2 ? searchTermList[2] : String.Empty,
                    //SearchTerms4 = searchTermList.Count > 3 ? searchTermList[3] : String.Empty,
                    //SearchTerms5 = searchTermList.Count > 4 ? searchTermList[4] : String.Empty,

                    MainImageURL = parentImage,
                    Parentage = ExcelHelper.ParentageParent,
                    VariationTheme = variationType,

                    Department = mainDepartment,
                    Color = "",
                    FeedProductType = clothingType,
                });

                foreach (var child in children)
                {
                    var childStyleImages = child.StyleId.HasValue ?
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
                    var childStyleGenderValue = child.StyleId.HasValue ? db.StyleFeatureValues.GetAllWithFeature()
                        .FirstOrDefault(st => st.StyleId == child.StyleId.Value && st.FeatureId == StyleFeatureHelper.GENDER)?.Value : null;
                    var childStyle = child.StyleId.HasValue ? db.Styles.GetByStyleIdAsDto(child.StyleId.Value) : null;

                    var msrp = childStyle != null && childStyle.MSRP.HasValue
                        ? childStyle.MSRP.Value.ToString("G")
                        : (child.ListPrice.HasValue ? Math.Round(child.ListPrice.Value / 100).ToString("G") : "");

                    var description = childStyle?.Description ?? parent.Description;
                    
                    var fbaInfo = fbaItems != null ? fbaItems.FirstOrDefault(f => f.SKU == child.SKU) : null;
                    if (exportMode == ExportToExcelMode.FBA)
                    {
                        if (fbaInfo == null)
                            throw new ArgumentNullException("fbaInfo", "for SKU=" + child.SKU);
                    }

                    IList<string> features = !String.IsNullOrEmpty(child.Features)
                        ? child.Features.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList()
                        : new List<string>();
                    if (!String.IsNullOrEmpty(childStyle.BulletPoint1))
                        features = StringHelper.ToArray(childStyle.BulletPoint1,
                            childStyle.BulletPoint2,
                            childStyle.BulletPoint3,
                            childStyle.BulletPoint4,
                            childStyle.BulletPoint5);

                    var childImage = ItemExportHelper.ImageConverter(StringHelper.GetFirstNotEmpty(
                        child.LargeImageUrl,
                        child.ImageUrl));
                    var childImage1 = "";
                    var childImage2 = "";
                    var childImage3 = "";

                    if (useStyleImageMode == UseStyleImageModes.Auto)
                    {
                        if (String.IsNullOrEmpty(childImage))
                        {
                            if (childStyleImages != null && childStyleImages.Any())
                                childImage = childStyleImages.First().Image;
                        }
                        else
                        {
                            if (childStyleImages != null && childStyleImages.Any(im => im.Type == (int) StyleImageType.HiRes))
                                childImage = childStyleImages.First(im => im.Type == (int) StyleImageType.HiRes).Image;
                        }
                    }
                    if (useStyleImageMode == UseStyleImageModes.StyleImage
                        || (useStyleImageMode == UseStyleImageModes.ListingImage && String.IsNullOrEmpty(childImage)))
                    {
                        if (childStyleImages != null && childStyleImages.Any())
                        {
                            childImage = childStyleImages.First().Image;
                            childImage1 = childStyleImages.Count > 1 ? childStyleImages[1].Image : "";
                            childImage2 = childStyleImages.Count > 2 ? childStyleImages[2].Image : "";
                            childImage3 = childStyleImages.Count > 3 ? childStyleImages[3].Image : "";
                        }

                    }
                    if (useStyleImageMode == UseStyleImageModes.ListingImage)
                    {
                        //Nothing
                    }

                    var hasAmazonUpdates = child.IsExistOnAmazon == true;
                    var displaySize = String.IsNullOrEmpty(child.Size) ? "" : ("Size " + child.Size);
                    models.Add(new ExcelProductUSViewModel
                    {
                        Id = child.Id,
                        StyleId = child.StyleId,
                        StyleItemId = child.StyleItemId,
                        IsExistOnAmazon = hasAmazonUpdates,

                        SKU = ItemExportHelper.PrepareSKU(child.SKU, exportMode),
                        Title = hasAmazonUpdates ? child.Name : StringHelper.Join(", ", child.Name, child.Color, displaySize),
                        ASIN = hasAmazonUpdates ? child.ASIN : child.Barcode,
                        ProductId = hasAmazonUpdates ? "ASIN" : "UPC",
                        BrandName = mainBrandName,
                        Description = description,
                        Type = mainItemType,
                        Update = hasAmazonUpdates ? "PartialUpdate" : "Update",

                        StandardPrice = (child.CurrentPrice + (exportMode == ExportToExcelMode.FBA || exportMode == ExportToExcelMode.FBP ? 5 : 0)).ToString("G"),
                        SuggestedPrice = msrp,
                        Currency = child.ListPrice.HasValue ? "USD" : "",
                        Quantity = exportMode == ExportToExcelMode.FBA ? fbaInfo.Quantity.ToString("G") : child.RealQuantity.ToString("G"),

                        KeyProductFeatures1 = features.Count > 0 ? features[0] : String.Empty,
                        KeyProductFeatures2 = features.Count > 1 ? features[1] : String.Empty,
                        KeyProductFeatures3 = features.Count > 2 ? features[2] : String.Empty,
                        KeyProductFeatures4 = features.Count > 3 ? features[3] : String.Empty,
                        KeyProductFeatures5 = features.Count > 4 ? features[4] : String.Empty,

                        SearchTerms1 = searchTerms,
                        //SearchTerms1 = searchTermList.Count > 0 ? searchTermList[0] : String.Empty,
                        //SearchTerms2 = searchTermList.Count > 1 ? searchTermList[1] : String.Empty,
                        //SearchTerms3 = searchTermList.Count > 2 ? searchTermList[2] : String.Empty,
                        //SearchTerms4 = searchTermList.Count > 3 ? searchTermList[3] : String.Empty,
                        //SearchTerms5 = searchTermList.Count > 4 ? searchTermList[4] : String.Empty,

                        MainImageURL = childImage,
                        OtherImageUrl1 = childImage1,
                        OtherImageUrl2 = childImage2,
                        OtherImageUrl3 = childImage3,
                        Parentage = ExcelHelper.ParentageChild,
                        ParentSKU = parentSku,
                        RelationshipType = "Variation",
                        VariationTheme = variationType,
                        Department = mainDepartment,
                        Color = child.Color,
                        Size = child.Size,
                        //SpecialSize = child.SpecialSize,

                        FulfillmentCenterID = exportMode == ExportToExcelMode.FBA ? "AMAZON_NA" : null,
                        PackageHeight = exportMode == ExportToExcelMode.FBA && fbaInfo.PackageHeight.HasValue ? fbaInfo.PackageHeight.ToString() : null,
                        PackageWidth = exportMode == ExportToExcelMode.FBA && fbaInfo.PackageWidth.HasValue ? fbaInfo.PackageWidth.ToString() : null,
                        PackageLength = exportMode == ExportToExcelMode.FBA && fbaInfo.PackageLength.HasValue ? fbaInfo.PackageLength.ToString() : null,
                        PackageLengthUnitOfMeasure = exportMode == ExportToExcelMode.FBA ? "IN" : null,
                        PackageWeight = exportMode == ExportToExcelMode.FBA ? child.Weight.ToString() : null,
                        PackageWeightUnitOfMeasure = exportMode == ExportToExcelMode.FBA ? "OZ" : null,

                        SupplierDeclaredDgHzRegulation1 = "Not Applicable",
                        BatteriesRequired = "FALSE",
                        FeedProductType = clothingType,

                        ConditionType = "New",
                        MerchantShippingGroupName = child.IsPrime ? AmazonTemplateHelper.PrimeTemplate : (child.Weight > 16 ? AmazonTemplateHelper.OversizeTemplate : null),
                    });
                }

                //Copy to parent child Type
                if (children.Count > 0)
                    models[0].Type = models[1].Type;

                if (children.Count > 0
                    && useStyleImageMode == UseStyleImageModes.StyleImage)
                    models[0].MainImageURL = models[1].MainImageURL;
            }

            filename = models[0].SKU + "_" + (defaultChildSubLicense != null ? defaultChildSubLicense.Value : "none") + "_US" + Path.GetExtension(USTemplatePath);

            return models;
        }



        public static IList<ExcelProductUSViewModel> GenerateToExcelUS(IUnitOfWork db,
            IBarcodeService barcodeService,
            IMarketCategoryService categoryService,
            StyleViewModel model,
            DateTime when,
            out string filename)
        {
            /*
                1.	Item_sku:
                    a.	First row = Style
                    b.	Other rows – Style-%size%, i.e. K123123-2T, K123123-3T..
                2.	Item name:
                    a.	First Raw - %Name% + “,”+%Size Group% (Infant, Toddler, Kids)+”Sizes”+%Size_Range%” (example: Planes Boys 'Turn Up the Heat' Coat Style Pajama Set, Toddler Sizes 2T-4T)
                    b.	Other rows: %Name% + “,”+%Size Group% (Infant, Toddler, Kids)+%Size% (i.e. Planes Boys 'Turn Up the Heat' Coat Style Pajama Set, Toddler Size 2T)
                3.	external_product_id – barcode (empty for first raw)
                4.	external_product_id_type – harcoded “UPC” (empty for first raw)
                16.	brand_name - Main License
                17.	item_type – use same logic as you use when you generate xls from ASIN
                18.	main_image_url – upload image to server, and insert public URL to this image here
                19.	Color - %Color%
                20.	Department_name - use same logic as you use when you generate xls from ASIN
                21.	Size - %size%
                22.	update_delete – “Update”
                23.	standard_price - %price%
                24.	list_price -- %MSRP%
                25.	currency – “USD”
                26.	quantity – “%Quantity%
                27.	bullet_point1 – “Authentic ”+ %Main License% +” product with reliable quality and durability
                28.	bullet_point2 – “Featuring “+%sublicense%
                29.	bullet_point3 – if(Material  != Cotton) “Flame resistant” else “100% Cotton”
                30.	bullet_point4 – “Machine Wash, Easy Care”
                31.	generic_keywords1 = if(Item style = pajama) “ sleepwear, pj, jummie, new, %sleeve%”
                    if(nightgown) “night, gown, night-gown, sleepwear, pj, jummie, new, %sleeve%, dress up”
                32.	“Gift, present, 2015, cozy ” If(Material =fleece) “Fleece, microfleece, warm, winter spring”
                33.	parent_child, parent_sku, relationship_type, variation_theme – same logic as you use when you generate xls from ASIN
                34.	if More then one picture provided insert their URL into other_image_url1, other_image_url2, other_image_url3
            */

            var models = new List<ExcelProductUSViewModel>();

            var parent = new ExcelProductUSViewModel();
            var childs = new List<ExcelProductUSViewModel>();

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
            //var shippingSize = ItemExportHelper.GetFeatureValue(features, allFeatureValues, Feature.SHIPPING_SIZE);

            //var itemType = ItemExportHelper.GetItemType(itemStyle);
            var sizeType = ItemExportHelper.GetSizeType(firstSize);
            var categoryInfo = categoryService.GetCategory(MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId, itemStyle, gender, sizeType);

            var brandName = ItemExportHelper.GetBrandName(mainLicense, subLicense);

            var newItemType = categoryInfo.Key1;// ItemExportHelper.ItemTypeConverter(firstSize ?? "", itemType, itemStyle, gender);
            var newDepartment = StringHelper.GetFirstNotEmpty(categoryInfo.Key2, ItemExportHelper.DepartmentConverter(gender, newItemType, sizeType));
            string clothingType = categoryInfo.Key3;

            var searchTerms = model.SearchTerms;// ItemExportHelper.BuildSearchTerms(itemStyle, material, sleeve);
            var keyFeatures = model.GetBulletPoints();// ItemExportHelper.BuildKeyFeatures(mainLicense, subLicense, material);


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

            parent.Type = newItemType;


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
                parent.SwatchImageUrl = swatchImage;
            }

            parent.Color = "";// color1;
            parent.Department = newDepartment;

            parent.Size = "";
            parent.Description = model.Description;
            parent.Update = "Update";
            parent.StandardPrice = "";
            parent.SuggestedPrice = Math.Round(model.MSRP).ToString("G");
            parent.Currency = "USD";

            parent.Quantity = "";

            parent.KeyProductFeatures1 = keyFeatures.Count > 0 ? keyFeatures[0] : "";
            parent.KeyProductFeatures2 = keyFeatures.Count > 1 ? keyFeatures[1] : "";
            parent.KeyProductFeatures3 = keyFeatures.Count > 2 ? keyFeatures[2] : "";
            parent.KeyProductFeatures4 = keyFeatures.Count > 3 ? keyFeatures[3] : "";
            parent.KeyProductFeatures5 = keyFeatures.Count > 4 ? keyFeatures[4] : "";

            parent.SearchTerms1 = searchTerms;
            //parent.SearchTerms3 = "";
            //parent.SearchTerms4 = "";
            //parent.SearchTerms5 = "";

            parent.Parentage = "Parent";
            parent.ParentSKU = "";
            parent.RelationshipType = "";
            parent.VariationTheme = "Size";
            parent.FeedProductType = clothingType;

            //--------------------------
            //Child items
            //--------------------------
            foreach (var size in sizes)
            {
                var child = new ExcelProductUSViewModel();
                child.SKU = model.StyleId + "-" + ItemExportHelper.ConvertSizeForStyleId(size.Size, hasKids2);

                child.StyleItemId = size.Id;
                child.StyleId = model.Id;

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

                child.ASIN = (size.Barcodes != null && size.Barcodes.Any()) ? size.Barcodes.FirstOrDefault().Barcode : String.Empty;

                child.Title = model.Name + "," + (inlcudeSizeGroup ? " " + ItemExportHelper.FormatSizeGroupName(size.SizeGroupName) : "") + " Size " + ItemExportHelper.ConvertSizeForItemName(size.Size, hasKids2);

                child.ProductId = "UPC";
                child.BrandName = brandName;

                child.Type = newItemType;

                child.MainImageURL = images.Count > 0 ? images[0] : "";
                child.OtherImageUrl1 = images.Count > 1 ? images[1] : "";
                child.OtherImageUrl2 = images.Count > 2 ? images[2] : "";
                child.OtherImageUrl3 = images.Count > 3 ? images[3] : "";
                child.SwatchImageUrl = swatchImage;

                child.Color = ItemExportHelper.PrepareColor(String.IsNullOrEmpty(size.Color) ? color1 : size.Color);
                child.Department = newDepartment;

                child.Size = size.Size;
                child.Description = model.Description;
                child.Update = "Update";
                child.StandardPrice = model.Price.ToString("G");
                child.SuggestedPrice = Math.Round(model.MSRP).ToString("G");
                child.Currency = "USD";

                child.Quantity = size.Quantity.ToString();

                child.KeyProductFeatures1 = parent.KeyProductFeatures1;
                child.KeyProductFeatures2 = parent.KeyProductFeatures2;
                child.KeyProductFeatures3 = parent.KeyProductFeatures3;
                child.KeyProductFeatures4 = parent.KeyProductFeatures4;
                child.KeyProductFeatures5 = parent.KeyProductFeatures5;

                child.SearchTerms1 = parent.SearchTerms1;
                //child.SearchTerms1 = parent.SearchTerms1;
                //child.SearchTerms2 = parent.SearchTerms2;
                //child.SearchTerms3 = parent.SearchTerms3;
                //child.SearchTerms4 = parent.SearchTerms4;
                //child.SearchTerms5 = parent.SearchTerms5;

                child.Parentage = "Child";
                child.ParentSKU = parent.SKU;
                child.RelationshipType = "Variation";
                child.VariationTheme = "Size";

                child.FeedProductType = clothingType;

                childs.Add(child);
            }
            
            models.Add(parent);
            models.AddRange(childs);

            filename = model.StyleId + "_" + subLicense;
            filename = filename.Replace(" ", "") + "_US.xls";


            return models;
        }

    }
}