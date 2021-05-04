using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities.Inventory
{
    public class OpenBoxTracking : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }
        public long BoxId { get; set; }
        public string TrackingNumber { get; set; }
        public string Carrier { get; set; }
        public DateTime? EstDeliveryDate { get; set; }
    }
}
