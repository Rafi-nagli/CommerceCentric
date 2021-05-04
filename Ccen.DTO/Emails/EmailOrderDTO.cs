using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO.Shippings;

namespace Amazon.DTO
{
    public class EmailOrderDTO
    {
        public long Id { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string MessageID { get; set; }
        public long UID { get; set; }
        public int FolderType { get; set; }
        public int EmailType { get; set; }
        public bool IsReviewed { get; set; }

        public long? OrderId { get; set; }
        public string OrderIdString { get; set; }
        public string CustomerOrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? OrderShipDate { get; set; }
        public DateTime? OrderDeliveryDate { get; set; }

        public string BuyerName { get; set; }
        public int? Market { get; set; }
        public string MarketplaceId { get; set; }

        public int ResponseStatus { get; set; }
        public string AnswerMessageID { get; set; }
        public bool HasAttachments { get; set; }

        //public string LastEmailFrom { get; set; }

        public DateTime? CreateDate { get; set; }
        
        public LabelDTO Label { get; set; }

        public bool IsEscalated { get; set; }
    }
}
