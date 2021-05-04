using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum RefundReasonCodes
    {
        None = 0,
        Oversold = 5,
        Late = 10,
        Lost = 15,
        Defective = 18,
        Courtesy = 20,
        Return = 25,
        Other = 30
    }
}
