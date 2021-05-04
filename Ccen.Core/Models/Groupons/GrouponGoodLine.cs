using Amazon.Core.Exports.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Groupons
{
    public class GrouponAttribute
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class GrouponGoodLine
    {
        [ExcelSerializable("Category ID", Order = 0)]
        public string CategoryID { get; set; }

        [ExcelSerializable("Vendor SKU", Order = 1)]
        public string VendorSKU { get; set; }

        [ExcelSerializable("Product/Product Group Title", Order = 2)]
        public string Title { get; set; }

        [ExcelSerializable("Description", Order = 3)]
        public string Description { get; set; }

        [ExcelSerializable("Manufacturer", Order = 4)]
        public string Manufacturer { get; set; }

        [ExcelSerializable("Manufacturer Model Number", Order = 5)]
        public string ModelNumber { get; set; }

        [ExcelSerializable("Brand", Order = 6)]
        public string Brand { get; set; }

        [ExcelSerializable("Is this a Bundle?", Order = 7)]
        public string IsBundle { get; set; }

        [ExcelSerializable("Product Identifier Type", Order = 8)]
        public string ProductIdentifierType { get; set; }

        [ExcelSerializable("Product Identifier", Order = 9)]
        public string ProductIdentifier { get; set; }

        [ExcelSerializable("Variation Grouping ID", Order = 10)]
        public string VariationGroupingID { get; set; }

        [ExcelSerializable("DWS Cost", Order = 11)]
        public string DWSCost { get; set; }

        [ExcelSerializable("Unit Price", Order = 12)]
        public string UnitPrice { get; set; }
        
        [ExcelSerializable("Shipping Cost", Order = 13)]
        public string ShippingCost { get; set; }
        
        [ExcelSerializable("Reference Price", Order = 14)]
        public string ReferencePrice { get; set; }
        
        [ExcelSerializable("Reference Price URL", Order = 15)]
        public string ReferencePriceURL { get; set; }
        
        [ExcelSerializable("Proof of Reference Price URL 1", Order = 16)]
        public string ProofofReferencePriceURL1 { get; set; }
        
        [ExcelSerializable("Proof of Reference Price URL 2", Order = 17)]
        public string ProofofReferencePriceURL2 { get; set; }
        
        [ExcelSerializable("Proof of Reference Price URL 3", Order = 18)]
        public string ProofofReferencePriceURL3 { get; set; }
        
        [ExcelSerializable("Proof of Reference Price URL 4", Order = 19)]
        public string ProofofReferencePriceURL4 { get; set; }
        
        [ExcelSerializable("Proof of Reference Price URL 5", Order = 20)]
        public string ProofofReferencePriceURL5 { get; set; }
        
        [ExcelSerializable("Minimum Advertised Price", Order = 21)]
        public string MinimumAdvertisedPrice { get; set; }


        [ExcelSerializable("Product Weight", Order = 22)]
        public decimal ProductWeight { get; set; }

        [ExcelSerializable("Product Weight Unit", Order = 23)]
        public string ProductWeightUnit { get; set; }

        [ExcelSerializable("Product Height", Order = 24)]
        public decimal ProductHeight { get; set; }

        [ExcelSerializable("Product Length", Order = 25)]
        public decimal ProductLength { get; set; }

        [ExcelSerializable("Product Width", Order = 26)]
        public decimal ProductWidth { get; set; }

        [ExcelSerializable("Product Dimensions Unit", Order = 27)]
        public string ProductDimensionsUnit { get; set; }

        [ExcelSerializable("Is LTL Shipping Required?", Order = 28)]
        public string IsLTLShippingRequired { get; set; }

        [ExcelSerializable("Package Weight", Order = 29)]
        public decimal PackageWeight { get; set; }

        [ExcelSerializable("Package Weight Unit", Order = 30)]
        public string PackageWeightUnit { get; set; }

        [ExcelSerializable("Package Height", Order = 31)]
        public decimal PackageHeight { get; set; }

        [ExcelSerializable("Package Length", Order = 32)]
        public decimal PackageLength { get; set; }

        [ExcelSerializable("Package Width", Order = 33)]
        public decimal PackageWidth { get; set; }

        [ExcelSerializable("Package Dimensions Unit", Order = 34)]
        public string PackageDimensionsUnit { get; set; }

        [ExcelSerializable("Country of Origin", Order = 35)]
        public string CountryofOrigin { get; set; }

        [ExcelSerializable("HAZMAT?", Order = 36)]
        public string HAZMAT { get; set; }

        [ExcelSerializable("What's in the Box", Order = 37)]
        public string WhatsintheBox { get; set; }


        [ExcelSerializable("Warranty Description", Order = 38)]
        public string WarrantyDescription { get; set; }

        [ExcelSerializable("Warranty Type", Order = 39)]
        public string WarrantyType { get; set; }

        [ExcelSerializable("Warranty Provider", Order = 40)]
        public string WarrantyProvider { get; set; }

        [ExcelSerializable("Conceal Warranty Provider?", Order = 41)]
        public string ConcealWarrantyProvider { get; set; }

        [ExcelSerializable("Warranty Length", Order = 42)]
        public string WarrantyLength { get; set; }

        [ExcelSerializable("Warranty Length Unit", Order = 43)]
        public string WarrantyLengthUnit { get; set; }

        [ExcelSerializable("Warranty File URL", Order = 44)]
        public string WarrantyFileURL { get; set; }


        [ExcelSerializable("Quantity", Order = 45)]
        public int Quantity { get; set; }

        [ExcelSerializable("Minimum Manufacturer Recommended Age", Order = 46)]
        public string MinimumManufacturerRecommendedAge { get; set; }

        [ExcelSerializable("Minimum Manufacturer Recommended Age Unit", Order = 47)]
        public string MinimumManufacturerRecommendedAgeUnit { get; set; }


        [ExcelSerializable("Main Image", Order = 48)]
        public string MainImage { get; set; }

        [ExcelSerializable("Alternate Image 1", Order = 49)]
        public string AlternateImage1 { get; set; }

        [ExcelSerializable("Alternate Image 2", Order = 50)]
        public string AlternateImage2 { get; set; }

        [ExcelSerializable("Alternate Image 3", Order = 51)]
        public string AlternateImage3 { get; set; }


        [ExcelSerializable("Bullet 1 Description", Order = 52)]
        public string Bullet1Description { get; set; }

        [ExcelSerializable("Bullet 2 Description", Order = 53)]
        public string Bullet2Description { get; set; }

        [ExcelSerializable("Bullet 3 Description", Order = 54)]
        public string Bullet3Description { get; set; }

        [ExcelSerializable("Bullet 4 Description", Order = 55)]
        public string Bullet4Description { get; set; }

        [ExcelSerializable("Bullet 5 Description", Order = 56)]
        public string Bullet5Description { get; set; }

        [ExcelSerializable("Bullet 6 Description", Order = 57)]
        public string Bullet6Description { get; set; }

        [ExcelSerializable("Bullet 7 Description", Order = 58)]
        public string Bullet7Description { get; set; }

        [ExcelSerializable("Bullet 8 Description", Order = 59)]
        public string Bullet8Description { get; set; }


        [ExcelSerializable("Proof of Brand Authenticity URL 1", Order = 60)]
        public string ProofofBrandAuthenticityURL1 { get; set; }

        [ExcelSerializable("Proof of Brand Authenticity URL 2", Order = 61)]
        public string ProofofBrandAuthenticityURL2 { get; set; }

        [ExcelSerializable("Proof of Brand Authenticity URL 3", Order = 62)]
        public string ProofofBrandAuthenticityURL3 { get; set; }

        [ExcelSerializable("Proof of Brand Authenticity URL 4", Order = 63)]
        public string ProofofBrandAuthenticityURL4 { get; set; }

        [ExcelSerializable("Proof of Brand Authenticity URL 5", Order = 64)]
        public string ProofofBrandAuthenticityURL5 { get; set; }


        [ExcelSerializable("Attribute 1 Name", Order = 65)]
        public string Attribute1Name { get; set; }

        [ExcelSerializable("Attribute 1 Value", Order = 66)]
        public string Attribute1Value { get; set; }

        [ExcelSerializable("Attribute 2 Name", Order = 67)]
        public string Attribute2Name { get; set; }

        [ExcelSerializable("Attribute 2 Value", Order = 68)]
        public string Attribute2Value { get; set; }

        [ExcelSerializable("Attribute 3 Name", Order = 69)]
        public string Attribute3Name { get; set; }

        [ExcelSerializable("Attribute 3 Value", Order = 70)]
        public string Attribute3Value { get; set; }

        [ExcelSerializable("Attribute 4 Name", Order = 71)]
        public string Attribute4Name { get; set; }

        [ExcelSerializable("Attribute 4 Value", Order = 72)]
        public string Attribute4Value { get; set; }

        [ExcelSerializable("Attribute 5 Name", Order = 73)]
        public string Attribute5Name { get; set; }

        [ExcelSerializable("Attribute 5 Value", Order = 74)]
        public string Attribute5Value { get; set; }

        [ExcelSerializable("Attribute 6 Name", Order = 75)]
        public string Attribute6Name { get; set; }

        [ExcelSerializable("Attribute 6 Value", Order = 76)]
        public string Attribute6Value { get; set; }

        [ExcelSerializable("Attribute 7 Name", Order = 77)]
        public string Attribute7Name { get; set; }

        [ExcelSerializable("Attribute 7 Value", Order = 78)]
        public string Attribute7Value { get; set; }

        [ExcelSerializable("Attribute 8 Name", Order = 79)]
        public string Attribute8Name { get; set; }

        [ExcelSerializable("Attribute 8 Value", Order = 80)]
        public string Attribute8Value { get; set; }

        [ExcelSerializable("Attribute 9 Name", Order = 81)]
        public string Attribute9Name { get; set; }

        [ExcelSerializable("Attribute 9 Value", Order = 82)]
        public string Attribute9Value { get; set; }

        [ExcelSerializable("Attribute 10 Name", Order = 83)]
        public string Attribute10Name { get; set; }

        [ExcelSerializable("Attribute 10 Value", Order = 84)]
        public string Attribute10Value { get; set; }

        [ExcelSerializable("Attribute 11 Name", Order = 85)]
        public string Attribute11Name { get; set; }

        [ExcelSerializable("Attribute 11 Value", Order = 86)]
        public string Attribute11Value { get; set; }

        [ExcelSerializable("Attribute 12 Name", Order = 87)]
        public string Attribute12Name { get; set; }

        [ExcelSerializable("Attribute 12 Value", Order = 88)]
        public string Attribute12Value { get; set; }

        [ExcelSerializable("Attribute 13 Name", Order = 89)]
        public string Attribute13Name { get; set; }

        [ExcelSerializable("Attribute 13 Value", Order = 90)]
        public string Attribute13Value { get; set; }

        [ExcelSerializable("Attribute 14 Name", Order = 91)]
        public string Attribute14Name { get; set; }

        [ExcelSerializable("Attribute 14 Value", Order = 92)]
        public string Attribute14Value { get; set; }

        [ExcelSerializable("Attribute 15 Name", Order = 93)]
        public string Attribute15Name { get; set; }

        [ExcelSerializable("Attribute 15 Value", Order = 94)]
        public string Attribute15Value { get; set; }

        [ExcelSerializable("Attribute 16 Name", Order = 95)]
        public string Attribute16Name { get; set; }

        [ExcelSerializable("Attribute 16 Value", Order = 96)]
        public string Attribute16Value { get; set; }

        [ExcelSerializable("Attribute 17 Name", Order = 97)]
        public string Attribute17Name { get; set; }

        [ExcelSerializable("Attribute 17 Value", Order = 98)]
        public string Attribute17Value { get; set; }

        [ExcelSerializable("Attribute 18 Name", Order = 99)]
        public string Attribute18Name { get; set; }

        [ExcelSerializable("Attribute 18 Value", Order = 100)]
        public string Attribute18Value { get; set; }

        [ExcelSerializable("Attribute 19 Name", Order = 101)]
        public string Attribute19Name { get; set; }

        [ExcelSerializable("Attribute 19 Value", Order = 102)]
        public string Attribute19Value { get; set; }

        [ExcelSerializable("Attribute 20 Name", Order = 103)]
        public string Attribute20Name { get; set; }

        [ExcelSerializable("Attribute 20 Value", Order = 104)]
        public string Attribute20Value { get; set; }

        [ExcelSerializable("Attribute 21 Name", Order = 105)]
        public string Attribute21Name { get; set; }

        [ExcelSerializable("Attribute 21 Value", Order = 106)]
        public string Attribute21Value { get; set; }

        [ExcelSerializable("Attribute 22 Name", Order = 107)]
        public string Attribute22Name { get; set; }

        [ExcelSerializable("Attribute 22 Value", Order = 108)]
        public string Attribute22Value { get; set; }

        [ExcelSerializable("Attribute 23 Name", Order = 109)]
        public string Attribute23Name { get; set; }

        [ExcelSerializable("Attribute 23 Value", Order = 110)]
        public string Attribute23Value { get; set; }

        [ExcelSerializable("Attribute 24 Name", Order = 111)]
        public string Attribute24Name { get; set; }

        [ExcelSerializable("Attribute 24 Value", Order = 112)]
        public string Attribute24Value { get; set; }

        [ExcelSerializable("Attribute 25 Name", Order = 113)]
        public string Attribute25Name { get; set; }

        [ExcelSerializable("Attribute 25 Value", Order = 114)]
        public string Attribute25Value { get; set; }

        public IList<GrouponAttribute> Attributes { get; set; }

    }
}
