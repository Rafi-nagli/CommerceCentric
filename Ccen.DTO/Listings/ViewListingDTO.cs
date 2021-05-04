using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Listings
{
    public class ViewListingDTO
    {
        public long Id { get; set; }

        public string ListingId { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }


        public int ItemId { get; set; }
        public string ASIN { get; set; }
        public bool IsFBA { get; set; }
        public bool IsPrime { get; set; }

        public string Barcode { get; set; }
        public double? Weight { get; set; }
        public long? StyleItemId { get; set; }

        public string StyleSize { get; set; }
        public string StyleColor { get; set; }
        public int? Quantity { get; set; }
        public DateTime? RestockDate { get; set; }

        public decimal CurrentPrice { get; set; }

        public int? Rank { get; set; }
        public string Picture { get; set; }

        public string ItemStyle { get; set; }
        public string ShippingSize { get; set; }
        public string InternationalPackage { get; set; }

        public string Size { get; set; }
        public string Color { get; set; }
        public string ParentASIN { get; set; }
        public string ItemPicture { get; set; }
        public string SKU { get; set; }
        public string Title { get; set; }
        public string StyleName { get; set; }


        public string StyleString { get; set; }
        public long? StyleId { get; set; }
        public string StyleImage { get; set; }
        public bool UseStyleImage { get; set; }

        public string SourceMarketId { get; set; }
        public string SourceMarketUrl { get; set; }

        public DateTime? CreateDate { get; set; }
        public bool IsRemoved { get; set; }
    }
}
