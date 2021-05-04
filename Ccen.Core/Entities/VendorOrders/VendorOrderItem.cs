using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.VendorOrders
{
    public class VendorOrderItem : BaseDateAndByEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
    }
}
