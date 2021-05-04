using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities
{
    public class CustomBarcode
    {
        [Key]
        public long Id { get; set; }
        public string Barcode { get; set; }

        public string SKU { get; set; }
        public long? AttachSKUBy { get; set; }
        public DateTime? AttachSKUDate { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
