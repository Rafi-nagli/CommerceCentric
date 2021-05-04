using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Amazon.ReportParser.Models.Xml
{
    public class ReturnReportMessageNode
    {
        [XmlElement("return_details")]
        public ReturnDetailsNode[] ReturnDetails { get; set; }
    }
}
