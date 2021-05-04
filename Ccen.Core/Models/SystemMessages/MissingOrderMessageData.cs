using Amazon.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.SystemMessages
{
    public class MissingOrderMessageData : ISystemMessageData
    {
        public string OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }
        public string[] MissingSKUList { get; set; }
    }
}
