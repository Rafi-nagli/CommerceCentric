using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Listings
{
    public class OfferInfo
    {
        [Key]
        public long Id { get; set; }

        public long OfferChangeEventId { get; set; }

        public string SellerId { get; set; }
        public int? SellerPositiveFeedbackRating { get; set; }
        public int? FeedbackCount { get; set; }

        public int? ShippingTimeMinimumHours { get; set; }
        public int? ShippingTimeMaximumHours { get; set; }
        public string ShippingTimeAvailabilityType { get; set; }

        public decimal? ListingPrice { get; set; }
        public decimal? Shipping { get; set; }
        public string ShipsFromCountry { get; set; }
        public string ShipsFromState { get; set; }

        public bool IsFulfilledByAmazon { get; set; }
        public bool IsBuyBoxWinner { get; set; }
        public bool IsFeaturedMerchant { get; set; }
        public bool ShipsDomestically { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
