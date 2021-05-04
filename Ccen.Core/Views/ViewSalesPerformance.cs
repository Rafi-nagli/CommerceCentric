using System;
using Amazon.Core.Contracts;

namespace Amazon.Core.Views
{
    public class ViewSalesPerformance : ReportSource
    {
        public long OrderId { get; set; }
        public long ShippingId { get; set; }
        public int ItemId { get; set; }

        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal? StampsShippingCost { get; set; }
        public string TotalPrice { get; set; }
        public decimal? Price { get; set; }
    }
}
