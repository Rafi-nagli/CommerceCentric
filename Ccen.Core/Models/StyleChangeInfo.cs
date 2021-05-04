using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public class StyleChangeInfo
    {
        public long SourceStyleId { get; set; }
        public string SourceStyleString { get; set; }
        public long SourceStyleItemId { get; set; }
        public string SourceStyleSize { get; set; }

        public long DestStyleId { get; set; }
        public string DestStyleString { get; set; }
        public long DestStyleItemId { get; set; }
        public string DestStyleSize { get; set; }
    }
}
