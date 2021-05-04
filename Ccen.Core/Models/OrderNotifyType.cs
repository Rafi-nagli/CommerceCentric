using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum OrderNotifyType
    {
        PrintLabel = 1,
        CalcRate = 2,
        BlackList = 10,
        Duplicate = 11,
        
        FutureShipping = 12,
        InternationalExpress = 13,
        SameDay = 14,

        AddressCheckStamps = 20,
        AddressCheckMelissa = 21,
        AddressCheckWithPerviousCorrection = 22,
        AddressCheckGoogleGeocode = 23,
        AddressCheckFedex = 24,
        OverchargedShpppingCost = 50,
        OversoldItem = 55,
        OversoldOnHoldItem = 56,

        CancellationRequest = 70,

        MarketComment = 80,

        SendConfirmationEmail = 110,
        Escalated = 120

    }
}
