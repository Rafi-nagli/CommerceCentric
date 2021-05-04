using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.CustomFeeds
{
    public class CustomFeedFieldDTO
    {
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
