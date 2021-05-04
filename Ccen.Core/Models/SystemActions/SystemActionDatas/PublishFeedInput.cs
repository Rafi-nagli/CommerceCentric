using System.Collections.Generic;
using Amazon.Core.Contracts.SystemActions;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class PublishFeedInput : ISystemActionInput
    {
        public string FileName { get; set; }
        public int? Mode { get; set; }
        public long? FieldMappingsId { get; set; }
        public int? Market { get; set; }
        public string MarketplaceId { get; set; }
        public bool IsPrime { get; set; }
    }
}
