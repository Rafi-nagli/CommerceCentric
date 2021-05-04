using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Bargains
{
    public class BargainSearchFilter
    {
        public string Keywords { get; set; }
        public string CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public int StartIndex { get; set; }
        public int LimitCount { get; set; }
    }
}
