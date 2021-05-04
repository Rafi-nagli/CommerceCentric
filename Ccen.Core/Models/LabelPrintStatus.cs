using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum LabelPrintStatus
    {
        None = 0,
        Printed = 1,
        OnHold = 5,
        AlreadyMailed = 10,
        PrintError = 503,
    }
}
