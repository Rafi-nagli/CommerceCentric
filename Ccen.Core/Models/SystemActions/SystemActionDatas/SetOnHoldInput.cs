using Amazon.Core.Contracts.SystemActions;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class SetOnHoldInput : ISystemActionInput
    {
        public long? OrderId { get; set; }
        public string OrderNumber { get; set; }
        public bool OnHold { get; set; }
        public long? By { get; set; }
    }
}
