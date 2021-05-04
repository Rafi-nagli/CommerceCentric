using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class OpenBoxCountingItemDto
    {
        public long Id { get; set; }
        public long BoxId { get; set; }
        public long StyleItemId { get; set; }
        public int Quantity { get; set; }

        public string Size { get; set; }
        public string Color { get; set; }
    }
}
