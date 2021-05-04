using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Emails
{
    public class EmailAttachmentDTO
    {
        public long Id { get; set; }
        public long EmailId { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public string RelativePath { get; set; }
        public string PhysicalPath { get; set; }

        public DateTime? CreateDate { get; set; }
    }
}
