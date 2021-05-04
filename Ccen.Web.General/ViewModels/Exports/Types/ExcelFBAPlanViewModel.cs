using Amazon.Core.Exports.Attributes;

namespace Amazon.Web.Models.Exports.Types
{
    public class ExcelFBAPlanViewModel
    {
        public static string TemplatePath = "~/App_Data/Flat.File.CreateInboundPlanRequest.xls";

        [ExcelSerializable("SKU", Order = 0, Width = 25)]
        public string SKU { get; set; }

        [ExcelSerializable("UnitsPerCase", Order = 1, Width = 65)]
        public int UnitsPerCase { get; set; }

        [ExcelSerializable("NumberOfCases", Order = 2, Width = 25)]
        public int NumberOfCases { get; set; }

        [ExcelSerializable("Quantity", Order = 3, Width = 25)]
        public int Quantity { get; set; }
    }
}