using Amazon.Core.Entities;
using Amazon.Core.Exports.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Models.Groupons
{
    public class GrouponProductLine
    {
        [ExcelSerializable("Unique Product Name", Order = 0)]
        public string UniqueProductName { get; set; }
        [ExcelSerializable("Category", Order = 1)]
        public string Category { get; set; }
        [ExcelSerializable("SubCategory", Order = 2)]
        public string SubCategory { get; set; }
        [ExcelSerializable("Class", Order = 3)]
        public string Class { get; set; }
        [ExcelSerializable("SubClass", Order = 4)]
        public string SubClass { get; set; }

        [ExcelSerializable("Item Taxonomy", Order = 5)]
        public string ItemTaxonomy { get; set; }

        [ExcelSerializable("Proposed Cost", Order = 6)]
        public decimal ProposedCost { get; set; }
        [ExcelSerializable("MSRP", Order = 7)]
        public decimal MSRP { get; set; }

        [ExcelSerializable("Product's Condition", Order = 8)]
        public string ProductsCondition { get; set; }
        [ExcelSerializable("Inventory On-Hand", Order = 9)]
        public int InventoryOnHand { get; set; }
        [ExcelSerializable("Origin Country", Order = 10)]
        public string OriginCountry { get; set; }
        [ExcelSerializable("Is Hazmat?", Order = 11)]
        public string IsHazmat { get; set; }

        [ExcelSerializable("Product Length", Order = 12)]
        public decimal ProductLength { get; set; }
        [ExcelSerializable("Product Height", Order = 13)]
        public decimal ProductHeight { get; set; }
        [ExcelSerializable("Product Width", Order = 14)]
        public decimal ProductWidth { get; set; }
        [ExcelSerializable("Product Weight", Order = 15)]
        public double ProductWeight { get; set; }

        [ExcelSerializable("Retail Package Length", Order = 16)]
        public decimal RetailPackageLength { get; set; }
        [ExcelSerializable("Retail Package Height", Order = 17)]
        public decimal RetailPackageHeight { get; set; }
        [ExcelSerializable("Retail Package Width", Order = 18)]
        public decimal RetailPackageWidth { get; set; }
        [ExcelSerializable("Retail Package Weight", Order = 19)]
        public double RetailPackageWeight { get; set; }

        [ExcelSerializable("Manufacturer", Order = 20)]
        public string Manufacturer { get; set; }
        [ExcelSerializable("Manufacturer Model Number", Order = 21)]
        public string ManufacturerModelNumber { get; set; }
        [ExcelSerializable("UPC Code", Order = 22)]
        public string UPCCode { get; set; }
        [ExcelSerializable("Vendor SKU#", Order = 23)]
        public string VendorSKU { get; set; }

        [ExcelSerializable("Weight Unit of Measure", Order = 24)]
        public string WeightUnitOfMeasure { get; set; }
        [ExcelSerializable("Linear Unit of Measure", Order = 25)]
        public string LinearUnitOfMeasure { get; set; }
        [ExcelSerializable("Brand", Order = 26)]
        public string Brand { get; set; }
        [ExcelSerializable("Retail site link", Order = 27)]
        public string RetailSiteLink { get; set; }
        [ExcelSerializable("MSRP site link", Order = 28)]
        public string MSRPSiteLink { get; set; }

        [ExcelSerializable("Highlights", Order = 29)]
        public string Highlights { get; set; }
        [ExcelSerializable("Master Carton Length", Order = 30)]
        public int? MasterCartonLength { get; set; }
        [ExcelSerializable("Master Carton Height", Order = 31)]
        public int? MasterCartonHeight { get; set; }
        [ExcelSerializable("Master Carton Width", Order = 32)]
        public int? MasterCartonWidth { get; set; }
        [ExcelSerializable("Master Carton Weight", Order = 33)]
        public decimal? MasterCartonWeight { get; set; }

        [ExcelSerializable("Product Image 1 URL", Order = 34)]
        public string ProductImage1Url { get; set; }
        [ExcelSerializable("Product Image 2 URL", Order = 35)]
        public string ProductImage2Url { get; set; }
        [ExcelSerializable("Product Image 3 URL", Order = 36)]
        public string ProductImage3Url { get; set; }
        [ExcelSerializable("Product Image 4 URL", Order = 37)]
        public string ProductImage4Url { get; set; }
        [ExcelSerializable("Product Image 5 URL", Order = 38)]
        public string ProductImage5Url { get; set; }

        [ExcelSerializable("Cartons per Pallet", Order = 39)]
        public int? CartonsPerPallet { get; set; }
        [ExcelSerializable("Units per Carton", Order = 40)]
        public int? UnitsPerCarton { get; set; }
        [ExcelSerializable("Are Pallets Stackable", Order = 41)]
        public int? ArePalletsStackable { get; set; }
        [ExcelSerializable("Currency Unit of Measure", Order = 42)]
        public string CurrencyUnitOfMeasure { get; set; }

        [ExcelSerializable("UUID", Order = 43)]
        public string UUID { get; set; }
        [ExcelSerializable("Attribute 1 Name", Order = 44)]
        public string Attribute1Name { get; set; }
        [ExcelSerializable("Attribute 1 Value", Order = 45)]
        public string Attribute1Value { get; set; }
    }
}
