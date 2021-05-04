using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class StyleGroupDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public long StyleCount { get; set; }

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
