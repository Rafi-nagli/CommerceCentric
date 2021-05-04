using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Emails
{
    public class EmailAttachment
    {
        [Key]
        public long Id { get; set; }
        public long EmailId { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public string Path { get; set; }

        public DateTime? CreateDate { get; set; }
    }
}
