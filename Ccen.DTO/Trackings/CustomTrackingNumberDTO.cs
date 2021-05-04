using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.DTO.Trackings
{
    public class CustomTrackingNumberDTO
    {
        public long Id { get; set; }
        public string TrackingNumber { get; set; }

        public long? AttachedToShippingInfoId { get; set; }
        public string AttachedToTrackingNumber { get; set; }

        public DateTime? AttachedDate { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
