using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Listings
{
    public class ViewFirstItemByParentAsin
    {
        [Key]
        public int Id { get; set; }

        public long? ParentItemId { get; set; }
        public string ParentASIN { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public string StyleString { get; set; }
        public long? StyleId { get; set; }
        public long? DropShipperId { get; set; }
        public string SKU { get; set; }
    }
}
