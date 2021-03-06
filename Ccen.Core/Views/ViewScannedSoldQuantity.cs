using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Amazon.Core.Entities;

namespace Amazon.Core.Views
{
    public class ViewScannedSoldQuantity
    {
        [Key]
        public long Id { get; set; }
        public long StyleId { get; set; }
        public string Size { get; set; }
        public int? InventoryQuantity { get; set; }
        public int? SoldQuantity { get; set; }
        public int? TotalSoldQuantity { get; set; }
        public DateTime? MinOrderDate { get; set; }
        public DateTime? MaxOrderDate { get; set; }
        public int OrderCount { get; set; }
    }
}
