using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class StyleItemSaleDTO
    {
        public long Id { get; set; }

        public long StyleItemId { get; set; }

        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
        public int? MaxPiecesOnSale { get; set; }
        public int MaxPiecesMode { get; set; }
        public int PiecesSoldOnSale { get; set; }
        
        
        public DateTime? CloseDate { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
