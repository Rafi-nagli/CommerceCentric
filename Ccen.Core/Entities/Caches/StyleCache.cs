using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Caches
{
    public class StyleCache : BaseCacheEntity
    {
        public string Brand { get; set; }
        public string MainLicense { get; set; }
        public string SubLicense { get; set; }
        public string Gender { get; set; }
        public string AgeGroup { get; set; }
        public string ItemStyle { get; set; }
        public string ShippingSizeValue { get; set; }
        public decimal? PackageLength { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }

        public string InternationalPackageValue { get; set; }
        public string ExcessiveShipmentValue { get; set; }
        public string HolidayValue { get; set; }


        public string MarketplacesInfo { get; set; }

        public DateTime? LastSoldDateOnMarket { get; set; }

        public string AssociatedASIN { get; set; }
        public string AssociatedSourceMarketId { get; set; }
        public string AssociatedMarketplaceId { get; set; }
        public int? AssociatedMarket { get; set; }

        public int? TotalQuantity { get; set; }
        public int? RemainingTotalQuantity { get; set; }

        public decimal? Cost { get; set; }
    }
}
