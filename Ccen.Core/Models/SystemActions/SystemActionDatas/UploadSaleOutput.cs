using System;
using Amazon.Core.Contracts.SystemActions;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class UploadSaleOutput : ISystemActionOutput
    {
        public bool? IsSuccess { get; set; }
        public int ProgressPercent { get; set; }
        public string Message { get; set; }
        public int AttemptNumber { get; set; }
        public DateTime? AttemptDate { get; set; }
    }
}
