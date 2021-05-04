using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;

namespace Amazon.Model.Implementation.Emails.Rules
{
    public class PrepareBodyEmailRule : IEmailRule
    {
        private ILogService _log;
        private ITime _time;

        public PrepareBodyEmailRule(ILogService log,
            ITime time)
        {
            _log = log;
            _time = time;
        }

        public void Process(IUnitOfWork db, EmailReadingResult result)
        {
            var wasModified = false;
            var body = EmailHelper.CutOutBody(result.Email.Message, out wasModified);

            if (wasModified)
            {
                _log.Info("Body was updated, emailId=" + result.Email.Id);

                var email = db.Emails.Get(result.Email.Id);
                email.Message = body;
                db.Commit();
            }

            //if (result.Email.From.Contains("ebay.com"))
            //{
            //    RegexOptions options = RegexOptions.None;
            //    Regex regex = new Regex("(<br/>|<br>){2,}", options);
            //    var ebayBody = regex.Replace(result.Email.Message, " ");

            //    var email = db.Emails.Get(result.Email.Id);
            //    email.Message = ebayBody;
            //    db.Commit();

            //    _log.Info("Prepared eBay body, emailId=" + email.Id);
            //}
        }
    }
}
