using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.DropShippers
{
    public class DSItem
    {
        [Key]
        public long Id { get; set; }
        public long DropShipperId { get; set; }
        public long FromFileLineId { get; set; }
        public int? ProductType { get; set; }

        public string Model { get; set; }
        public string SKU { get; set; }
        public decimal? Price { get; set; }
        public bool IsManuallyPrice { get; set; }

        public decimal? Cost { get; set; }
        public int? Qty { get; set; }
        public string Barcode { get; set; }
        
        public string Data { get; set; }

        public long? StyleItemId { get; set; }

        public DateTime? LastQtyUpdateDate { get; set; }
        public bool QtyUpdateRequested { get; set; }
        public DateTime? LastPriceUpdateDate { get; set; }
        public bool PriceUpdateRequested { get; set; }

        public int Status { get; set; }
        public int PublishStatus { get; set; }
        public bool? IsManuallyDisabled { get; set; }

        public DateTime? UpdateDate { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
