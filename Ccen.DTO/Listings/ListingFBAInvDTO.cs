using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO
{
    public class ListingFBAInvDTO : IReportItemDTO
    {
        public long Id { get; set; }

        public string SellerSKU { get; set; }
        public string FulfillmentChannelSKU { get; set; }
        public string ASIN { get; set; }
        public string WarehouseConditionCode { get; set; }
        public int? QuantityAvailable { get; set; }

        public bool IsRemoved { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
