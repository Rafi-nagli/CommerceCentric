using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum RetryModeType
    {
        None,
        Normal,
        Progressive,
        Random,
        Fast
    }
}
