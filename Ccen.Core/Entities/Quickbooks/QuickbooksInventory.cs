using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Inventory
{
    public class QuickbooksInventory
    {
        [Key]
        public string Item { get; set; }

        public string SKU { get; set; }
        public string Quantity { get; set; }
        public long? IntQuantity { get; set; }

        public decimal? MSRP { get; set; }
        public decimal? SalePrice { get; set; }

        public int IsImageVerified { get; set; }
        public bool IsHiRes { get; set; }
        public string Image { get; set; }
        public int? ImageWidth { get; set; }
        public int? ImageHeight { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}
