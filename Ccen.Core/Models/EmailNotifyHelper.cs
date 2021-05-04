using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public class EmailNotifyHelper
    {
        public static OrderEmailNotifyType GetEmailNotifyFrom(EmailTypes emailType)
        {
            switch (emailType)
            {
                case EmailTypes.RequestFeedback:
                    return OrderEmailNotifyType.OutputSendFeedbackEmail;
                case EmailTypes.AddressVerify:
                    return OrderEmailNotifyType.OutputUnverifiedAddressEmail;
                case EmailTypes.AddressNotServedByUSPS:
                    return OrderEmailNotifyType.OutputAddressNotServedByUSPSEmail;

                case EmailTypes.AddressChanged:
                    return OrderEmailNotifyType.OutputAddressChanged;
                case EmailTypes.AddressNotChanged:
                    return OrderEmailNotifyType.OutputAddressNotChanged;
                
                case EmailTypes.Duplicate:
                    return OrderEmailNotifyType.OutputDuplicateAlertEmail;
                case EmailTypes.LostPackage:
                    return OrderEmailNotifyType.OutputLostPackageEmail;
                case EmailTypes.LostPackage2:
                    return OrderEmailNotifyType.OutputLostPackage2Email;
                case EmailTypes.UndeliverableInquiry:
                    return OrderEmailNotifyType.OutputUndeliverableInquiryEmail;
                case EmailTypes.BusinessDays18to21:
                    return OrderEmailNotifyType.OutputBusinessDays18to21Email;
                case EmailTypes.OrdersWithDropshipAndReadyEmailInfo:
                    return OrderEmailNotifyType.OutputOrdersWithDropshipAndReadyEmailInfoEmail;                    

                case EmailTypes.SignConfirmationRequest:
                    return OrderEmailNotifyType.OutputSignConfirmationEmail;
                case EmailTypes.NoticeLeft:
                    return OrderEmailNotifyType.OutputNoticeLeft;
                case EmailTypes.ExchangeInstructions:
                    return OrderEmailNotifyType.OutputExchangeInstructionsEmail;
                case EmailTypes.ReturnInstructions:
                    return OrderEmailNotifyType.OutputReturnInstructionsEmail;
                case EmailTypes.DamagedItem:
                    return OrderEmailNotifyType.OutputDamagedItemEmail;
                case EmailTypes.ReturnPeriodExpired:
                    return OrderEmailNotifyType.OutputReturnPeriodExpired;
                case EmailTypes.NotOurs:
                    return OrderEmailNotifyType.OutputNotOursEmail;
                case EmailTypes.ReturnWrongDamagedItem:
                    return OrderEmailNotifyType.OutputReturnWrongDamagedItemEmail;
                case EmailTypes.IncompleteName:
                    return OrderEmailNotifyType.OutputIncompleteNameEmail;
                case EmailTypes.PhoneMissing:
                    return OrderEmailNotifyType.OutputPhoneMissingEmail;
                case EmailTypes.UndeliverableAsAddressed:
                    return OrderEmailNotifyType.OutputUndeliveredAsAddressedEmail;
                case EmailTypes.AlreadyShipped:
                    return OrderEmailNotifyType.OutputAlreadyShippedEmail;
                case EmailTypes.Oversold:
                    return OrderEmailNotifyType.OutputOversoldEmail;
                case EmailTypes.GiftReceipt:
                    return OrderEmailNotifyType.OutputGiftEmail;

                case EmailTypes.AcceptOrderCancellationToBuyer:
                    return OrderEmailNotifyType.OutputAcceptedOrderCancelledEmailToBuyer;
                case EmailTypes.RejectOrderCancellationToBuyer:
                    return OrderEmailNotifyType.OutputRejectedOrderCancelledEmailToBuyer;
                case EmailTypes.AcceptOrderCancellationToSeller:
                    return OrderEmailNotifyType.OutputAcceptedOrderCancelledEmail;
                case EmailTypes.AcceptRemoveSignConfirmation:
                    return OrderEmailNotifyType.OutputAcceptedRemoveSignConfirmationEmail;
                case EmailTypes.RejectRemoveSignConfirmation:
                    return OrderEmailNotifyType.OutputRejectedRemoveSignConfirmationEmail;
                case EmailTypes.AcceptReturnRequest:
                    return OrderEmailNotifyType.OutputAcceptedReturnRequest;

                case EmailTypes.AutoResponseAmazon:
                    return OrderEmailNotifyType.OutputAutoResponseAmazon;
                case EmailTypes.AutoResponseWalmart:
                    return OrderEmailNotifyType.OutputAutoResponseWalmart;

                case EmailTypes.CustomEmail:
                    return OrderEmailNotifyType.OutputCustom;
                    
                case EmailTypes.Default:
                    return OrderEmailNotifyType.OutputCustom;
                default:
                    throw new Exception("Unsupported email type, type=" + emailType);
            }
        }
    }
}
