
using System;
using System.Collections.Generic;
using Amazon.DTO.Inventory;

namespace Amazon.DTO
{
    public class StyleItemDTO
    {
        public long StyleItemId { get; set; }
        public long StyleId { get; set; }
        public string Size { get; set; }
        public int? SizeId { get; set; }
        public string Color { get; set; }
        public double? Weight { get; set; }
        public decimal? PackageLength { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }


        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public int? Quantity { get; set; }
        public DateTime? QuantitySetDate { get; set; }
        public long? QuantitySetBy { get; set; }

        public DateTime? RestockDate { get; set; }
        public DateTime? FulfillDate { get; set; }
        public DateTime? PreOrderBoxExpReceiptDate { get; set; }
        public bool OnHold { get; set; }

        public IList<BarcodeDTO> Barcodes { get; set; } 

        //Calcualted
        public int? RemainingQuantity { get; set; }
        public int? ReservedQuantity { get; set; }

        //Additional
        public int? SizeGroupId { get; set; }
        public string SizeGroupName { get; set; }

        public int? SizeGroupSortOrder { get; set; }
        public int? SizeSortOrder { get; set; }

        //Counting
        public bool LiteCountingProcessed { get; set; }
        public string LiteCountingStatus { get; set; }
        public int? ApproveStatus { get; set; }
    }
}
