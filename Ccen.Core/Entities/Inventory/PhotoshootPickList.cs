using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Inventory
{
    public class PhotoshootPickList
    {
        [Key]
        public long Id { get; set; }


        public bool Archived { get; set; }

        public bool IsLocked { get; set; }
        public DateTime? PhotoshootDate { get; set; }

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
