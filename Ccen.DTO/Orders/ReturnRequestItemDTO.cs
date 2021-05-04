using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Orders
{
    public class ReturnRequestItemDTO
    {
        public long Id { get; set; }

        public int? LineNumber { get; set; }
        public string LineItemId { get; set; }

        public string ItemName { get; set; }
        public int? Quantity { get; set; }
        public decimal? RefundTotalAmount { get; set; }
        public string SKU { get; set; }

        public long? StyleId { get; set; }
        public long? StyleItemId { get; set; }
        public string StyleString { get; set; } 

        public DateTime? CreateDate { get; set; }
    }
}
