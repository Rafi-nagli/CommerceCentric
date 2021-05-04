using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum DhlInvoiceStatusEnum
    {
        None = 0,

        //esli всё сошлош - status = matched
        Matched = 1,

        //first it will be "Incorrect", then I can set it to "DHL Notified", then "refund approved", or "rejected"
        Incorrect = 5,
        DhlNotified = 10,
        RefundApproved = 15,
        Rejected = 20,

        OrderNotFound = 404,
    }
}
