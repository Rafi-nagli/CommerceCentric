using System;
using System.Collections.Generic;

namespace Amazon.Core.Models
{
    public class SyncResult
    {
        public bool IsSuccess { get; set; }
        public List<SyncResultOrderInfo> ProcessedOrders { get; set; } 
        public List<SyncResultOrderInfo> SkippedOrders { get; set; }

        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}
