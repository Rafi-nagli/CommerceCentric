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
    public class SetSentByEmailRule : IEmailRule
    {
        private ILogService _log;
        private ITime _time;

        public SetSentByEmailRule(ILogService log,
            ITime time)
        {
            _log = log;
            _time = time;
        }

        public void Process(IUnitOfWork db, EmailReadingResult result)
        {
            if (result.Email.FolderType == (int)EmailFolders.Sent)
            {
                if (result.Headers != null && result.Headers[EmailHelper.UserNameHeadersKey] != null)
                {
                    var userId = result.Headers[EmailHelper.UserNameHeadersKey];
                    _log.Info("Headers contain userId=" + userId);
                    var user = db.Users.GetAllAsDto().FirstOrDefault(u => !u.Deleted && u.Id.ToString() == userId);
                    if (user != null)
                    {
                        var email = db.Emails.Get(result.Email.Id);
                        email.CreatedBy = user.Id;
                        db.Commit();
                    }
                }
            }
        }
    }
}
