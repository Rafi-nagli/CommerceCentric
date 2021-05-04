using System;

namespace Amazon.DTO
{
    public class OrderToBatchDTO
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long? ShippingInfoId { get; set; }
        public long BatchId { get; set; }

        public decimal SortIndex1{ get; set; }
        public decimal SortIndex2 { get; set; }
        public decimal SortIndex3 { get; set; }
        public decimal SortIndex4 { get; set; }
        public decimal SortIndex5 { get; set; }
        //public decimal SortIndex6 { get; set; }
    }
}
