using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities
{
    public class SyncHistory
    {
        [Key]
        public long Id { get; set; }
        public int Type { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }
        public int Status { get; set; }

        public int? CountToProcess { get; set; }
        public int ProcessedTotal { get; set; } 
        public int ProcessedWithError { get; set; }
        public int ProcessedWithWarning { get; set; }

        public string AdditionalData { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? PingDate { get; set; }
    }
}
