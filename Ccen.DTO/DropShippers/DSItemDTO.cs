using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.DropShippers
{
    public class DSItemDTO
    {
        public long Id { get; set; }
        public long DropShipperId { get; set; }
        public long FromFileLineId { get; set; }

        public string Model { get; set; }
        public string SKU { get; set; }
        public decimal? Price { get; set; }
        public bool IsManuallyPrice { get; set; }
        public int? Qty { get; set; }
        public string Barcode { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }

        public long? StyleItemId { get; set; }

        public decimal? Cost { get; set; }
        public decimal? CostPercent { get; set; }
        public int CostMode { get; set; }

        public string Data { get; set; }
        public bool HasData { get; set; }

        public DateTime? LastQtyUpdateDate { get; set; }
        public bool QtyUpdateRequested { get; set; }
        public DateTime? LastPriceUpdateDate { get; set; }
        public bool PriceUpdateRequested { get; set; }

        public int Status { get; set; }
        public int PublishStatus { get; set; }

        public DateTime? UpdateDate { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
