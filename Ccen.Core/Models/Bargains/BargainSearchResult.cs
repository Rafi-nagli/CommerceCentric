using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Bargains
{
    public class BargainSearchResult
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int StartIndex { get; set; }

        public IList<BargainItem> Bargains { get; set; }
    }
}
