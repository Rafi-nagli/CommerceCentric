using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Emails;
using Amazon.Core.Models;

namespace Amazon.Model.Implementation.Emails.Rules
{
    public class SetSystemTypesEmailRule : IEmailRule
    {
        private ILogService _log;
        private ITime _time;

        public SetSystemTypesEmailRule(ILogService log,
            ITime time)
        {
            _log = log;
            _time = time;
        }

        public void Process(IUnitOfWork db, EmailReadingResult result)
        {
            result.Email.FolderType = (int)EmailHelper.GetFolderType(result.Folder);

            var type = IncomeEmailTypes.Default;
            if (result.Email.From.Contains("auto-communication@amazon"))
                type = IncomeEmailTypes.SystemAutoCopy;

            //Ex: FW: Return authorization request for order # 112-7446790-9716267
            if (result.Email.From.Contains("order-update@amazon"))
            {
                if (result.Email.Subject != null &&
                    (result.Email.Subject.StartsWith("Return authorization", StringComparison.InvariantCultureIgnoreCase)
                     || result.Email.Subject.StartsWith("Return authorisation", StringComparison.InvariantCultureIgnoreCase)))
                    type = IncomeEmailTypes.ReturnRequest;
                else
                    type = IncomeEmailTypes.System;
            }

            if (result.Email.From.Contains("Your Immediate Response Required: Charge Dispute Inquiry"))
            {
                type = IncomeEmailTypes.SystemNotification;
            }

            if (result.Email.From.Contains("order-update@amazon"))
            {
                if (StringHelper.ContainsNoCase(result.Email.Subject,
                    "Action Required: Unshipped Prime orders that need to be shipped today"))
                {
                    type = IncomeEmailTypes.SystemNotification;
                }
            }

            //Your Immediate Response Required: Charge Dispute Inquiry

            if (result.Email.From.Contains("seller-answers@amazon"))
            {
                type = IncomeEmailTypes.System;
            }

            if (result.Email.From.Contains("seller-guarantee@amazon.com")
                || (result.Email.Subject != null
                    && StringHelper.ContainsNoCase(result.Email.Subject, "A-to-z Guarantee Claim for Order")))
            {
                type = IncomeEmailTypes.AZClaim;
            }

            if (result.Email.From.Contains("atoz-guarantee-no-reply@amazon.com")
                && (result.Email.Subject != null
                    && (result.Email.Subject.StartsWith("Action required: Claim received on order", StringComparison.InvariantCultureIgnoreCase)
                    || StringHelper.ContainsNoCase(result.Email.Subject, "Claim received on order"))))
            {
                type = IncomeEmailTypes.AZClaim;
            }

            if (result.Email.From.Contains("atoz-guarantee-no-reply@amazon.com")
                && (result.Email.Subject != null
                    && (result.Email.Subject.StartsWith("Claim Decision on Order ", StringComparison.InvariantCultureIgnoreCase))))
            {
                type = IncomeEmailTypes.System;
            }

            if (result.Email.From.Contains("do-not-reply@amazon.com")
                && (result.Email.Subject != null
                    && result.Email.Subject.StartsWith("Refund initiated for order", StringComparison.InvariantCultureIgnoreCase)))
            {
                type = IncomeEmailTypes.SystemNotification;
            }

            //if (result.Email.From.Contains("ebay@ebay.com"))
            //NOTE: Need to exclude return request
            if (result.Email.From.Contains("ebay@ebay.com")
                && result.Email.Subject != null
                && (result.Email.Subject.StartsWith("Your eBay listing is confirmed:", StringComparison.InvariantCultureIgnoreCase)
                    || result.Email.Subject.StartsWith("Your eBay item sold!", StringComparison.InvariantCultureIgnoreCase)))
            {
                type = IncomeEmailTypes.SystemNotification;
            }

            if (result.Email.Subject != null &&
                result.Email.Subject.Contains(".csv") &&
                //result.Email.To == "billing@premiumapparel.com" &&
                result.Email.Attachments != null &&
                result.Email.Attachments.Count > 0)
            {
                type = IncomeEmailTypes.DhlInvoice;
            }

            if (result.Email.Subject != null &&
                (result.Email.Subject.StartsWith("Order cancellation request from Amazon customer", StringComparison.InvariantCultureIgnoreCase)
                || result.Email.Subject.StartsWith("Solicitud de cancelación de pedido del cliente de Amazon", StringComparison.InvariantCultureIgnoreCase)))
                type = IncomeEmailTypes.CancellationRequest;

            if (result.Email.Subject != null
                && result.Email.FolderType == (int)EmailFolders.Sent
                && (result.Email.Subject.StartsWith("Test", StringComparison.InvariantCultureIgnoreCase)
                || result.Email.Subject.StartsWith("[Test", StringComparison.InvariantCultureIgnoreCase)
                || result.Email.Subject.StartsWith("Launch", StringComparison.InvariantCultureIgnoreCase)
                || result.Email.Subject.StartsWith("Print", StringComparison.InvariantCultureIgnoreCase)
                || result.Email.Subject.StartsWith("[System", StringComparison.InvariantCultureIgnoreCase)
                || StringHelper.ContainsNoCase(result.Email.Subject, "commercentric.com]")))
                type = IncomeEmailTypes.Test;
            
            result.Email.Type = (int) type;
            
            var email = db.Emails.Get(result.Email.Id);
            email.FolderType = result.Email.FolderType;
            email.Type = result.Email.Type;
            db.Commit();
        }
    }
}
