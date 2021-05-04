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
using Amazon.DTO.Inventory;
using Amazon.Web.ViewModels.Inventory;

namespace Amazon.Web.ViewModels.ExcelToAmazon
{
    public class ExcelProductWalmartViewModel : IExcelProductViewModel
    {
        public static string TemplatePath = "~/App_Data/Flat.File.Clothing-full.OneSheet.Walmart.xlsx";

        [ExcelSerializable("Product Name", Order = 1)]
        public string Title { get; set; }

        [ExcelSerializable("Long Description", Order = 2)]
        public string LongDescription { get; set; }


        [ExcelSerializable("Shelf Description", Order = 3)]
        public string ShelfDescription { get; set; }

        [ExcelSerializable("Short Description", Order = 4)]
        public string ShortDescription { get; set; }

        [ExcelSerializable("Main Image Url", Order = 5)]
        public string MainImageUrl { get; set; }
        
        [ExcelSerializable("Main Image Alt Text", Order = 6)]
        public string MainImageAltText { get; set; }


        [ExcelSerializable("Product Id Type", Order = 16)]
        public string ProductIdType { get; set; }

        [ExcelSerializable("Product Id", Order = 17)]
        public string ProductId { get; set; }

        
        [ExcelSerializable("Product Tax Code", Order = 39)]
        public string ProductTaxCode { get; set; }


        [ExcelSerializable("SKU", Order = 71)]
        public string SKU { get; set; }


        [ExcelSerializable("Price Currency", Order = 75)]
        public string PriceCurrency { get; set; }

        [ExcelSerializable("Price Amount", Order = 76)]
        public decimal PriceAmount { get; set; }

        [ExcelSerializable("Min Advertised Price Currency", Order = 77)]
        public string MinAdvertisedPriceCurrency { get; set; }

        [ExcelSerializable("Min Advertised Price Amount", Order = 78)]
        public decimal MinAdvertisedPriceAmount { get; set; }


        [ExcelSerializable("Shipping Weight Value", Order = 80)]
        public string ShippingWeightValue { get; set; }

        [ExcelSerializable("Shipping Weight Unit", Order = 81)]
        public string ShippingWeightUnit { get; set; }


        [ExcelSerializable("Brand", Order = 139)]
        public string Brand { get; set; }

        [ExcelSerializable("Clothing Size", Order = 142)]
        public string ClothingSize { get; set; }

        [ExcelSerializable("Color Value", Order = 144)]
        public string Color { get; set; }


        [ExcelSerializable("VariantGroupId", Order = 130)]
        public string VariantGroupId { get; set; }

        [ExcelSerializable("Variant Attribute Names", Order = 129)]
        public string VariantAttributeNames { get; set; }


      

        public static IList<ExcelProductWalmartViewModel> GenerateToExcelWalmart(IUnitOfWork db,
            IBarcodeService barcodeService,
            StyleViewModel model,
            DateTime when,
            out string filename)
        {
            var models = new List<ExcelProductWalmartViewModel>();

            var childs = new List<ExcelProductWalmartViewModel>();

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

            var brandName = ItemExportHelper.GetBrandName(mainLicense, subLicense);

            //var newItemType = ItemExportHelper.ItemTypeConverter(firstSize ?? "", itemType, itemStyle, gender);
            //var newDepartment = ItemExportHelper.DepartmentConverter(newItemType, gender, firstSize);


            var searchTerms = model.SearchTerms;// ItemExportHelper.BuildSearchTerms(itemStyle, material, sleeve);
            var keyFeatures = model.GetBulletPoints();// ItemExportHelper.BuildKeyFeatures(mainLicense, subLicense, material);


            ////--------------------------
            ////Parent item
            ////--------------------------
            //parent.SKU = model.StyleId;
            //parent.ASIN = "";
            //parent.Title = model.Name
            //               + (!String.IsNullOrEmpty(sizeGroupName) ? ", " + ItemExportHelper.FormatSizeGroupName(sizeGroupName) : String.Empty)
            //               + " " + sizeRange;

            //parent.ProductId = "";
            //parent.BrandName = brandName;

            //parent.Type = newItemType;

            //if (model.ImageSet != null)
            //{
            //    parent.MainImageURL = model.ImageSet.Image1Url;
            //    parent.OtherImageUrl1 = model.ImageSet.Image2Url;
            //    parent.OtherImageUrl2 = model.ImageSet.Image3Url;
            //    parent.OtherImageUrl3 = model.ImageSet.Image4Url;
            //}

            //parent.Color = "";// color1;
            //parent.Department = newDepartment;

            //parent.Size = "";
            //parent.Description = model.Description;
            //parent.Update = "Update";
            //parent.StandardPrice = "";
            //parent.SuggestedPrice = model.MSRP.ToString("G");
            //parent.Currency = "USD";

            //parent.Quantity = "";

            //parent.KeyProductFeatures1 = keyFeatures.Count > 0 ? keyFeatures[0] : "";
            //parent.KeyProductFeatures2 = keyFeatures.Count > 1 ? keyFeatures[1] : "";
            //parent.KeyProductFeatures3 = keyFeatures.Count > 2 ? keyFeatures[2] : "";
            //parent.KeyProductFeatures4 = keyFeatures.Count > 3 ? keyFeatures[3] : "";
            //parent.KeyProductFeatures5 = keyFeatures.Count > 4 ? keyFeatures[4] : "";

            //parent.SearchTerms1 = searchTerms;
            ////parent.SearchTerms3 = "";
            ////parent.SearchTerms4 = "";
            ////parent.SearchTerms5 = "";

            //parent.Parentage = "Parent";
            //parent.ParentSKU = "";
            //parent.RelationshipType = "";
            //parent.VariationTheme = "Size";

            //--------------------------
            //Child items
            //--------------------------
            foreach (var size in sizes)
            {
                var child = new ExcelProductWalmartViewModel();
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

                child.ProductIdType = "UPC";
                child.ProductId = (size.Barcodes != null && size.Barcodes.Any()) ? size.Barcodes.FirstOrDefault().Barcode : String.Empty;
                child.Title = model.Name + ", " + ItemExportHelper.FormatSizeGroupName(size.SizeGroupName) + " Size " + ItemExportHelper.ConvertSizeForItemName(size.Size, hasKids2);
                child.LongDescription = String.IsNullOrEmpty(model.Description) ? child.Title : model.Description;
                child.ShortDescription = String.IsNullOrEmpty(model.Description) ? child.Title : model.Description;
                child.ShelfDescription = String.IsNullOrEmpty(model.Description) ? child.Title : model.Description;
                child.Brand = brandName;

                if (model.ImageSet != null)
                {
                    child.MainImageUrl = model.ImageSet.Image1Url;
                    //child.OtherImageUrl1 = model.ImageSet.Image2Url;
                    //child.OtherImageUrl2 = model.ImageSet.Image3Url;
                    //child.OtherImageUrl3 = model.ImageSet.Image4Url;
                }
                child.MainImageAltText = child.Title;

                child.Color = ItemExportHelper.PrepareColor(String.IsNullOrEmpty(size.Color) ? color1 : size.Color);
                

                child.ClothingSize = size.Size;
                child.PriceCurrency = "USD";
                child.PriceAmount = 99.99M; //TODO: price

                child.MinAdvertisedPriceAmount = child.PriceAmount;
                child.MinAdvertisedPriceCurrency = child.PriceCurrency;

                child.VariantGroupId = model.StyleId;
                child.VariantAttributeNames = "ClothingSize"; //TODO: add optional Color

                child.ProductTaxCode = "2038345";

                child.ShippingWeightValue = size.Weight.ToString();
                child.ShippingWeightUnit = "OZ";

                childs.Add(child);
            }

            models.AddRange(childs);

            filename = model.StyleId + "_" + subLicense;
            filename = filename.Replace(" ", "") + "_Walmart" + Path.GetExtension(TemplatePath);
            
            return models;
        }

    }
}