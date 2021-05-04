using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DTO.Contracts;
using Amazon.DTO.Orders;

namespace Amazon.DTO
{
    public class PackingListDTO : ISortableEntity
    {
        public long Id { get; set; }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }
        
        public long OrderEntityId { get; set; }
        public string CustomerOrderId { get; set; }
        public string OrderId { get; set; }
        public string SalesRecordNumber { get; set; }
        public string OrderStatus { get; set; }

        public long? DropShipperId { get; set; }
        public string DSReturnAddress { get; set; }
        public int? DSSortOrder { get; set; }

        public bool OnHold { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal ShippingPrice { get; set; }
        public decimal ShippingPaid { get; set; }
        public decimal ShippingTax { get; set; }

        public int Quantity { get; set; }
        public long ShippingInfoId { get; set; }

        public string ShippingMethodName { get; set; }
        public int ShippingMethodId { get; set; }
        public string InitialServiceType { get; set; }

        public string SourceShippingService { get; set; }

        public string Carrier { get; set; }

        public decimal? StampsShippingCost { get; set; }
        

        public DateTime? OrderDate { get; set; }

        public long? BatchId { get; set; }
        public int? NumberInBatch { get; set; }

        public string BuyerName { get; set; }
        public string PersonName { get; set; }

        #region Shipping Address
        public string ShippingAddress1 { get; set; }
        public string ShippingAddress2 { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingZip { get; set; }
        public string ShippingZipAddon { get; set; }
        public string ShippingPhone { get; set; } 
        #endregion
        
        public bool IsManuallyUpdated { get; set; }
        #region Manual Shipping Address
        public string ManuallyPersonName { get; set; }
        public string ManuallyShippingAddress1 { get; set; }
        public string ManuallyShippingAddress2 { get; set; }
        public string ManuallyShippingCity { get; set; }
        public string ManuallyShippingState { get; set; }
        public string ManuallyShippingCountry { get; set; }
        public string ManuallyShippingZip { get; set; }
        public string ManuallyShippingZipAddon { get; set; }
        public string ManuallyShippingPhone { get; set; }
        #endregion

        #region Final Shipping Address

        public string FinalPersonName
        {
            get
            {
                if (!String.IsNullOrEmpty(ManuallyPersonName)
                    && IsManuallyUpdated)
                    return ManuallyPersonName;
                return PersonName; 
            }
        }
        public string FinalShippingAddress1
        {
            get
            {
                return IsManuallyUpdated ? ManuallyShippingAddress1 : ShippingAddress1;
            }
        }

        public string FinalShippingAddress2
        {
            get
            {
                return IsManuallyUpdated ? ManuallyShippingAddress2 : ShippingAddress2;
            }
        }

        public string FinalShippingCity
        {
            get
            {
                return IsManuallyUpdated ? ManuallyShippingCity : ShippingCity;
            }
        }

        public string FinalShippingState
        {
            get
            {
                return IsManuallyUpdated ? ManuallyShippingState : ShippingState;
            }
        }

        public string FinalShippingCountry
        {
            get
            {
                return IsManuallyUpdated ? ManuallyShippingCountry : ShippingCountry;
            }
        }

        public string FinalShippingZip
        {
            get
            {
                return IsManuallyUpdated ? ManuallyShippingZip : ShippingZip;
            }
        }

        public string FinalShippingZipAddon
        {
            get
            {
                return IsManuallyUpdated ? ManuallyShippingZipAddon : ShippingZipAddon;
            }
        }

        public string FinalShippingPhone
        {
            get
            {
                return IsManuallyUpdated ? ManuallyShippingPhone : ShippingPhone;
            }
        }
        #endregion
        public IList<ListingOrderDTO> Items { get; set; }
        public IList<OrderNotifyDto> Notifies { get; set; }

        #region ISortableByLocation

        public long? SortLocationIndex
        {
            get { return Items != null && Items.Any() ? Items.First().LocationIndex : null; }
        }

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
            get { return (Items != null && Items.Count > 0) ? Items[0].StyleID : ""; }
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
            get { return OrderId; }
        }
        #endregion
    }
}
