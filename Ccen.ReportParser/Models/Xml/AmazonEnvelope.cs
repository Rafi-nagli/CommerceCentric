using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Amazon.ReportParser.Models.Xml
{
    [XmlRoot("AmazonEnvelope")]
    public class AmazonEnvelope<T>
    {
        [XmlElement("Message")]
        public T Message { get; set; }
    }
}
