using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Listings
{
    public class OfferChangeEvent
    {
        [Key]
        public long Id { get; set; }

        public string MarketplaceId { get; set; }
        public string ASIN { get; set; }
        public DateTime ChangeDate { get; set; }
        public decimal? LowestLandedPriceMerchant { get; set; }
        public decimal? LowestListingPriceMerchant { get; set; }
        public decimal? LowestShippingMerchant { get; set; }

        public decimal? LowestLandedPriceAmazon { get; set; }
        public decimal? LowestListingPriceAmazon { get; set; }
        public decimal? LowestShippingAmazon { get; set; }

        public decimal? BuyBoxLandedPrice { get; set; }
        public decimal? BuyBoxListingPrice { get; set; }
        public decimal? BuyBoxShipping { get; set; }

        public decimal? ListPrice { get; set; }


        public int? NumberOfOffersMerchant { get; set; }
        public int? NumberOfOffersAmazon { get; set; }

        public int? BuyBoxEligibleOffersMerchant { get; set; }
        public int? BuyBoxEligibleOffersAmazon { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
