using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum EmailTypes
    {
        Default = 0,
        RequestFeedback = 10,
        AddressVerify = 20,
        AddressNotServedByUSPS = 21,
        AddressChanged = 25,
        AddressNotChanged = 26,

        Duplicate = 30,
        LostPackage = 40,
        LostPackage2 = 42,
        UndeliverableInquiry = 45,
        BusinessDays18to21 = 46,
        OrdersWithDropshipAndReadyEmailInfo = 47,

        SignConfirmationRequest = 50,
        NoticeLeft = 60,
        IncompleteName = 70,
        PhoneMissing = 71,
        UndeliverableAsAddressed = 80,
        AlreadyShipped = 85,
        Oversold = 86,
        GiftReceipt = 87,

        ExchangeInstructions = 90,
        ReturnInstructions = 91,

        OrdersWithDropshipAndReady = 93,

        DamagedItem = 95,
        ReturnWrongDamagedItem = 97,
        ReturnPeriodExpired = 98,
        RefundPeriodExpired = 99,
        NotOurs = 101,
        
        AcceptOrderCancellationToBuyer = 200,
        RejectOrderCancellationToBuyer = 201,
        AcceptOrderCancellationToSeller = 210,
        AcceptRemoveSignConfirmation = 220,
        RejectRemoveSignConfirmation = 230,
        AcceptReturnRequest = 240,

        System = 500,
        CustomEmail = 1000,
        RawEmail = 1010,
        AutoResponseAmazon = 1020,
        AutoResponseWalmart = 1030,

        NoWeightToSeller = 2001,
        DhlPickupScheduleErrorToSeller = 2010,
        DhlInvoice = 2011,
        SmsEmail = 2020,
    }
}
