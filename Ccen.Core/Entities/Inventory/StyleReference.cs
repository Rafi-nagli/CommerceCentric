using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Inventory
{
    public class StyleReference : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }

        public long StyleId { get; set; }
        public long LinkedStyleId { get; set; }

        public decimal? Price { get; set; }
    }
}
