using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities
{
    public class MailLabelInfo
    {
        [Key]
        public long Id { get; set; }
        public int Marketplace { get; set; }
        public long? OrderId { get; set; }

        public string TrackingNumber { get; set; }
        public string IntegratorTxIdentifier { get; set; }
        public string StampsTxId { get; set; }

        public bool CancelLabelRequested { get; set; }
        public bool LabelCanceled { get; set; }
        public DateTime? LabelCanceledDate { get; set; }

        public decimal? StampsShippingCost { get; set; }
        public int ShipmentProviderType { get; set; }

        public DateTime? ShippingDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }

        public int ShippingMethodId { get; set; }
        public int ReasonId { get; set; }

        public string Notes { get; set; }
        public string Instructions { get; set; }

        public int WeightLb { get; set; }
        public double WeightOz { get; set; }
        public bool IsAddressSwitched { get; set; }

        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string ZipAddon { get; set; }
        public string Phone { get; set; }
        public DateTime? BuyDate { get; set; }

        public string LabelPath { get; set; }
        public long? LabelPrintPackId { get; set; }


        public bool IsInsured { get; set; }
        public decimal InsuredValue { get; set; }
        public decimal? InsuranceCost { get; set; }

        public bool IsSignConfirmation { get; set; }
        public decimal? SignConfirmationCost { get; set; }
        public decimal? UpChargeCost { get; set; }

        public bool IsUpdateRequired { get; set; }
        public bool IsCancelCurrentOrderRequested { get; set; }
        public bool IsFulfilled { get; set; }
        public long? FeedId { get; set; }
        public int? MessageIdentifier { get; set; }

        public long? ScanFormId { get; set; }

        public DateTime? LastTrackingRequestDate { get; set; }
        public int TrackingRequestAttempts { get; set; }
        public bool IsDelivered { get; set; }
        public int? DeliveredStatus { get; set; }

        public int? TrackingStateSource { get; set; }
        public DateTime? TrackingStateDate { get; set; }
        public string TrackingStateEvent { get; set; }
        public string TrackingLocation { get; set; }

        public DateTime? ActualDeliveryDate { get; set; }

        public bool NotDeliveredDismiss { get; set; }
        public bool NotDeliveredSubmittedClaim { get; set; }
        public bool NotDeliveredHighlight { get; set; }

        public DateTime? CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
