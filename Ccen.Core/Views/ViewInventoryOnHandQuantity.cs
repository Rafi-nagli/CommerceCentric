using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Amazon.Core.Entities;

namespace Amazon.Core.Views
{
    public class ViewInventoryOnHandQuantity
    {
        [Key]
        public long Id { get; set; }
        public long StyleId { get; set; }
        public string StyleString { get; set; }
        public string Size { get; set; }
        public int? Quantity { get; set; }
        public int? DirectQuantity { get; set; }
        public int? BoxQuantity { get; set; }
        public string Barcode { get; set; }
        public DateTime? QuantitySetDate { get; set; }
        public DateTime? DirectQuantitySetDate { get; set; }
        public DateTime? BoxQuantitySetDate { get; set; }
    }
}
