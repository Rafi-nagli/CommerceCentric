using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Events
{
    public class SaleEventEntryDTO
    {
        public long Id { get; set; }

        public long SaleEventId { get; set; }

        public long StyleId { get; set; }

        public string DSName { get; set; }

        public decimal QuantityPercent { get; set; }

        public decimal Price { get; set; }

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }

        //Additional
        public string StyleString { get; set; }
        public string StyleImage { get; set; }
        public decimal StyleRemainingQty { get; set; }
        public decimal StyleReservedQty { get; set; }
        public IList<StyleItemDTO> Sizes { get; set; }
    }
}
