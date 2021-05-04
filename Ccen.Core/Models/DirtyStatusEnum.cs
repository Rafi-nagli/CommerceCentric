using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum DirtyStatusEnum
    {
        None = 0,
        DirectChanges = 1,
        LinkedChanges = 2,
        RestockDateChanges = 3,
        OnHoldChanges = 4,
        UnpublishChanges = 5,
    }
}
