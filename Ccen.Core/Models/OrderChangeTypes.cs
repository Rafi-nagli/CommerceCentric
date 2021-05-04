using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum OrderChangeTypes
    {
        None = 0,
        Create = 1,
        StatusChanged = 10,

        Hold = 20,
        UnHold = 21,
        DismissAddressWarn = 25,
        ChangeAddress = 26,
        ChangeShippingMethod = 27,
        ChangeShippingProvider = 28,
        RatesRecalculated = 29,


        AddToBatch = 30,
        RemoveFromBatch = 31,
        PrintLabel = 32,
        FirstScan = 33,
        Delivered = 34,

        DSChanged = 35,

        NewReturnRequest = 40,
        MadeRefund = 41,
        CancelationRequested = 45,

        IncomeEmail = 50,
        SendEmail = 51,
        EmailStatusChanged = 55,

        NewComment = 60,
    }
}
