using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Events
{
    public class SaleEventFeedDTO
    {
        public long Id { get; set; }

        public string Filename { get; set; }
        public long SaleEventId { get; set; }

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
