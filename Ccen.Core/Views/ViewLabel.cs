using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Amazon.Core.Views
{
    [Table("ViewLabels")]
    public class ViewLabel
    {
        [Key, Column(Order = 0)]
        public long Id { get; set; }
        [Key, Column(Order = 1)]
        public int LabelFromType { get; set; }
        public long OrderId { get; set; }
        public int? ShippingMethodId { get; set; }

        public string LabelPath { get; set; }

        public int? LabelPurchaseResult { get; set; }
        public string LabelPurchaseMessage { get; set; }
        public DateTime? LabelPurchaseDate { get; set; }
        public long? LabelPurchaseBy { get; set; }
        public long? LabelPrintPackId { get; set; }

        public string TrackingNumber { get; set; }
        public string IntegratorTxIdentifier { get; set; }
        public string StampsTxId { get; set; }
        public int ShipmentProviderType { get; set; }

        public bool CancelLabelRequested { get; set; }
        public bool LabelCanceled { get; set; }
        public DateTime? LabelCanceledDate { get; set; }

        public DateTime? LastTrackingRequestDate { get; set; }
        public int TrackingRequestAttempts { get; set; }

        public bool IsDelivered { get; set; }
        public int? DeliveredStatus { get; set; }

        public DateTime? TrackingStateDate { get; set; }
        public string TrackingStateEvent { get; set; }
        public string TrackingLocation { get; set; }

        public DateTime? ActualDeliveryDate { get; set; }

        public DateTime? ShippingDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
    }
}
