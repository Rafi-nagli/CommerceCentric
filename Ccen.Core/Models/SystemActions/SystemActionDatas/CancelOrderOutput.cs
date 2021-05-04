using Amazon.Core.Contracts.SystemActions;
using Amazon.Core.Helpers;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class CancelOrderOutput : ISystemActionOutput
    {
        public long FeedId { get; set; }
        public int Identifier { get; set; }
        public bool IsProcessed { get; set; }
        public string ResultMessage { get; set; }

        public override string ToString()
        {
            return ToStringHelper.ToString(this);
        }
    }
}
