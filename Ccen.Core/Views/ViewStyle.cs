using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Views
{
    public class ViewStyle
    {
        [Key]
        public long Id { get; set; }
        public string StyleID { get; set; }
        public bool OnHold { get; set; }

        public string Name { get; set; }
        public int Type { get; set; }

        public string DisplayName { get; set; }
        public string Image { get; set; }

        public int? ManuallyQuantity { get; set; }
        public int? HasManuallyQuantity { get; set; }

        public int BoxQuantity { get; set; }
        public decimal BoxTotalPrice { get; set; }
        public decimal? BoxItemMinPrice { get; set; }
        public decimal? BoxItemMaxPrice { get; set; }

        public long LocationIndex { get; set; }

        public string AssociatedASIN { get; set; }
        public int? AssociatedMarket { get; set; }
        public string AssociatedMarketplaceId { get; set; }

        public int? ItemTypeId { get; set; }
        public string ItemTypeName { get; set; }

        public int DisplayMode { get; set; }

        public bool RemovePriceTag { get; set; }
        public int PictureStatus { get; set; }
        public int FillingStatus { get; set; }

        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? ReSaveDate { get; set; }
    }
}
