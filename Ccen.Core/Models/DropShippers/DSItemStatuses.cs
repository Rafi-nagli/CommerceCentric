using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.DropShippers
{
    public enum DSItemStatuses
    {
        Inactive = 0,
        Active = 1,
        Deleted = 20,
        DeletedByFullFeed = 21,
        DeletedByDailyFeed = 22,
    }
}
