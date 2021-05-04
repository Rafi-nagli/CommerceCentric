using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Search
{
    public class EmailSearchFilter
    {
        public string OrderId { get; set; }
        public string BuyerName { get; set; }
        public int? Market { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool OnlyIncoming { get; set; }
        public bool OnlyWithoutAnswer { get; set; }
        public bool IncludeSystem { get; set; }
        public int ResponseStatus { get; set; }        
    }
}
