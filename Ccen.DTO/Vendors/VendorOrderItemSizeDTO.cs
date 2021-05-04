using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Vendors
{
    public class VendorOrderItemSizeDTO
    {
        public long Id { get; set; }
        public long VendorOrderItemId { get; set; }

        public string Size { get; set; }
        public int? Breakdown { get; set; }
        public string ASIN { get; set; }

        public int Order { get; set; }

        public DateTime? UpdateDate { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
