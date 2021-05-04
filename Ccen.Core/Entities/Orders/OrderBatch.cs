using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amazon.Core.Entities
{
    public class OrderBatch : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public bool Archive { get; set; }
        public bool IsClosed { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockDate { get; set; }
        public int Type { get; set; }

        public int? PrintStatus { get; set; }
        public long? LablePrintPackId { get; set; }

        public string ScanFormPath { get; set; }
        public string ScanFormId { get; set; }

        public string PickupConfirmationNumber { get; set; }
        public long? PickupTicks { get; set; }

        [NotMapped]
        public TimeSpan? PickupTime
        {
            get { return PickupTicks.HasValue ? TimeSpan.FromTicks(PickupTicks.Value) : (TimeSpan?)null; }
            set { PickupTicks = value.HasValue ? value.Value.Ticks : (long?)null; }
        }

        public DateTime? PickupDate { get; set; }
    }
}
