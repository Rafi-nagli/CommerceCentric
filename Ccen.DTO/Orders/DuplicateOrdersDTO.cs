using System.Collections.Generic;
using System.Linq;

namespace Amazon.DTO
{
    public class DuplicateOrdersDTO
    {
        public IList<string> OrderNumbers { get; set; } 
        public IList<ListingOrderDTO> Items { get; set; }
    }
}
