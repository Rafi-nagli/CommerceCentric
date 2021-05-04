using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Orders
{
    public class ReturnRequest
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string OrderNumber { get; set; }
        public string Reason { get; set; }
        public string CustomerComments { get; set; }
        public string Details { get; set; }
        public string ItemName { get; set; }

        public int Type { get; set; }
        public int? ProcessMode { get; set; }

        public string MarketReturnId { get; set; }
        public DateTime? ReturnByDate { get; set; }
        public string Status { get; set; }
        public DateTime? StatusDate { get; set; }

        public decimal? RequestedRefundAmount { get; set; }

        public bool? HasPrepaidLabel { get; set; }
        public decimal? PrepaidLabelCost { get; set; }
        public string PrepaidLabelBy { get; set; }

        public int? Quantity { get; set; }
        public string SKU {get; set; }
        
        public long? StyleId {get; set; }
        public long? StyleItemId { get; set; }
        public DateTime? ReceiveDate { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
