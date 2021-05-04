using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Listings
{
    public class ListingDefect
    {
        [Key]
        public long Id { get; set; }

        public long FeedId { get; set; }
        public string ReportId { get; set; }
        public int MarketType { get; set; }
        public string MarketplaceId { get; set; }

        public string SKU { get; set; }
        public string ASIN { get; set; }
        public string FieldName { get; set; }
        public string CurrentValue { get; set; }
        public DateTime? LastUpdated { get; set; }

        public string AlertType { get; set; }
        public string AlertName { get; set; }
        public string Status { get; set; }
        public string Explanation { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime UpdateDate { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
