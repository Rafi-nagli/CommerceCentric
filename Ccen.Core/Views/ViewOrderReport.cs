using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Views
{
    public class ViewOrderReport
    {
        [Key]
        public long Id { get; set; }
        public long? DropShipperId { get; set; }
        public string DropShipperName { get; set; }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        //OrderDate
        public DateTime OrderDate { get; set; }
        public DateTime OrderDateDate { get; set; }

        //OrderId
        public string CustomerOrderId { get; set; }
        public string AmazonIdentifier { get; set; }
        public string MarketOrderId { get; set; }
        public string DSOrderId { get; set; }
        
        //Tracking #
        public string TrackingNumber { get; set; }
        public string CarrierName { get; set; }
        public string ShippingMethodName { get; set; }
        public DateTime? ShippingDate { get; set; }

        //PersonName
        public string PersonName { get; set; }
        public string ShippingState {get; set; }
        public string ShippingCity { get; set; }

        //QuantityOrdered
        public int QuantityOrdered { get; set; }

        //Model
        public string Model { get; set; }
        public long StyleId { get; set; }
        public long StyleItemId { get; set; }

        //ItemPaid
        public decimal? ItemPaid { get; set; }

        //ShippingPaid
        public decimal? ShippingPaid { get; set; }

        //Cost
        public decimal? Cost { get; set; }

        //ItemTax
        public decimal? ItemTax { get; set; }

        //ShippingTax
        public decimal? ShippingTax { get; set; }


        public string OrderStatus { get; set; }

        public decimal? ItemRefunded { get; set; }
        public decimal? ShippingRefunded { get; set; }

        public decimal? ItemProfit { get; set; }
        public decimal? ShippingCost { get; set; }
        public decimal? UpChargeCost { get; set; }
    }
}
