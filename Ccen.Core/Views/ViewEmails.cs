using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Amazon.Core.Views
{
    public class ViewEmails
    {
        [Key()]
        [Column(Order = 1)]
        public long Id { get; set; }

        [Key()]
        [Column(Order = 2)]
        public string MappingOrderNumber { get; set; }

        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string MessageID { get; set; }
        public long UID { get; set; }

        public int Type { get; set; }
        public int FolderType { get; set; }
        public bool IsReviewed { get; set; }

        public bool HasAttachments { get; set; }

        public int ResponseStatus { get; set; }
        public string AnswerMessageID { get; set; }
        public DateTime? CreateDate { get; set; }

        //Order
        public string AmazonIdentifier { get; set; }
        public string CustomerOrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? SourceShippedDate { get; set; }
        public string BuyerName { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }
        public string ShippingCountry { get; set; }

        //Label
        public string CarrierName { get; set; }
        public string TrackingNumber { get; set; }
        public DateTime? LabelPurchaseDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public int? TrackingStateSource { get; set; }
        public DateTime? TrackingStateDate { get; set; }
        public int? DeliveredStatus { get; set; }
        public DateTime? ShippingDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
    }
}
