using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Orders
{
    public class OrderNotifyDto
    {
        public int Id { get; set; }
        public long OrderId { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
        public string Params { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
