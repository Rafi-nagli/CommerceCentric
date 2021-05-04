using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Listings
{
    public class ItemAdditionDTO
    {
        public long Id { get; set; }
        public int ItemId { get; set; }
        public string Field { get; set; }
        public string Value { get; set; }
        public string Source { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
