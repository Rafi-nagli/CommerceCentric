using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Orders
{
    public class ReturnRequestItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long ReturnRequestId { get; set; }

        public int? LineNumber { get; set; }
        public string LineItemId { get; set; }

        public string ItemName { get; set; }

        public int? Quantity { get; set; }
        public string SKU {get; set; }
        public decimal? RefundTotalAmount { get; set; }


        public long? StyleId {get; set; }
        public long? StyleItemId { get; set; }

        public DateTime? CreateDate { get; set; }
    }
}
