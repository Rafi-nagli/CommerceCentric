using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Inventory
{
    public class StyleAddition
    {
        [Key]
        public long Id { get; set; }
        public long StyleId { get; set; }
        public string Language { get; set; }
        public string Field { get; set; }
        public string Value { get; set; }
        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
