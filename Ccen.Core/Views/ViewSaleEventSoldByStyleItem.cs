using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Amazon.Core.Entities;

namespace Amazon.Core.Views
{
    public class ViewSaleEventSoldByStyleItem
    {
        [Key]
        public long Id { get; set; }
        public long StyleId { get; set; }

        public DateTime? InventorySetDate { get; set; }
        public int? InventoryQuantity { get; set; }
        public int? SoldQuantity { get; set; }
        public int? TotalSoldQuantity { get; set; }
    }
}
