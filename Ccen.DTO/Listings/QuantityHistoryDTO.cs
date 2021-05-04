using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Listings
{
    public class QuantityHistoryDTO
    {
        public long Id { get; set; }
        public int QuantityChanged { get; set; }
        public int Type { get; set; }

        public long ListingId { get; set; }

        public string SKU { get; set; }
        public long? StyleId { get; set; }
        public string StyleString { get; set; }
        public long? StyleItemId { get; set; }

        public string Size { get; set; }

        public string OrderId { get; set; }
    }
}
