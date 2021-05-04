using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Enums
{
    public enum ReportProcessingResultType
    {
        Success = 0,
        Cancelled = 5,
        MaxAttampts = 10
    }
}
