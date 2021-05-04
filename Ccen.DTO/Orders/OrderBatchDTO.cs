using System;

namespace Amazon.DTO
{
    public class OrderBatchDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool Archive { get; set; }
        public bool IsClosed { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockDate { get; set; }
        public int Type { get; set; }
        public bool CanArchive { get; set; }
        public DateTime? CreateDate { get; set; }
        
        public int? PrintStatus { get; set; }
        public string LabelPath { get; set; }
        public long? LabelPrintPackId { get; set; }

        public int OrdersCount { get; set; }
        public int ShippedOrdersCount { get; set; }

        public string ScanFormPath { get; set; }
        public string ScanFormId { get; set; }
    }
}
