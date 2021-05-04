using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.CustomFeeds
{
    public class CustomFeedScheduleDTO
    {
        public long Id { get; set; }
        public long CustomFeedId { get; set; }

        public DateTime StartTime { get; set; }
        public string RecurrencyPeriod { get; set; }
        public int RepeatInterval { get; set; }
        public string DaysOfWeek { get; set; }

        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
