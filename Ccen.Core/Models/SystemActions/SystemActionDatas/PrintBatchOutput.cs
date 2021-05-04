using System.Collections.Generic;
using Amazon.Core.Contracts.SystemActions;
using Amazon.Core.Helpers;
using Amazon.DTO.Orders;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class PrintBatchOutput : ISystemActionOutput
    {
        public bool IsProcessed { get; set; }
        public IList<Message> Messages { get; set; }
        public string FilePath { get; set; }
        public long? PrintPackId { get; set; }
        public ScheduledPickupDTO PickupInfo { get; set; }

        public override string ToString()
        {
            return ToStringHelper.ToString(this);
        }
    }
}
