using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Vendors
{
    public class VendorOrderDTO
    {
        public long Id { get; set; }
        public string VendorName { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public decimal PaidAmount { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? UpdateDate { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
