using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Caches
{
    public class StyleItemCache : BaseCacheEntity
    {
        public long? StyleId { get; set; }
        public string Size { get; set; }

        public decimal? Cost { get; set; }

        public int RemainingQuantity { get; set; }
        public int RemainingOnHandQuantity { get; set; }

        public int InventoryQuantity { get; set; }
        public DateTime? InventoryQuantitySetDate { get; set; }

        public int InventoryOnHandQuantity { get; set; }
        public DateTime? InventoryOnHandQuantitySetDate { get; set; }

        public int ScannedSoldQuantityFromDate { get; set; }
        public int SentToFBAQuantityFromDate { get; set; }
        public int MarketsSoldQuantityFromDate { get; set; }
        public int MarketsSoldOnHandQuantityFromDate { get; set; }
        public int SpecialCaseQuantityFromDate { get; set; }
        public int SaleEventQuantityFromDate { get; set; }
        public int ReservedQuantityFromDate { get; set; }
        public int SentToPhotoshootQuantityFromDate { get; set; }

        public int TotalScannedSoldQuantity { get; set; }
        public int TotalSentToFBAQuantity { get; set; }
        public int TotalMarketsSoldQuantity { get; set; }
        public int TotalMarketsSoldOnHandQuantity { get; set; }
        public int TotalSpecialCaseQuantity { get; set; }
        public int TotalSaleEventQuantity { get; set; }
        public int TotalReservedQuantity { get; set; }
        public int TotalSentToPhotoshootQuantity { get; set; }

        public int? BoxQuantity { get; set; }
        public DateTime? BoxQuantitySetDate { get; set; }

        public DateTime? ScannedMaxOrderDate { get; set; }
        public DateTime? PreOrderExpReceiptDate { get; set; }

        public bool? IsInVirtual { get; set; }

        public string MarketplacesInfo { get; set; }
        public string SalesInfo { get; set; }
    }
}
