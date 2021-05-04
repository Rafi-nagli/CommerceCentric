using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Feeds
{
    public class FeedItem
    {
        [Key]
        public long Id { get; set; }

        public long FeedId { get; set; }

        public long MessageId { get; set; }
        public long ItemId { get; set; }
    }
}
