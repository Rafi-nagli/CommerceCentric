using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Graphs
{
    public class PurchaseByDateDTO
    {
        public DateTime Date { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public long StyleItemId { get; set; }
        public string SKU { get; set; }

        public string ItemStyle { get; set; }

        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
