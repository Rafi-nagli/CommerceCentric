using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum OrderEmailNotifyType
    {
        OutputUnverifiedAddressEmail = 1,
        OutputSignConfirmationEmail = 2,
        OutputDuplicateAlertEmail = 3,

        OutputAddressNotServedByUSPSEmail = 5,

        OutputAddressChanged = 7,
        OutputAddressNotChanged = 8,

        OutputIncompleteNameEmail = 10,
        OutputPhoneMissingEmail = 11,

        OutputSendFeedbackEmail = 20,
        OutputLostPackageEmail = 21,
        OutputLostPackage2Email = 22,

        OutputUndeliverableInquiryEmail = 25,

        OutputBusinessDays18to21Email = 26,
        OutputOrdersWithDropshipAndReadyEmailInfoEmail = 27,

        OutputNoticeLeft = 31,

        OutputUndeliveredAsAddressedEmail = 40,

        OutputAlreadyShippedEmail = 42,
        OutputOversoldEmail = 43,
        OutputGiftEmail = 44,

        OutputExchangeInstructionsEmail = 45,
        OutputReturnInstructionsEmail = 46,

        OutputDamagedItemEmail = 48,
        OutputReturnWrongDamagedItemEmail = 49,
        OutputReturnPeriodExpired = 490,

        OutputCustom = 50,

        OutputAutoResponseAmazon = 51,
        OutputAutoResponseWalmart = 52,

        OutputNotOursEmail = 55,

        OutputAcceptedRemoveSignConfirmationEmail = 101,
        OutputAcceptedOrderCancelledEmail = 102,
        OutputAcceptedOrderCancelledEmailToBuyer = 103,
        OutputAcceptedReturnRequest = 104,

        OutputRejectedRemoveSignConfirmationEmail = 201,
        OutputRejectedOrderCancelledEmailToBuyer = 203,
        
        InputRemoveSignConfirmationEmail = 1001,
        InputOrderCancelledEmail = 1002,

        OutputNoWeightEmailToSeller = 2001,

        //TEMP
        Automation1MaskNotification = 100001
    }
}
