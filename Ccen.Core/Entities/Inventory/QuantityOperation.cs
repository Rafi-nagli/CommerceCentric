using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Inventory
{
    public class QuantityOperation : BaseDateAndByEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long Id { get; set; }
        public int Type { get; set; }
        public string OrderId { get; set; }
        public string Comment { get; set; }

        //Navigation
        public virtual IList<QuantityChange> QuantityChanges { get; set; }
    }
}
