using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO
{
    public class CustomBarcodeDTO
    {
        public long Id { get; set; }
        public string Barcode { get; set; }

        public string SKU { get; set; }
        public long? AttachSKUBy { get; set; }
        public DateTime? AttachSKUDate { get; set; }

        public DateTime CreateDate { get; set; }

        //Additional
        public bool IsNewAssociation { get; set; }
    }
}
