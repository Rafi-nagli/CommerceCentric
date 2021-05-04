using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Shippings
{
    public class GetLabelDTO
    {
        public int ShipmentProviderType { get; set; }

        public decimal? NewAccountBalance { get; set; }

        public string ProviderShipmentId { get; set; }
        public string IntegratorShipmentId { get; set; }
        public int PurchaseResult { get; set; }

        public string TrackingNumber { get; set; }
        public string Carrier { get; set; }
        public DateTime? ShipDate { get; set; }
        public string ShippingServiceId {get; set; }
        public string ShippingServiceName { get; set; }
        public string ShippingServiceOfferId { get; set; }
        public decimal RateAmount {get; set; }
        public string RateAmountCurrency { get; set; }
        public DateTime? EarliestEstimatedDeliveryDate { get; set; }
        public DateTime? LatestEstimatedDeliveryDate {get; set; }
        public string DeliveryDaysInfo { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }
        public decimal? PackageLength { get; set; }
        public double Weight { get; set; }



        public decimal DeclaredAmount { get; set; }
        public decimal InsuranceAmount {get; set; }
        public bool HasInsurance { get; set; }
        public decimal? InsuranceCost { get; set; }
        
        public bool HasSignConfirmation { get; set; }
        public decimal? SignConfirmationCost { get; set; }

        public string Status { get; set; }

        public IList<LabelFileInfo> LabelFileList { get; set; }
    }
}
