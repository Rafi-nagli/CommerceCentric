using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Orders
{
    public class OrderSearchDTO
    {
        public long Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime? CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
