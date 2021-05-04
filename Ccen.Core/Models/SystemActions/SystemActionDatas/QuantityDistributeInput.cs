using Amazon.Core.Contracts.SystemActions;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class QuantityDistributeInput : ISystemActionInput
    {
        public long? StyleId { get; set; }
        public long[] StyleIdList { get; set; }
    }
}
