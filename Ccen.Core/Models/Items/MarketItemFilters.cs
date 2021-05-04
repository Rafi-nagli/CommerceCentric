using System;
using System.Collections.Generic;

namespace Amazon.Core.Models.Items
{
    public class MarketItemFilters
    {
        public IList<string> ASINList { get; set; }
        public DateTime? FromDate { get; set; }

        public static MarketItemFilters Build(IList<string> asinList)
        {
            return new MarketItemFilters()
            {
                ASINList = asinList
            };
        }
    }
}
