using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Listings
{
    public class ListingQuantityDTO
    {
        public long Id { get; set; }
        public string ListingId { get; set; }
        public string SKU { get; set; }

        public decimal CurrentPrice { get; set; }

        public int RealQuantity { get; set; }
        public DateTime? RestockDate { get; set; }
        public int? DisplayQuantity { get; set; }
        public bool IsFBA { get; set; }
        public bool IsRemoved { get; set; }

        public int? ItemPublishedStatus { get; set; }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public long? DropShipperId { get; set; }

        //public int? AutoQuantity { get; set; }
        public DateTime? AutoQuantityUpdateDate { get; set; }

        public bool QuantityUpdateRequested { get; set; }
        public bool OnHold { get; set; }
        public bool? OnHoldParent { get; set; }

        public string StyleSize { get; set; }
        public string Size { get; set; }
        public long? StyleId { get; set; }
        public long? StyleItemId { get; set; }

        public int? Rank { get; set; }
        public float ReciprocalRank { get; set; }


        public int DirtyStatus { get; set; }
        public bool IsLinked { get; set; }
        public int? OldQuantity { get; set; }
    }
}
