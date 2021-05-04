using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class ItemToStyleDTO
    {
        public long Id { get; set; }

        public int ItemId { get; set; }
        public long StyleId { get; set; }
        public long? StyleItemId { get; set; }

        public DateTime? CreateDate { get; set; }
    }
}
