using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.DropShippers
{
    public class DSFileMessageDTO
    {
        public long Id { get; set; }
        public long FileId { get; set; }
        public string Message { get; set; }
        public int MessageStatus { get; set; }
    }
}
