using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum PaymentValidationStatuses
    {
        None = 0,
        Green = 1,
        Yellow = 2,
        Red = 3,
        InProgress = 100,
        NotSupport = 404
    }
}
