using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Views
{
    public class ViewShipmentReport
    {
        [Key]
        public long Id { get; set; }

        //OrderId
        public string AmazonIdentifier { get; set; }
        public string CustomerOrderId { get; set; }
        public string MarketOrderId { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }
               

        //Tracking #
        public DateTime? ShippingDate { get; set; }


        //QuantityOrdered
        public int QuantityOrdered { get; set; }


        public decimal? ShippingCost { get; set; }
        public decimal? UpChargeCost { get; set; }
    }
}
