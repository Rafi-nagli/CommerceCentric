using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities
{
    public class Email : BaseDateAndByEntity
    {
        [Key]
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

        public int ResponseStatus { get; set; }
        public string AnswerMessageID { get; set; }
        public int Type { get; set; }
        public int FolderType { get; set; }
        public bool IsReviewed { get; set; }
    }
}
