using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class StyleItemQuantityHistoryDTO
    {
        public long Id { get; set; }
        public long StyleId { get; set; }
        public long StyleItemId { get; set; }

        public string Size { get; set; }
        public string Color { get; set; }

        public int? Quantity { get; set; }
        public int? FromQuantity { get; set; }

        public int? RemainingQuantity { get; set; }
        public int? BeforeRemainingQuantity { get; set; }

        public int Type { get; set; }
        public string Tag { get; set; }

        public long? SourceEntityTag { get; set; }

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }

        public string CreatedByName { get; set; }
    }
}
