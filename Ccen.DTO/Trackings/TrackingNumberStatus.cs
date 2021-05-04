using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.TrackingNumbers
{
    public class TrackingNumberStatusDTO
    {
        public long Id { get; set; }
        public string Carrier { get; set; }

        public string TrackingNumber { get; set; }

        public DateTime? LastTrackingRequestDate { get; set; }
        public int TrackingRequestAttempts { get; set; }

        public bool IsDelivered { get; set; }
        public int? DeliveredStatus { get; set; }

        public int? TrackingStateSource { get; set; }
        public DateTime? TrackingStateDate { get; set; }
        public string TrackingStateType { get; set; }
        public string TrackingStateEvent { get; set; }
        public string TrackingLocation { get; set; }

        public DateTime? ScanDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
