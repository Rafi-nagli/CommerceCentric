using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Stamps
{
    public class OrderRateInfo
    {
        public string OrderNumber { get; set; }
        public OrderTypeEnum OrderType { get; set; }

        public IList<OrderItemRateInfo> Items { get; set; }
        public IList<OrderItemRateInfo> SourceItems { get; set; }

        public DateTime? EstimatedShipDate { get; set; }

        public string ShippingService { get; set; }
        public string InitialServiceType { get; set; }
        public PackageTypeCode InternationalPackage { get; set; }

        public decimal TotalPrice { get; set; }
        public string Currency { get; set; }
        public bool IsPickup { get; set; }
    }
}
