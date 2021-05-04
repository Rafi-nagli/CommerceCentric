using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Inventory
{
    public class QuantityChange : BaseDateAndByEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long Id { get; set; }
        public long QuantityOperationId { get; set; }
        public long StyleId { get; set; }
        public long StyleItemId { get; set; }
        public int Quantity { get; set; }
        public bool InActive { get; set; }

        public DateTime? ExpiredOn { get; set; }
        public string Tag { get; set; }
    }
}
