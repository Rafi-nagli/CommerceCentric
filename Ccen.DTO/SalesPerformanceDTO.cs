using System;

namespace Amazon.DTO
{
    public class SalesPerformanceDTO
    {
        public long OrderId { get; set; }
        public long ShippingId { get; set; }
        public long ItemId { get; set; }

        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal? StampsShippingCost { get; set; }
        public string TotalPrice { get; set; }
        public decimal? Price { get; set; }
    }
}
