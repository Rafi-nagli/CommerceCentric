using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DTO.Contracts;

namespace Amazon.DTO
{
    public class OrderShippingInfoDTO : ISortableEntity
    {
        public long Id { get; set; }

        public int? ShippingNumber { get; set; }

        //Stamps.com info
        public decimal? StampsShippingCost { get; set; }

        public decimal TotalPrice { get; set; }
        public string TotalPriceCurrency { get;set; }
        
        public bool IsInsured { get; set; }
        public bool IsSignConfirmation { get; set; }

        public decimal? InsuranceCost { get; set; }
        public decimal? SignConfirmationCost { get; set; }
        public decimal? UpChargeCost { get; set; }

        public string ShipmentOfferId { get; set; }
        public int ShipmentProviderType { get; set; }
        public int ShipmentProviderName { get; set; }
        public int ShipmentProviderShortName { get; set; }

        public string CustomCarrier { get; set; }
        public string CustomShippingMethodName { get; set; }

        public string TrackingNumber { get; set; }
        public string StampsTxId { get; set; }
        public string IntegratorTxIdentifier { get; set; }

        public long? ScanFormId { get; set; }

        public DateTime? ShippingDate { get; set; }
        public string LabelPath { get; set; }

        public bool CancelLabelRequested { get; set; }
        public bool LabelCanceled { get; set; }


        public int? LabelPurchaseResult { get; set; }
        public string LabelPurchaseMessage { get; set; }
        public DateTime? LabelPurchaseDate { get; set; }
        public long? LabelPurchaseBy { get; set; }
        public string LabelPurchaseByName { get; set; }

        public int? TrackingStateSource { get; set; }
        public DateTime? TrackingStateDate { get; set; }
        public string TrackingStateEvent { get; set; }
        public DateTime? LastTrackingRequestDate { get; set; }

        public int? DeliveredStatus { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public int? DeliveryDays { get; set; }
        public string DeliveryDaysInfo { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }

        public long? PrintLabelPackId { get; set; }

        public bool OnHold { get; set; }
        public bool IsActive { get; set; }
        public bool IsVisible { get; set; }
        public int ShippingGroupId { get; set; }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public long OrderId { get; set; }
        public string OrderAmazonId { get; set; }
        public long? DropShipperId { get; set; }
        public int OrderType { get; set; }

        public int MailReasonId { get; set; }

        public long? BatchId { get; set; }
        public int? NumberInBatch { get; set; }

        public int LabelFromType { get; set; }


        public decimal? PackageLength { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }


        public IList<DTOOrderItem> Items { get; set; }
        

        #region ISortableByLocation
        public int SortIsle
        {
            get
            {
                return Items != null && Items.Any() ? Items.First().SortIsle : int.MaxValue;
            }
        }

        public int SortSection
        {
            get
            {
                return Items != null && Items.Any() ? Items.First().SortSection : int.MaxValue;
            }
        }

        public int SortShelf
        {
            get
            {
                return Items != null && Items.Any() ? Items.First().SortShelf : int.MaxValue;
            }
        }

        public string SortStyleString
        {
            get { return (Items != null && Items.Count > 0) ? Items[0].StyleId : ""; }
        }

        public string SortSize
        {
            get
            {
                return Items != null && Items.Any() ? Items.First().StyleSize : String.Empty;
            }
        }

        public string SortColor
        {
            get
            {
                return Items != null && Items.Any() ? Items.First().StyleColor : String.Empty;
            }
        }

        public string FirstItemName
        {
            get { return Items.Count > 0 ? Items[0].Title : ""; }
        }

        public string SortOrderId
        {
            get { return OrderAmazonId; }
        }
        #endregion

        public long ItemId { get; set; }

        public string PersonName { get; set; }

        public string BuyerName { get; set; }
        
        public string FormattedBuyerName
        {
            get { return String.IsNullOrEmpty(this.BuyerName) ? this.PersonName : this.BuyerName; }
        }


        public double WeightD { get; set; }
        public int TotalQuantity { get; set; }

        public string Weight
        {
            get
            {
                return WeightD > 0
                    ? WeightD >= 16
                        ? ((int)WeightD / 16) + "lb " + (WeightD % 16).ToString("F2") + "oz"
                        : (WeightD % 16).ToString("F2") + "oz"
                    : string.Empty;
            }
        }

        public string CustomNotes { get; set; }

        public int? CustomLabelSortOrder { get; set; }
        public int NumberInList { get; set; }
        
        public DTOOrder Order { get; set; }

        public ShippingMethodDTO ShippingMethod { get; set; }

        public int ShippingMethodId
        {
            get
            {
                return ShippingMethod != null ? ShippingMethod.Id : 0;
            }
        }

        public MarketIdentifier GetMarketId()
        {
            return new MarketIdentifier(Market, MarketplaceId);
        }

        public AddressDTO ToAddress { get; set; }

        //Additional
        public int LabelPrintStatus { get; set; } 
        public AddressDTO ReturnAddress { get; set; }
    }
}
