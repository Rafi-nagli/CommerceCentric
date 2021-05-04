using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Vendors
{
    public class VendorOrderItemDTO
    {
        public long Id { get; set; }
        public long VendorOrderId { get; set; }

        public string StyleString { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int? QuantityDate1 { get; set; }
        //public int? QuantityDate2 { get; set; }
        //public decimal? SubtotalDate1 { get; set; }
        //public decimal LineTotal { get; set; }
        public DateTime? TargetSaleDate { get; set; }
        public string Comment { get; set; }
        public int? AvailableQuantity { get; set; }
        public string RelatedStyle { get; set; }
        public string Reason { get; set; }
        public string Picture { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? UpdateDate { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
