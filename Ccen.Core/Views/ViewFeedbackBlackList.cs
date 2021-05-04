
using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Views
{
    public class ViewFeedbackBlackList
    {
        [Key]
        public long Id { get; set; }
        public string OrderId { get; set; }
        public string Reason { get; set; }
        public DateTime? CreateDate { get; set; }


        //Order Fields
        public int Market { get; set; }
        public string MarketplaceId { get; set; }
        public string MarketOrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string BuyerName { get; set; }
        public string BuyerEmail { get; set; }
        public string PersonName { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingAddress1 { get; set; }
        public string ShippingAddress2 { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingZip { get; set; }
        public string ShippingZipAddon { get; set; }
        public string ShippingPhone { get; set; }
        public string AmazonEmail { get; set; }
        
    }
}
