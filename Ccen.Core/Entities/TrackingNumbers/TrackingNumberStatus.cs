using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amazon.Core.Entities
{
    public class TrackingNumberStatus
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
