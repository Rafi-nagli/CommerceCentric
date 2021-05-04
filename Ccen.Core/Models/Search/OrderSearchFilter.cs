using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Search
{
    public class OrderSearchFilter
    {
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }
        public string FulfillmentChannel { get; set; }
        public string[] OrderStatus { get; set; }
        public string Status { get; set; }
        public bool IsGlobalStatus { get; set; }

        public long? DropShipperId { get; set; }

        public bool ExcludeOnHold { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string BuyerName { get; set; }
        public string OrderNumber { get; set; }
        public string EqualOrderNumber { get; set; }
        public long[] EqualOrderIds { get; set; }
        public long? BatchId { get; set; }
        public string PaymentMethod { get; set; }
        public string StyleId { get; set; }
        public long? StyleItemId { get; set; }

        public int? StartIndex { get; set; }
        public int? LimitCount { get; set; }
        public string SortField { get; set; }
        public int SortMode { get; set; }

        public bool IgnoreBatchFilter { get; set; }
        public bool IncludeNotify { get; set; }
        public bool IncludeMailInfos { get; set; }
        public bool IncludeSourceItems { get; set; }
        public bool ExcludeWithLabels { get; set; }
        public bool IncludeForceVisible { get; set; }
        public bool IncludeAllShippings { get; set; }

        public bool UnmaskReferenceStyles { get; set; }
        public bool IncludeAllItems { get; set; }

        public bool HasGlobalSearchParams()
        {
            return
                //Market != MarketType.None
                //|| !String.IsNullOrEmpty(MarketplaceId)
                !String.IsNullOrEmpty(MarketplaceId)
                || !String.IsNullOrEmpty(OrderNumber)
                || !String.IsNullOrEmpty(BuyerName)
                || !String.IsNullOrEmpty(EqualOrderNumber)
                || !String.IsNullOrEmpty(StyleId)
                || (IsGlobalStatus && !String.IsNullOrEmpty(Status))
                || StyleItemId.HasValue
                || From.HasValue
                || To.HasValue;
        }
    }
}
