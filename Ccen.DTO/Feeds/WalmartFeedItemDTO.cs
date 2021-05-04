using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Listings
{
    public class WalmartFeedItemDTO
    {
        public long FeedId { get; set; }
        public string ItemId { get; set; }
        public string Status { get; set; }
        public int Index { get; set; }
        public DateTime? UpdateDate { get; set; }

        public IList<ItemErrorDTO> Errors { get; set; }
    }
}
