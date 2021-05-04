using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class StyleAdditionDTO
    {
        public long Id { get; set; }
        public long StyleId { get; set; }
        public string Language { get; set; }
        public string Field { get; set; }
        public string Value { get; set; }
        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
