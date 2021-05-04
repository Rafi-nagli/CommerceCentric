using Amazon.Core.Contracts.SystemActions;
using Amazon.Core.Helpers;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class CancelOrderInput : ISystemActionInput
    {
        public string OrderNumber { get; set; }
        public string ItemId { get; set; }

        public override string ToString()
        {
            return ToStringHelper.ToString(this);
        }
    }
}
