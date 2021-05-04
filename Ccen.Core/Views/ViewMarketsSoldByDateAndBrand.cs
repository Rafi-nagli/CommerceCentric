using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Amazon.Core.Views
{
    public class ViewMarketsSoldByDateAndBrand
    {
        [Key, Column(Order = 0)]
        public DateTime Date { get; set; }
        [Key, Column(Order = 1)]
        public string Brand { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
    }
}
