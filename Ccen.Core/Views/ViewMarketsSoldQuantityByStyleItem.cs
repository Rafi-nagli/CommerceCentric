using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Amazon.Core.Entities;

namespace Amazon.Core.Views
{
    public class ViewMarketsSoldQuantityByStyleItem
    {
        [Key]
        public long StyleItemId { get; set; }
        public long? StyleId { get; set; }
        public int? InventoryQuantity { get; set; }
        public int? SoldQuantity { get; set; }
        public int? TotalSoldQuantity { get; set; }
        public DateTime? MaxOrderDate { get; set; }
        public DateTime? MinOrderDate { get; set; }
        public int OrderCount { get; set; }
    }
}
