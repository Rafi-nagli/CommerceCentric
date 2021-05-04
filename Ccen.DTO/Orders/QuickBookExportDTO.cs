using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Orders
{
    public class QuickBookExportDTO
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }

        public string BuyerName { get; set; }
        public string ItemName { get; set; }
        public string ItemSku { get; set; }
        public decimal ItemPrice { get; set; }

        public int Quantity { get; set; }

        public DateTime OrderDate { get; set; }
    }
}
