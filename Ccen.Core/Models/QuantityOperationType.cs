using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{    
    public enum QuantityOperationType
    {
        None = 0,
        Exchange = 1,
        Return = 2,
        Replacement = 3,
        Lost = 4,
        Damaged = 5,
        [Description("Invalid box")]
        InvalidBox = 6,
        [Description("Sold outside")]
        SoldOutside = 7,
        [Description("Compensation gift")]
        CompensationGift = 8,
        [Description("Return on Hold")]
        ReturnOnHold = 9,
        Wholesale = 10,
        [Description("Store manual")]
        StoreManual = 11,
        [Description("From Mailing page")]
        FromMailPage = 12,
        [Description("Exchange on Hold")]
        ExchangeOnHold = 20,

        [Description("Add Manually")]
        AddManually = 25,

        [Description("In pending when inventory")]
        InPendingWhenInventory = 50,
        [Description("Sale event returns")]
        SaleEventReturns = 200,
    }
}
