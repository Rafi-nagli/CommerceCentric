using System;
using System.Collections.Generic;

namespace Amazon.DTO
{
    public class RateItemDTO
    {
        public string OrderItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class RateDTO
    {
        public string CurrierName;
        public string OfferId;
        
        public decimal? Amount;
        public decimal MaxAmount;
        public DateTime ShipDate;
        public DateTime DeliveryDate;
        public DateTime? EarliestDeliveryDate;
        public string DeliveryDaysInfo;
        public int? DeliveryDays { get; set; }

        public string ServiceIdentifier;
        public string ServiceName;
        
        public int ServiceTypeUniversal;
        public int PackageTypeUniversal;

        public bool HasInsurance;
        public decimal? InsuranceCost;

        public bool HasSignConfirmation;
        public decimal? SignConfirmationCost;
        public decimal? UpChargeCost;

        public decimal? PackageLength { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }

        public int ProviderType;

        public bool IsDefault;
        public bool IsVisible;

        public int GroupId;

        public int? NumberInBatch;

        public IList<RateItemDTO> ItemOrderIds { get; set; } 
    }
}
