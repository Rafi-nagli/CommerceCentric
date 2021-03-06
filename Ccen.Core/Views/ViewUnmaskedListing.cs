using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Amazon.Core.Views
{
    [Table("ViewUnmaskedListings")]
    public class ViewUnmaskedListing 
    {
        [Key, Column(Order = 0)]
        public long Id { get; set; }

        [Key, Column(Order = 1)]
        public long? StyleId { get; set; }
        
        public long? StyleItemId { get; set; }

        public string ListingId { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }


        public int ItemId { get; set; }
        public string ASIN { get; set; }
        public bool IsFBA { get; set; }

        public double? Weight { get; set; }

        public string StyleSize { get; set; }
        public string StyleColor { get; set; }
        public int? Quantity { get; set; }
        public DateTime? RestockDate { get; set; }

        public decimal CurrentPrice { get; set; }
        public decimal? CurrentPriceInUSD { get; set; }

        public int? Rank { get; set; }
        public string Picture { get; set; }

        public string ItemStyle { get; set; }
        public string ShippingSize { get; set; }
        public string InternationalPackage { get; set; }

        public string Size { get; set; }
        public string ColorVariation { get; set; }
        public string Color { get; set; }
        public string ParentASIN { get; set; }
        public bool? OnHoldParent { get; set; }
        public string ItemPicture { get; set; }
        public string SKU { get; set; }
        public string Title { get; set; }

        public string StyleString { get; set; }
        public string StyleImage { get; set; }


        public string SourceMarketId { get; set; }
        public string SourceMarketUrl { get; set; }

        public DateTime? OpenDate { get; set; }
        public DateTime? CreateDate { get; set; }

        public bool IsRemoved { get; set; }
    }
}
