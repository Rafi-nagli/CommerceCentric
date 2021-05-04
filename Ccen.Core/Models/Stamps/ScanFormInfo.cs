using System.Collections.Generic;

namespace Amazon.Core.Models
{
    public class ScanFormInfo
    {
        public string ScanFormPath { get; set; }
        public string ScanFormId { get; set; }
        public IList<string> CloseoutIds { get; set; }
    }
}
