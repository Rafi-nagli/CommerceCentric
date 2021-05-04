using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.DropShippers
{
    public class DSFileLineDTO
    {
        public long Id { get; set; }
        public long FileId { get; set; }

        public int Index { get; set; }

        public string Model { get; set; }

        public string SKU { get; set; }
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
        public int? Qty { get; set; }
        public string Barcode { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }

        public string Data { get; set; }
        public int Status { get; set; }

        //Additional
        public IList<DSFileLineMessageDTO> Messages { get; set; } 
    }
}
