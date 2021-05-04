using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Enums
{
    public enum FeedStatus
    {
        None = 0,
        Submitted = 1,
        InProgress = 5,
        Processed = 10,
        ProcessedWithWarnings = 15,
        ProcessedWithErrors = 20,
        Cancelled = 50,
        SubmissionFail = 60,
    }

    public static class FeedStatusHelper
    {
        public static string ToString(FeedStatus status)
        {
            switch (status)
            {
                case FeedStatus.InProgress:
                case FeedStatus.Submitted:
                    return "Submitted";
                case FeedStatus.Processed:
                    return "Processed";
                case FeedStatus.ProcessedWithErrors:
                case FeedStatus.ProcessedWithWarnings:
                    return "Processed with errors";
            }
            return "n/a";
        }
    }
}
