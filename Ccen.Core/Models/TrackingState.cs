using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public class TrackingState
    {
        public string TrackingNumber { get; set; }

        public List<TrackingRecord> Records { get; set; }
    }
}
