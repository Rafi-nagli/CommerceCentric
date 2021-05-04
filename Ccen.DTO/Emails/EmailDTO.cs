using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO.Emails;

namespace Amazon.DTO
{
    public class EmailDTO
    {
        public long Id { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public string CopyTo { get; set; }
        public string BCopyTo { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string MessageID { get; set; }
        public long UID { get; set; }
        public int Type { get; set; }
        public int FolderType { get; set; }

        public DateTime? CreateDate { get; set; }

        public IList<EmailAttachmentDTO> Attachments { get; set; }
    }
}
