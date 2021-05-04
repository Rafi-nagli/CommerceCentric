using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public class MarketplaceName
    {
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }

        public string Name { get; set; }
        public string ShortName { get; set; }
        public string DotName { get; set; }
    }
}
