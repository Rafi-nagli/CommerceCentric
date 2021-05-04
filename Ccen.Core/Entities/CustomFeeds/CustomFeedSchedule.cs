using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.DropShippers
{
    public class CustomFeedSchedule
    {
        [Key]
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
