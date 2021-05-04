using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class SealedBoxCountingDto
    {
        public long Id { get; set; }
        public long StyleId { get; set; }

        public int BoxQuantity { get; set; }
    }
}
