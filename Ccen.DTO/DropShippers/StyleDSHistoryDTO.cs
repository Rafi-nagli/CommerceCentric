using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.DropShippers
{
    public class StyleDSHistoryDTO
    {
        public long Id { get; set; }
        public long StyleId { get; set; }
        public long? DropShipperId { get; set; }
        public decimal? Cost { get; set; }
        public decimal? Price { get; set; }
        public long? LinkedDSItemId { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
