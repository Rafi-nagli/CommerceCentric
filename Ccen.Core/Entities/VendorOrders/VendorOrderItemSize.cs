using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.VendorOrders
{
    public class VendorOrderItemSize : BaseDateAndByEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long VendorOrderItemId { get; set; }

        public string Size { get; set; }
        public int? Breakdown { get; set; }
        public string ASIN { get; set; }
        public int Order { get; set; }
    }
}
