using Amazon.Core.Contracts.SystemActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class DeleteProductInput : ISystemActionInput
    {
        public int Market { get; set; }
        public string MarketplaceId { get; set; }
        public string SKU { get; set; }
    }
}
