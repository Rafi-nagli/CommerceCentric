using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class SealedBoxCountingItemDto
    {
        public long Id { get; set; }
        public long BoxId { get; set; }
        public long StyleItemId { get; set; }
        public int BreakDown { get; set; }

        public string Size { get; set; }
        public string Color { get; set; }
    }
}
