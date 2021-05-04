using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Listings
{
    public class ListingDefectDTO : IReportItemDTO
    {
        public long Id { get; set; }
        public long? FeedId { get; set; }

        public string ReportId { get; set; }
        public int MarketType { get; set; }
        public string MarketplaceId { get; set; }

        public string SKU { get; set; }
        public string ASIN { get; set; }
        public string ProductName { get; set; }

        public string FieldName { get; set; }
        public string CurrentValue { get; set; }
        public DateTime? LastUpdated { get; set; }

        public string AlertType { get; set; }
        public string AlertName { get; set; }
        public string Status { get; set; }
        public string Explanation { get; set; }
    }
}
