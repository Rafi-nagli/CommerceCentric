using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Shippings
{
    public class LabelDTO
    {
        public long Id { get; set; }
        public int LabelFromType { get; set; }
        public long OrderId { get; set; }
        public int? ShippingMethodId { get; set; }

        public string LabelPath { get; set; }

        public int? LabelPurchaseResult { get; set; }
        public string LabelPurchaseMessage { get; set; }
        public DateTime? LabelPurchaseDate { get; set; }
        public long? LabelPrintPackId { get; set; }

        public string Carrier { get; set; }
        public string TrackingNumber { get; set; }
        public string IntegratorTxIdentifier { get; set; }
        public string StampsTxId { get; set; }

        public bool CancelLabelRequested { get; set; }
        public bool LabelCanceled { get; set; }
        public DateTime? LabelCanceledDate { get; set; }

        public DateTime? LastTrackingRequestDate { get; set; }
        public int TrackingRequestAttempts { get; set; }

        public bool IsDelivered { get; set; }
        public int? DeliveredStatus { get; set; }

        public int? TrackingStateSource { get; set; }
        public DateTime? TrackingStateDate { get; set; }
        public string TrackingStateEvent { get; set; }
        public string TrackingLocation { get; set; }

        public DateTime? ActualDeliveryDate { get; set; }

        public DateTime? ShippingDate { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }
        public string ShippingCountry { get; set; }
    }
}
