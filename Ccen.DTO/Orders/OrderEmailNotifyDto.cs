using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Orders
{
    public class OrderEmailNotifyDto
    {
        public long Id { get; set; }
        public string OrderNumber { get; set; }
        public int Type { get; set; }
        public string Reason { get; set; }
        public DateTime? CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
