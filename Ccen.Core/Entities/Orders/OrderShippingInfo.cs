using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amazon.Core.Entities
{
    public class OrderShippingInfo : BaseDateEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long OrderId { get; set; }
        public int? ShippingNumber { get; set; }
        public int ShippingGroupId { get; set; }
        public int? ShippingMethodId { get; set; }
        public string CustomCarrier { get; set; }
        public string CustomShippingMethodName { get; set; }

        [StringLength(1024)]
        public string ShipmentOfferId { get; set; }
        public int ShipmentProviderType { get; set; }

        public string LabelPath { get; set; }

        public int? LabelPurchaseResult { get; set; }
        public string LabelPurchaseMessage { get; set; }
        public DateTime? LabelPurchaseDate { get; set; }
        public long? LabelPurchaseBy { get; set; }
        public long? LabelPrintPackId { get; set; }

        public decimal? StampsShippingCost { get; set; }
        public decimal? InsuranceCost { get; set; }
        public decimal? SignConfirmationCost { get; set; }
        public decimal? UpChargeCost { get; set; }
        public double? UsedWeight { get; set; }

        public int? NumberInBatch { get; set; }
        public int? CustomLabelSortOrder { get; set; }

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

        public DateTime? ScanDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }


        public bool NotDeliveredDismiss { get; set; }
        public bool NotDeliveredSubmittedClaim { get; set; }
        public bool NotDeliveredHighlight { get; set; }


        public bool IsFeedSubmitted { get; set; }
        public bool IsFulfilled { get; set; }
        public long? FeedId { get; set; }
        public int MessageIdentifier { get; set; }
        public long? ScanFormId { get; set; }

        public bool IsActive { get; set; }
        public bool IsVisible { get; set; }
        
        public DateTime? ShippingDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }

        public DateTime? EarliestDeliveryDate { get; set; }
        public string DeliveryDaysInfo { get; set; }
        public int? DeliveryDays { get; set; }

        public string AvgDeliveryDaysByZip { get; set; }
        public decimal? AvgDeliveryDays { get; set; }

        public decimal? PackageLength { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }
    }
}
