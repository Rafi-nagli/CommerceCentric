using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities
{
    public class StyleItemQuantityHistory
    {
        [Key]
        public long Id { get; set; }
        public long StyleItemId { get; set; }

        public int? Quantity { get; set; }
        public int? FromQuantity { get; set; }
        
        public int? RemainingQuantity { get; set; }
        public int? BeforeRemainingQuantity { get; set; }

        public int Type { get; set; }
        public string Tag { get; set; }

        public long? SourceEntityTag { get; set; }
        public string SourceEntityName { get; set; }

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
