using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Feeds
{
    public class FeedItemDTO
    {
        public long Id { get; set; }

        public long FeedId { get; set; }

        public long MessageId { get; set; }
        public long ItemId { get; set; }
    }
}
