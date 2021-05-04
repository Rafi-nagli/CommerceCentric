using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class BarcodeDTO
    {
        public long? Id { get; set; }
        public long StyleItemId { get; set; }
        public string Barcode { get; set; }

        //Additional
        public string StyleId { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string Picture { get; set; }

        public int? RemainingQuantity { get; set; }
    }
}
