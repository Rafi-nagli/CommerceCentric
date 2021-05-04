using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities
{
    public class StyleItemAttribute : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }
        public long StyleItemId { get; set; }

        public string Name { get; set; }
        public string Value { get; set; }        
    }
}
