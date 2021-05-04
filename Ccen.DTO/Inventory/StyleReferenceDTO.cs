using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class StyleReferenceDTO
    {
        public long Id { get; set; }
        public long StyleId { get; set; }

        public long LinkedStyleId { get; set; }
        public string LinkedStyleString { get; set; }

        public decimal? Price { get; set; }
    }
}
