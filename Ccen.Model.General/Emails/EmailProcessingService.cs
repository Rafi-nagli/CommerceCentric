using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.Implementation.Emails.Rules;
using Amazon.Model.Models.EmailInfos;
using Org.BouncyCastle.Asn1.Cms;

namespace Amazon.Model.Implementation.Emails
{
    public class EmailProcessingService
    {
        private IDbFactory _dbFactory;
        private ILogService _log;
        private IEmailService _emailService;
        private ISystemActionService _systemAction;
        private ITime _time;
        private CompanyDTO _company;

        public EmailProcessingService(ILogService log,
            IDbFactory dbFactory,
            IEmailService emailService,
            ISystemActionService systemAction,
            CompanyDTO company,
            ITime time)
        {
            _log = log;
            _time = time;
            _emailService = emailService;
            _dbFactory = dbFactory;
            _systemAction = systemAction;
            _company = company;
        }


        public void ProcessEmails(IList<EmailReadingResult> matchingResults, IList<IEmailRule> rules)
        {
            _log.Info("Begin ProcessEmails by rules, ruleCount=" + rules.Count());

            var index = 0;
            foreach (var emailResult in matchingResults)
            {                
                foreach (var rule in rules)
                {
                    try
                    {
                        if (index % 10 == 0)
                            Thread.Sleep(10000);

                        using (var db = _dbFactory.GetRWDb())
                        {
                            rule.Process(db, emailResult);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Info("Error when processing email rule", ex);
                    }
                }
                index++;
            }
        }
        
    }
}
