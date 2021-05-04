using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Stamps
{
    public enum DeliveredStatusEnum
    {
        None = 0,
        Delivered = 1,
        DeliveredToSender = 2,
        NotDelivered = 3,
        InfoUnavailable = 10,
    }
}
