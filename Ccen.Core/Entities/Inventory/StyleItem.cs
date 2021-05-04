using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities.Inventory
{
    public class StyleItem : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }
        public long StyleId { get; set; }
        public string Size { get; set; }
        public int? SizeId { get; set; }
        public string Color { get; set; }

        public double? Weight { get; set; }
        public decimal? PackageLength { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }

        public decimal? Cost { get; set; }

        public decimal? Price { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public int? Quantity { get; set; }
        public DateTime? QuantitySetDate { get; set; }
        public long? QuantitySetBy { get; set; }

        public DateTime? RestockDate { get; set; }

        public DateTime? FulfillDate { get; set; }

        public bool OnHold { get; set; }

        public string LiteCountingStatus { get; set; }
        public string LiteCountingName { get; set; }
        public DateTime? LiteCountingDate { get; set; }

        public bool LiteCountingProcessed { get; set; }
        public DateTime? LiteCountingProcessedDate { get; set; }

        public int? ApproveStatus { get; set; }
        public DateTime? ApproveStatusDate { get; set; }
        public long? ApproveStatusBy { get; set; }

        public string SourceMarketId { get; set; }

        public virtual ICollection<StyleItemBarcode> StyleItemBarcodes { get; set; } 
    }
}
