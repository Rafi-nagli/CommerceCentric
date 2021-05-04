using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.DropShippers
{
    public class DSFileLine
    {
        [Key]
        public long Id { get; set; }
        public long FileId { get; set; }

        public int Index { get; set; }

        public string Model { get; set; }
        public string SKU { get; set; }
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
        public int? Qty { get; set; }
        public string Barcode { get; set; }

        public string Data { get; set; }

        public int Status { get; set; }
    }
}
