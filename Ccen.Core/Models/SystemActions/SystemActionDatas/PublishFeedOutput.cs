using System;
using Amazon.Core.Contracts.SystemActions;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class PublishFeedOutput : ISystemActionOutput
    {
        public bool? IsSuccess { get; set; }
        public int ProgressPercent { get; set; }
        public string Message { get; set; }
        public int AttemptNumber { get; set; }
        public DateTime? AttemptDate { get; set; }

        public int? ParsedCount { get; set; }
        public int? MatchedCount { get; set; }
        public int? Valid1OperationCount { get; set; }
        public int? Valid2OperationCount { get; set; }
        public int? Processed1OperationCount { get; set; }
        public int? Processed2OperationCount { get; set; }
    }
}
