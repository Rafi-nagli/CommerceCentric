using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.VendorOrders
{
    public class VendorOrderAttachment : BaseDateAndByEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long VendorOrderId { get; set; }

        public string FileName { get; set; }
    }
}
