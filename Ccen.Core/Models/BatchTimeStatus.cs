using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum BatchTimeStatus
    {
        BeforeFirst = 0,
        AfterFirstBeforeSecond = 1,
        AfterSecond = 2,
    }
}
