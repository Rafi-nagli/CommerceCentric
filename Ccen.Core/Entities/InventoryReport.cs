using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities
{

    //TODO: AmazonReport
    public class InventoryReport
    {
        [Key]
        public long Id { get; set; }
        public string Path { get; set; }
        public DateTime? ProcessDate { get; set; }
        public int ProcessResult { get; set; }

        public DateTime RequestDate { get; set; }

        public string ReportIdentifier { get; set; }
        public string RequestIdentifier { get; set; }
        
        public int TypeId { get; set; }
        public string MarketplaceId { get; set; }
    }
}
