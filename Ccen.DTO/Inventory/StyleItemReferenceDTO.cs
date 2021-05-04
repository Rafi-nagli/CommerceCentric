using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class StyleItemReferenceDTO
    {
        public long Id { get; set; }
        public long StyleId { get; set; }

        public long StyleItemId { get; set; }
        public long LinkedStyleItemId { get; set; }
    }
}
