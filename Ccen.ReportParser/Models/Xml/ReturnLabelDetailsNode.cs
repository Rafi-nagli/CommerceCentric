using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Amazon.ReportParser.Models.Xml
{
    public class ReturnLabelDetailsNode
    {
        [XmlElement("tracking_id")]
        public string TrackingId { get; set; }

        [XmlElement("return_carrier")]
        public string ReturnCarrier { get; set; }

        [XmlElement("currency_code")]
        public string CurrencyCode { get; set; }

        [XmlElement("label_cost")]
        public decimal? LabelCost { get; set; }

        [XmlElement("label_type")]
        public string LabelType { get; set; }
    }
}
