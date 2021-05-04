using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Listings
{
    public class ItemSearchFiltersDTO
    {
        public int? MainLicense { get; set; }
        public int? SubLicense { get; set; }

        public string Keywords { get; set; }
        public long? StyleId { get; set; }
        public string StyleName { get; set; }

        public int Availability { get; set; }
        public int? ListingsMode { get; set; }

        public long? DropShipperId { get; set; }

        public List<int> Genders { get; set; }
        public string Brand { get; set; }
        public int? NoneSoldPeriod { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public int? PublishedStatus { get; set; }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public string MarketCode { get; set; }

        public int StartIndex { get; set; }
        public int LimitCount { get; set; }
    }
}
