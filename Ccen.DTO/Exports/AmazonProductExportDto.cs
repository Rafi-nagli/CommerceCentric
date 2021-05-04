using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Exports
{
    public class AmazonProductExportDto
    {
        public int MessageID { get; set; }

        public int? Id { get; set; }
        public long? StyleId { get; set; }
        public long? StyleItemId { get; set; }
        public string StyleString { get; set; }

        public string ProductType { get; set; }


        public string SKU { get; set; }
        public bool IsPrime { get; set; }
        public string MerchantShippingGroupName { get; set; }

        public string ASIN { get; set; }
        public string UPC { get; set; }


        public string Title { get; set; }

        public string ProductId { get; set; }

        public string ProductIdType { get; set; }//Hardcode to word “ASIN”

        public string BrandName { get; set; }//Can be different for child elements
        
        public string Description { get; set; }//U can put same as parent’s description for all elements

        public string Type { get; set; }//Usually “pajama-sets” for pajamas or “nightgown” for gowns. 
        
        public string SubType { get; set; }

        //[ExcelSerializable("Style Number", Order = 7, Width = 25)]
        //public string StyleNumber { get; set; }//todo: no use

        public string Update { get; set; }//Hardcode to word “update”


        //PART II
        public decimal? StandardPrice { get; set; }

        public decimal? SuggestedPrice { get; set; }//Could be empty

        //[ExcelSerializable("Currency", Order = 11, Width = 25)]
        public string Currency { get; set; }//If Suggested Price provided hardcode to “USD”

        public int? Quantity { get; set; }



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
        public string KeyProductFeatures1 { get; set; }

        public string KeyProductFeatures2 { get; set; }

        public string KeyProductFeatures3 { get; set; }

        public string KeyProductFeatures4 { get; set; }

        public string KeyProductFeatures5 { get; set; }

        public string SearchTerms1 { get; set; }

        //[ExcelSerializable("SearchTerms2", Order = 43, Width = 25)]
        //public string SearchTerms2 { get; set; }

        //[ExcelSerializable("SearchTerms3", Order = 44, Width = 25)]
        //public string SearchTerms3 { get; set; }

        //[ExcelSerializable("SearchTerms4", Order = 45, Width = 25)]
        //public string SearchTerms4 { get; set; }

        //[ExcelSerializable("SearchTerms5", Order = 46, Width = 25)]
        //public string SearchTerms5 { get; set; }


        public string MainImageURL { get; set; }

        public string OtherImageUrl1 { get; set; }

        public string OtherImageUrl2 { get; set; }

        public string OtherImageUrl3 { get; set; }



        //PINK
        public string Parentage { get; set; } //Hardcode to 4th row – “parent”, all other rows “Child” 

        public string ParentSKU { get; set; } //BS4 –empty, others copy A4 

        public string RelationshipType { get; set; } //“Variation”, BT4 – empty

        public string VariationTheme { get; set; } //Hardcode to “Size”


        //BROWN

        public string MaterialComposition { get; set; }

        public string Color { get; set; } //Can be different for child elements
        public string ColorMap { get; set; }

        public string Department { get; set; } //Usually: girls, boys, baby-boys, baby-girls

        public string Size { get; set; } //DQ4 - empty
        public string SizeMap { get; set; }

        public string SpecialSize { get; set; } //DQ4 - empty


        public string FulfillmentCenterID { get; set; }

        public string PackageHeight { get; set; }

        public string PackageWidth { get; set; }

        public string PackageLength { get; set; }

        public string PackageLengthUnitOfMeasure { get; set; }

        public string PackageWeight { get; set; }

        public string PackageWeightUnitOfMeasure { get; set; }

        public List<string> RecommendedBrowseNodes { get; set; }
    }
}
