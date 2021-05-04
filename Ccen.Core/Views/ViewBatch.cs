using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Views
{
    public class ViewBatch
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Archive { get; set; }
        public bool IsLocked { get; set; }
        public bool IsClosed { get; set; }
        public int Type { get; set; }
        public string FileName { get; set; }
        public long? LablePrintPackId { get; set; }
        public int? AllPrinted { get; set; }
        public int? PrintStatus { get; set; }
        public DateTime CreateDate { get; set; }
        public int? Count { get; set; }
        public int? ShippedCount { get; set; }
    }
}
