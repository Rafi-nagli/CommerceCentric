using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO
{
    public class CommentDTO
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public string Message { get; set; }
        public int Type { get; set; }
        public long? LinkedEmailId { get; set; }

        public DateTime? CreateDate { get; set; }
        public long? CreatedBy { get; set; }
        public string CreatedByName { get; set; }
    }
}
