using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Amazon.Core.Views
{
    public class ViewListingSale
    {
        [Key, Column(Order = 0)]
        public long Id { get; set; }

        [Key, Column(Order = 1)]
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
