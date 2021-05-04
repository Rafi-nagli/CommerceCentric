using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class OpenBoxTrackingDTO
    {
        public long Id { get; set; }
        public long BoxId { get; set; }
        public string TrackingNumber { get; set; }
        public string Carrier { get; set; }
        public DateTime? EstDeliveryDate { get; set; }
    }
}
