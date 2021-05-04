using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Emails
{
    public class EmailToOrderDTO
    {
        public long Id { get; set; }
        public long EmailId { get; set; }
        public string OrderId { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
