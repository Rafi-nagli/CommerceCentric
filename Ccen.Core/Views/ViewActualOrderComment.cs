using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Views
{
    public class ViewActualOrderComment
    {
        [Key]
        public long Id { get; set; }
        public string Message { get; set; }

        public int OrderId { get; set; }
        public bool Deleted { get; set; }

        public long? CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public long CommentNumber { get; set; }

        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
