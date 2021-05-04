using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum EmailResponseStatusFilterEnum
    {
        None = 0,
        ResponseNeeded = 1,
        ResponseNeededDismissed = 2,
        ResponsePromised = 5,
        Escalated = 6
    }
}
