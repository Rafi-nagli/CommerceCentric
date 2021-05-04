using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum MailLabelReasonCodes
    {
        None = 0,

        ReturnLabelReasonCode = 2,
        ResendingOrderCode = 1,
        ExchangeCode = 4,
        OtherCode = 5,

        ReplacementLabelCode = 8,
        ManualLabelCode = 9,
        ReplacingLostDamagedReasonCode = 10,

        RefundCode = 15,
    }
}
