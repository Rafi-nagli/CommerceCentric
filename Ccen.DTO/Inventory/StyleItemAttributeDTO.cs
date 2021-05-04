using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.DTO.Inventory
{
    public class StyleItemAttributeDTO
    {
        public long Id { get; set; }
        public long StyleItemId { get; set; }

        public string Name { get; set; }
        public string Value { get; set; }

        public DateTime? UpdateDate { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
