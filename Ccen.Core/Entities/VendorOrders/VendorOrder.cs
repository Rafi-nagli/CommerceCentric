using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.VendorOrders
{
    public class VendorOrder : BaseDateAndByEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string VendorName { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public decimal PaidAmount { get; set; }
        public bool IsDeleted { get; set; }
    }
}
