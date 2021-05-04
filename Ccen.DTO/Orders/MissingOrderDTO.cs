using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Orders
{
    public class SystemMessageDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Tag { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
        public int? Status { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
