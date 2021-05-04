using Amazon.Core.Exports.Attributes;

namespace Amazon.Web.ViewModels.ExcelToAmazon
{
    public class ExcelProductViewModel
    {
        [ExcelSerializable("SKU", Order = 0, Width = 25)]
        public string SKU { get; set; }
        [ExcelSerializable("Product Name", Order = 1, Width = 65)]
        public string Title { get; set; }
        [ExcelSerializable("Product ID", Order = 2, Width = 25)]
        public string ASIN { get; set; }
        [ExcelSerializable("Product ID Type", Order = 3, Width = 25)]
        public string ProductId { get; set; }//Hardcode to word “ASIN”
        [ExcelSerializable("Brand Name", Order = 4, Width = 25)]
        public string BrandName { get; set; }//Can be different for child elements
        [ExcelSerializable("Product Description", Order = 5, Width = 75)]
        public string Description { get; set; }//U can put same as parent’s description for all elements
        [ExcelSerializable("Item Type Keyword", Order = 6, Width = 25)]
        public string Type { get; set; }//Usually “pajama-sets” for pajamas or “nightgown” for gowns. 
        //[ExcelSerializable("Style Number", Order = 70, Width = 25)]
        //public string StyleNumber { get; set; }//todo: no use
        [ExcelSerializable("Update", Order = 8, Width = 25)]
        public string Update { get; set; }//Hardcode to word “update”
        [ExcelSerializable("Standard Price", Order = 9, Width = 25)]
        public string StandardPrice { get; set; }
        [ExcelSerializable("Suggested Price", Order = 10, Width = 25)]
        public string SuggestedPrice { get; set; }//Could be empty
        [ExcelSerializable("Currency", Order = 11, Width = 25)]
        public string Currency { get; set; }//If Suggested Price provided hardcode to “USD”
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
        [ExcelSerializable("Quantity", Order = 17, Width = 25)]
        public string Quantity { get; set; }
        [ExcelSerializable("KeyProductFeatures1", Order = 37, Width = 25)]
        public string KeyProductFeatures1 { get; set; }
        [ExcelSerializable("KeyProductFeatures2", Order = 38, Width = 25)]
        public string KeyProductFeatures2 { get; set; }
        [ExcelSerializable("KeyProductFeatures3", Order = 39, Width = 25)]
        public string KeyProductFeatures3 { get; set; }
        [ExcelSerializable("KeyProductFeatures4", Order = 40, Width = 25)]
        public string KeyProductFeatures4 { get; set; }
        [ExcelSerializable("KeyProductFeatures5", Order = 41, Width = 25)]
        public string KeyProductFeatures5 { get; set; }
        [ExcelSerializable("SearchTerms1", Order = 42, Width = 25)]
        public string SearchTerms1 { get; set; }
        [ExcelSerializable("SearchTerms2", Order = 43, Width = 25)]
        public string SearchTerms2 { get; set; }
        [ExcelSerializable("SearchTerms3", Order = 44, Width = 25)]
        public string SearchTerms3 { get; set; }
        [ExcelSerializable("SearchTerms4", Order = 45, Width = 25)]
        public string SearchTerms4 { get; set; }
        [ExcelSerializable("SearchTerms5", Order = 46, Width = 25)]
        public string SearchTerms5 { get; set; }
        [ExcelSerializable("Main Image URL", Order = 52, Width = 57)]
        public string MainImageURL { get; set; }
        //todo: BB-BE**		URL for additional images
        [ExcelSerializable("Parentage", Order = 69, Width = 25)]
        public string Parentage { get; set; } //Hardcode to 4th row – “parent”, all other rows “Child” 
        [ExcelSerializable("Parent SKU", Order = 70, Width = 35)]
        public string ParentSKU { get; set; } //BS4 –empty, others copy A4 
        [ExcelSerializable("Relationship Type", Order = 71, Width = 25)]
        public string RelationshipType { get; set; } //“Variation”, BT4 – empty
        [ExcelSerializable("Variation Theme", Order = 72, Width = 25)]
        public string VariationTheme { get; set; } //Hardcode to “Size”
        [ExcelSerializable("Color", Order = 88, Width = 25)]
        public string Color { get; set; } //Can be different for child elements
        [ExcelSerializable("Department", Order = 93, Width = 25)]
        public string Department { get; set; } //Usually: girls, boys, baby-boys, baby-girls
        [ExcelSerializable("Size", Order = 120, Width = 25)]
        public string Size { get; set; } //DQ4 - empty
    }
}