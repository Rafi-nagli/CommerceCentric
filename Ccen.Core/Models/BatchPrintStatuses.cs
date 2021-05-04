using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum BatchPrintStatuses
    {
        None = 0,
        Printed = 1,
        InProgress = 5,

        PrintError = 503,
    }
}
