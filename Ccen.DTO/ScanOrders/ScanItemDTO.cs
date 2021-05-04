using System;

namespace Amazon.DTO.ScanOrders
{
    public class ScanItemDTO
    {
        public int? Id { get; set; }
        public long? OrderId { get; set; }
        public string Barcode { get; set; }
        public int Quantity { get; set; }

        public long? StyleId { get; set; }
        public string StyleString { get; set; }
        public long? StyleItemId { get; set; }
        public string Size { get; set; }

        public string StyleName { get; set; }
        public string SubLicense { get; set; }

        public DateTime? CreateDate { get; set; }
    }
}
