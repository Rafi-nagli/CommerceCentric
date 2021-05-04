using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Events
{
    public class SaleEventSizeHoldInfo
    {
        [Key]
        public long Id { get; set; }

        public long EventEntryId { get; set; }

        public long StyleItemId { get; set; }

        public int? HoldedCount { get; set; }
        public int? PurchasedCount { get; set; }
        public int? FeedIndex { get; set; }
        public long? FeedId { get; set; }

        public DateTime? ProcessedDate { get; set; }
    }
}
