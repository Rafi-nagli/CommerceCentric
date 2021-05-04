using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Listings
{
    public class ItemAddition
    {
        [Key]
        public long Id { get; set; }
        public int ItemId { get; set; }
        public string Field { get; set; }
        public string Value { get; set; }
        public string Source { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
