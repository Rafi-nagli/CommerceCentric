using Amazon.Core.Contracts.SystemActions;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class AddCommentInput : ISystemActionInput
    {
        public long? OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string Message { get; set; }
        public int Type { get; set; }
        public long? LinkedEmailId { get; set; }
        public long? By { get; set; }
    }
}
