using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Inventory
{
    public class StyleItemReference : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }

        public long StyleId { get; set; }

        public long StyleItemId { get; set; }
        public long LinkedStyleItemId { get; set; }
    }
}
