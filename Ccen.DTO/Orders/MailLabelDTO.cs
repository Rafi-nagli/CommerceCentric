using System;
using System.Collections.Generic;

namespace Amazon.DTO
{
    public class MailLabelDTO
    {
        public AddressDTO ToAddress { get; set; }
        public AddressDTO FromAddress { get; set; }
        public ShippingMethodDTO ShippingMethod { get; set; }
        public IList<DTOOrderItem> Items { get; set; }
        public IList<OrderShippingInfoDTO> Labels { get; set; }


        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public long? DropShipperId { get; set; }

        public string OrderId { get; set; }
        public string CustomerOrderId { get; set; }
        public long OrderEntityId { get; set; }
        public string OrderStatus { get; set; }
        public int OrderType { get; set; }
        public string SourceShippingService { get; set; }

        public decimal? PackageHeight { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageLength { get; set; }

        public int? WeightLb { get; set; }
        public double? WeightOz { get; set; }
        public decimal TotalPrice { get; set; }
        public string TotalPriceCurrency { get; set; }

        public string Notes { get; set; }
        public string Instructions { get; set; }
        //public string Service { get; set; }

        public bool IsAddressSwitched { get; set; }


        public int MarketplaceCode { get; set; }
        
        public string TrackingNumber { get; set; }
        public string IntegratorTxIdentifier { get; set; }
        public string StampsTxId { get; set; }
        public decimal? StampsShippingCost { get; set; }
        public int ShipmentProviderType { get; set; }

        public DateTime? ShippingDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }


        public int Reason { get; set; }
        public string LabelPath { get; set; }
        public long? LabelPrintPackId { get; set; }
        public bool IsUpdateRequired { get; set; }
        public bool IsCancelCurrentOrderLabel { get; set; }

        public bool IsInsured { get; set; }
        public decimal? InsuranceCost { get; set; }
        public bool IsSignConfirmation { get; set; }
        public decimal? SignConfirmationCost { get; set; }
        public decimal? UpChargeCost { get; set; }

        public string BoughtInTheCountry { get; set; }
    }
}
