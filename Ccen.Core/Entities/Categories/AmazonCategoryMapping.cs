using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Categories
{
    public class AmazonCategoryMapping
    {
        [Key]
        public long Id { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }
        public string ItemStyle { get; set; }
        public string Gender { get; set; }
        public int? SizeType { get; set; }
        public string ItemTypeKeyword { get; set; }
        public string DepartmentKey { get; set; }
        public string FeedType { get; set; }
        public string BrowseNodeIds { get; set; }
        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
