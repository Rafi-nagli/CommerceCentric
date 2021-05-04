using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Amazon.ReportParser.Models.Xml
{
    public class ReturnItemDetailsNode
    {
        [XmlElement("item_name")]
        public string ItemName { get; set; }

        [XmlElement("asin")]
        public string ASIN { get; set; }

        [XmlElement("return_reason_code")]
        public string ReturnReasonCode { get; set; }

        [XmlElement("merchant_sku")]
        public string MerchantSku { get; set; }

        [XmlElement("in_policy")]
        public string InPolicy { get; set; }

        [XmlElement("return_quantity")]
        public int ReturnQuantity { get; set; }

        [XmlElement("Resolution")]
        public string Resolution { get; set; }
    }
}
