using Amazon.Core.Exports.Attributes;

namespace Ccen.Web.General.ViewModels.Exports.Types
{
    public class ExcelWFSLocationViewModel
    {
        public long PickListEntryId { get; set; }

        [ExcelSerializable("SKU", Order = 0, Width = 15)]
        public string SKU { get; set; }

        [ExcelSerializable("Barcode", Order = 1, Width = 15)]
        public string Barcode { get; set; }

        [ExcelSerializable("Title", Order = 2, Width = 35)]
        public string Title { get; set; }

        [ExcelSerializable("Quantity", Order = 3, Width = 15)]
        public int Quantity { get; set; }

        [ExcelSerializable("Location", Order = 4, Width = 15)]
        public string Location { get; set; }

        public long? LocationIndex { get; set; }
    }
}
