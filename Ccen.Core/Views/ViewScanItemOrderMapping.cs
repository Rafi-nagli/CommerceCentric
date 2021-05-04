using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Views
{
    public class ViewScanItemOrderMapping
    {
        [Key]
        public int ItemId { get; set; }
        public long OrderId { get; set; }

        public int Quantity { get; set; }
    }
}
