using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Sizes
{
    public class SoldSizeInfo
    {
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public long? StyleItemId { get; set; }
        public long? ListingId { get; set; }
        public long? StyleId { get; set; }

        public string Size { get; set; }
        public string Color { get; set; }


        public int? SoldQuantity { get; set; }
        public int? TotalSoldQuantity { get; set; }
        public int? TotalQuantity { get; set; }
        public DateTime? QuantitySetDate { get; set; }

        public decimal? ItemPrice { get; set; }

        //Details
        public int? BoxQuantity { get; set; }
        public DateTime? BoxQuantitySetDate { get; set; }
        public int? DirectQuantity { get; set; }
        public DateTime? DirectQuantitySetDate { get; set; }



        public DateTime? MinOrderDate { get; set; }
        public DateTime? MaxOrderDate { get; set; }
        public int? OrderCount { get; set; }
    }
}
