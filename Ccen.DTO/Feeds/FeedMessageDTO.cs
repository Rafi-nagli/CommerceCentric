using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Feeds
{
    public class FeedMessageDTO
    {
        public long Id { get; set; }

        public long FeedId { get; set; }

        public long MessageId { get; set; }
        public string ResultCode { get; set; }
        public string MessageCode { get; set; }
        public string Message { get; set; }

        public DateTime CreateDate { get; set; }

        //Related
        public long? ItemId { get; set; }
    }
}
