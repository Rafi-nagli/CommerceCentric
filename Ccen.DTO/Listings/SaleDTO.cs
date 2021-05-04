using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Listings
{
    public class SaleDTO
    {
        public long Id { get; set; }

        public decimal? DiscountValue { get; set; }
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
        public int? MaxPiecesOnSale { get; set; }
        public int? PiecesSoldOnSale { get; set; }

        public long? CreatedFromParentItemId { get; set; }
        public long? CreatedFromListingId { get; set; }

        public DateTime? CloseDate { get; set; }
        public DateTime? CreateDate { get; set; }

        //EBay
        public long? MarketSaleId { get; set; }
        public string SaleName { get; set; }
        public int? DiscountType { get; set; }
        public int? SaleType { get; set; }

        public IList<string> LinkedItemIds { get; set; }
    }
}
