using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Listings
{
    public class ViewListingSaleDTO
    {
        public long Id { get; set; }

        public long ListingId { get; set; }

        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
        public int? MaxPiecesOnSale { get; set; }
        public int? PiecesSoldOnSale { get; set; }
        public int MaxPiecesMode { get; set; }

        public decimal? SalePrice { get; set; }
        public decimal? SalePercent { get; set; }

        public DateTime ListingSaleCreateDate { get; set; }
    }
}
