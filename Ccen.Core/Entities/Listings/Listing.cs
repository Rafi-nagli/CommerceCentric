using System;
using System.ComponentModel.DataAnnotations;
using Amazon.Core.Helpers;

namespace Amazon.Core.Entities
{
    public class Listing : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }
        public string ListingId { get; set; }
        public int ItemId { get; set; }
        public string SKU { get; set; }
        public string SourceSKU { get; set; }
        public string ASIN { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public bool IsDefault { get; set; }
        public bool IsFBA { get; set; }
        public bool IsPrime { get; set; }

        public decimal CurrentPrice { get; set; }
        public DateTime? CurrentPriceUpdateDate { get; set; }
        public string CurrentPriceCurrency { get; set; }
        public decimal? CurrentPriceInUSD { get; set; }

        public bool IsManualPrice { get; set; }

        public decimal? BusinessPrice { get; set; }

        public decimal? AutoAdjustedPrice { get; set; }

        public int? DisplayQuantity { get; set; }
        public int RealQuantity { get; set; }
        public DateTime? RealQuantityUpdateDate { get; set; }

        public int? SoldQuantity { get; set; }

        public bool OnHold { get; set; }
        public DateTime? AutoQuantityUpdateDate { get; set; }
        public DateTime? RestockDate { get; set; }
        

        //Source info from Amazon
        public int? AmazonRealQuantity { get; set; }
        public DateTime? AmazonRealQuantityUpdateDate { get; set; }
        public decimal? AmazonCurrentPrice { get; set; }
        public DateTime? AmazonCurrentPriceUpdateDate { get; set; }
        public int? SendToAmazonQuantity { get; set; }

        public decimal? ListingPriceFromMarket { get; set; }
        public decimal? ReqularPriceFromMarket { get; set; }
        public decimal? ShippingPriceFromMarket { get; set;  }
        public DateTime? PriceFromMarketUpdatedDate { get; set; }

        public DateTime? OpenDate { get; set; }
        
        public decimal? LowestPrice { get; set; }
        public DateTime? LowestPriceUpdateDate { get; set; }

        public bool PriceUpdateRequested { get; set; }
        public DateTime? PriceUpdateRequestedDate { get; set; }

        public long? PriceFeedId { get; set; }
        public DateTime? LastPriceUpdatedOnMarket { get; set; }
        
        public bool QuantityUpdateRequested { get; set; }
        public DateTime? QuantityUpdateRequestedDate { get; set; }
        public long? InventoryFeedId { get; set; }
        public DateTime? LastQuantityUpdatedOnMarket { get; set; }

        public int MessageIdentifier { get; set; }

        public bool IsRemoved { get; set; }

        public override string ToString()
        {
            return ToStringHelper.ToString(this);
        }
    }
}
