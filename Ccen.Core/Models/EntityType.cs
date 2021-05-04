using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum EntityType
    {
        Order = 1,
        Tracking = 5,
        Label = 5,
        Listing = 10,
        Item = 11,
        ParentItem = 12,
    }
}
