using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Orders
{
    public enum OrderSyncModes
    {
        None = 0,
        Full = 1,
        Fast,
        OnlyNewFast,
        OnlyUpdateExists
    }
}
