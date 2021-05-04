using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.DropShippers
{
    public class DSFileMessage
    {
        [Key]
        public long Id { get; set; }
        public long FileId { get; set; }
        public string Message { get; set; }
        public int MessageStatus { get; set; }
    }
}
