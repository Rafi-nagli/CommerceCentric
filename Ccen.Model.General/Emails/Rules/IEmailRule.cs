using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;

namespace Amazon.Model.Implementation.Emails.Rules
{
    public interface IEmailRule
    {
        void Process(IUnitOfWork db, EmailReadingResult emailResult);
    }
}
