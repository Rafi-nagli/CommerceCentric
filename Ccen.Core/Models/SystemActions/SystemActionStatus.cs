using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum SystemActionStatus
    {
        None = 0,
        Done = 1,
        InProgress = 5,

        Suspended = 10,
        Skipped = 11,
        
        Fail = 503,
        NotFoundEntity = 504,
        Expired = 505
    }
}
