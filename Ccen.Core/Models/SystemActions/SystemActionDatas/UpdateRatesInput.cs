using Amazon.Core.Contracts.SystemActions;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class UpdateRatesInput : ISystemActionInput
    {
        public long? OrderId { get; set; }
        public string OrderNumber { get; set; }
    }
}
