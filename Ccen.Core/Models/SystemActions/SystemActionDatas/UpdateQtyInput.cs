using Amazon.Core.Contracts.SystemActions;
using Amazon.Core.Helpers;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class UpdateQtyInput : ISystemActionInput
    {
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }
        public long? ListingId { get; set; }
        public string SKU { get; set; }
        public string SourceMarketId { get; set; }

        public int NewQty { get; set; }

        public override string ToString()
        {
            return ToStringHelper.ToString(this);
        }
    }
}
