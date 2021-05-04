using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.DropShippers
{
    public class CustomFeedField
    { 
        [Key]
        public long Id { get; set; }
        public long CustomFeedId { get; set; }

        public string SourceFieldName { get; set; }
        public string CustomFieldName { get; set; }
        public string CustomFieldValue { get; set; }
        public int SortOrder { get; set; }

        public DateTime? CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
