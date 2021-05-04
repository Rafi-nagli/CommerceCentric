using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum IncomeEmailTypes
    {
        Default = 0,
        ReturnRequest = 1,
        System = 2,
        
        CancellationRequest = 5,
        DhlInvoice = 6,
        SystemAutoCopy = 10,

        SystemNotification = 15,

        AZClaim = 31,

        Test = 100
    }
}
