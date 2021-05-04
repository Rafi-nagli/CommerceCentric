using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class QuantityChangeDTO
    {
        public long Id { get; set; }
        public long QuantityOperationId { get; set; }
        public long StyleId { get; set; }
        public string StyleString { get; set; }
        public long StyleItemId { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
        public bool InActive { get; set; }

        public DateTime? ExpiredOn { get; set; }

        public string Tag { get; set; }

        public DateTime? CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
