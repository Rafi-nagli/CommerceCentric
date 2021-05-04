using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class PhotoshootPickListDTO
    {
        public long Id { get; set; }

        public bool Archived { get; set; }

        public bool IsLocked { get; set; }
        public DateTime? PhotoshootDate { get; set; }

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
