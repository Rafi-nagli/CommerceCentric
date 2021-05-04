using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Orders
{
    public class ReturnRequestDTO : IReportItemDTO
    {
        public long Id { get; set; }
        public string MarketReturnId { get; set; }
        public DateTime? ReturnByDate { get; set; }

        public string Name { get; set; }

        public string OrderNumber { get; set; }
        public string MarketOrderId { get; set; }
        public string Reason { get; set; }
        public string ItemName { get; set; }
        public string CustomerComments { get; set; }
        public decimal? PrepaidLabelCost { get; set; }
        public bool? HasPrepaidLabel { get; set; }
        public string PrepaidLabelBy { get; set; }
        public string Details { get; set; }
        public int? Quantity { get; set; }

        public int Type { get; set; }
        public int? ProcessMode { get; set; }

        public string Status { get; set; }
        public DateTime? StatusDate { get; set; }

        public string SKU { get; set; }
        public long? StyleId { get; set; }
        public long? StyleItemId { get; set; }
        public string StyleString { get; set; } 

        public decimal? RequestedRefundAmount { get; set; }
        public decimal? RequestedRefundShippingPrice { get; set; }

        public string SourceRequestId { get; set; }
        public string MerchantRequestId { get; set; }

        public DateTime? ReceiveDate { get; set; }
        public DateTime? CreateDate { get; set; }


        public IList<ReturnRequestItemDTO> Items { get; set; }
    }
}
