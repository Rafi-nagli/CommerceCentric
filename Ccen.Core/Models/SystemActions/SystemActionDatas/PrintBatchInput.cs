using Amazon.Core.Contracts.SystemActions;
using Amazon.Core.Helpers;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class PrintBatchInput : ISystemActionInput
    {
        public long BatchId { get; set; }
        public long CompanyId { get; set; }
        public long? UserId { get; set; }

        public override string ToString()
        {
            return ToStringHelper.ToString(this);
        }
    }
}
