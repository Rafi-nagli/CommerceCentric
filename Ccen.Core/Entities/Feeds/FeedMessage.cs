using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Feeds
{
    public class FeedMessage
    {
        [Key]
        public long Id { get; set; }

        public long FeedId { get; set; }

        public long MessageId { get; set; }
        public string ResultCode { get; set; }
        public string MessageCode { get; set; }
        public string Message { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
