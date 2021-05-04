using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Inventory
{
    public class StyleItemSale : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }

        public long StyleItemId { get; set; }

        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
        public int? MaxPiecesOnSale { get; set; }
        public int MaxPiecesMode { get; set; }
        public int PiecesSoldOnSale { get; set; }
        public DateTime? PiecesSoldOnSaleUpdateDate { get; set; }

        public DateTime? CloseDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
