using Amazon.Core.Exports.Attributes;

namespace Amazon.Web.ViewModels.ExcelToAmazon
{
    public class ExcelOrderVendorViewModel
    {
        [ExcelSerializable("style", Order = 0, Width = 25)]
        public string Style { get; set; }

        [ExcelSerializable("name", Order = 1, Width = 25)]
        public string Name { get; set; }

        [ExcelSerializable("sizes", Order = 2, Width = 25)]
        public string Sizes { get; set; }

        [ExcelSerializable("breakdown", Order = 3, Width = 25)]
        public string Breakdown { get; set; }

        [ExcelSerializable("qty", Order = 4, Width = 25)]
        public int Quantity { get; set; }

        [ExcelSerializable("price", Order = 5, Width = 25)]
        public double Price { get; set; }

        [ExcelSerializable("qty Date1", Order = 6, Width = 25)]
        public int QuantityDate1 { get; set; }

        //[ExcelSerializable("qty Date2", Order = 7, Width = 25)]
        //public string QuantityDate2 { get; set; }

        //[ExcelSerializable("subtotal Date1", Order = 8, Width = 25)]
        //public string SubtotalDate1 { get; set; }

        //[ExcelSerializable("Line Total", Order = 9, Width = 25)]
        //public string LineTotal { get; set; }
        
        [ExcelSerializable("Target Sale date", Order = 10, Width = 25)]
        public string TargetSaleDate { get; set; }

        [ExcelSerializable("Comment", Order = 11, Width = 25)]
        public string Comment { get; set; }
    }
}