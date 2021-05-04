using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.ReportParser.Models.Xml
{
    public class AmazonEnvelopeMessageNode<T>
    {
        public T[] Items { get; set; }
    }
}
