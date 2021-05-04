using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO
{
    public class BuyBoxStatusDTO
    {
        public long Id { get; set; }
        public string ASIN { get; set; }
        public string Barcode { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        
        public string WinnerMerchantName { get; set; }
        public decimal? WinnerPrice { get; set; }
        public decimal? WinnerSalePrice { get; set; }
        public decimal? WinnerAmountSaved { get; set; }

        public DateTime CheckedDate { get; set; }
        public BuyBoxStatusCode Status { get; set; }
        public DateTime? LostWinnerDate { get; set; }
        public bool IsIgnored { get; set; }


        //Item Field
        public int? Quantity { get; set; }
        public decimal Price { get; set; }

        public string Size { get; set; }
        public string ParentASIN { get; set; }
        public string SKU { get; set; }
        public string Images { get; set; }

        public string StyleString { get; set; }
        public long? StyleId { get; set; }
    }
}
