using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Events
{
    public class SaleEventSizeHoldInfoDTO
    {
        public long Id { get; set; }

        public long EventEntryId { get; set; }

        public long StyleItemId { get; set; }

        public int? HoldedCount { get; set; }
        public int? PurchasedCount { get; set; }
        public int? FeedIndex { get; set; }

        public DateTime? ProcessedDate { get; set; }
    }
}
