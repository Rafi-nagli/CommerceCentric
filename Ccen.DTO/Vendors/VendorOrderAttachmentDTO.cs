using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Vendors
{
    public class VendorOrderAttachmentDTO
    {
        public long Id { get; set; }
        public long VendorOrderId { get; set; }

        public string FileName { get; set; }

        public DateTime? UpdateDate { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
