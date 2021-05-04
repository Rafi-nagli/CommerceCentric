using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Amazon.ReportParser.Models.Xml
{
    public class ReturnDetailsNode
    {
        [XmlElement("item_details")]
        public ReturnItemDetailsNode[] ItemDetails { get; set; }

        [XmlElement("order_id")]
        public string OrderId { get; set; }

        [XmlElement("order_date")]
        public DateTime OrderDate { get; set; }

        [XmlElement("amazon_rma_id")]
        public string AmazonRmaId { get; set; }

        [XmlElement("return_request_date")]
        public DateTime ReturnRequestDate { get; set; }

        [XmlElement("return_request_status")]
        public string ReturnRequestStatus { get; set; }

        [XmlElement("a_to_z_claim")]
        public bool AtoZclaim { get; set; }

        [XmlElement("is_prime")]
        public string IsPrime { get; set; }

        [XmlElement("label_details")]
        public ReturnLabelDetailsNode LabelDetails { get; set; }

        [XmlElement("label_to_be_paid_by")]
        public string LabelToBePaidBy { get; set; }

        [XmlElement("return_type")]
        public string ReturnType { get; set; }
    }
}
