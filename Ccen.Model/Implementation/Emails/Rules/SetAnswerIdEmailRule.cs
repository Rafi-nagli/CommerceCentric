using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Models;

namespace Amazon.Model.Implementation.Emails.Rules
{
    public class SetAnswerIdEmailRule : IEmailRule
    {
        private ILogService _log;
        private ITime _time;

        public SetAnswerIdEmailRule(ILogService log,
            ITime time)
        {
            _log = log;
            _time = time;
        }

        public void Process(IUnitOfWork db, EmailReadingResult result)
        {
            if (result.Status == EmailMatchingResultStatus.New
                //&& result.HasMatches //NOTE: also mark w/o matches
                && result.Email.Type != (int)IncomeEmailTypes.Test
                && (EmailHelper.GetFolderType(result.Folder) == EmailFolders.Sent
                    || EmailHelper.IsAutoCommunicationAddress(result.Email.From)))
            {
                //Get by subject
                var subject = EmailHelper.GetWoReplySubject(result.Email.Subject);
                var existEmails = db.Emails
                    .GetFiltered(e => e.Subject == subject 
                        && String.IsNullOrEmpty(e.AnswerMessageID)
                        && e.FolderType == (int)EmailFolders.Inbox)
                    .ToList();
                existEmails = existEmails.Where(e => !String.IsNullOrEmpty(e.From) && e.From.Contains(result.Email.To)).ToList();

                //If no results, all unanswered with the same order ids
                if (!existEmails.Any() 
                    || (result.Email.To ?? "").Contains("relay.walmart.com")) //NOTE: For Walmart always add unanswered emails related to order; each email has own from address; so multiple equal emails would be unanswered!
                {
                    var emailIds = db.EmailToOrders
                        .GetAll()
                        .Where(eo => result.MatchedIdList.Contains(eo.OrderId))
                        .Select(eo => eo.EmailId)
                        .Distinct()
                        .ToList();
                    existEmails = db.Emails
                        .GetFiltered(e => emailIds.Contains(e.Id) 
                            && String.IsNullOrEmpty(e.AnswerMessageID)
                            && e.FolderType == (int)EmailFolders.Inbox)
                        .ToList();
                }

                if (existEmails.Any())
                {
                    foreach (var email in existEmails)
                    {
                        if (email.ReceiveDate < result.Email.ReceiveDate && email.ReceiveDate > result.Email.ReceiveDate.AddDays(-10)) //Mark Answered only previos emails, during 10 days
                        {
                            _log.Info("Mark as answered, emailId=" + email.Id + ", subject=" + email.Subject);
                            email.AnswerMessageID = result.Email.MessageID;
                            if (email.ResponseStatus != (int) EmailResponseStatusEnum.ResponsePromised)
                                email.ResponseStatus = (int) EmailResponseStatusEnum.ReceivedFromMailbox;
                        }
                    }
                    db.Commit();
                }
                else
                {
                    _log.Info("No emails to mark as answered");
                }
            }
        }
    }
}
