using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities
{
    public class Setting
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
