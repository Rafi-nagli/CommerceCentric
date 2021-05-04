using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO
{
    public class InventoryReportDTO
    {
        public long Id { get; set; }
        public string Path { get; set; }
        public DateTime? ProcessDate { get; set; }
        public int ProcessResult { get; set; }

        public DateTime RequestDate { get; set; }
        public string RequestIdentifier { get; set; }
        public int TypeId { get; set; }
        public string MarketplaceId { get; set; }
    }
}
