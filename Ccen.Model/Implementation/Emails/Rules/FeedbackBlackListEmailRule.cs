using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;

namespace Amazon.Model.Implementation.Emails.Rules
{
    public class FeedbackBlackListEmailRule : IEmailRule
    {
        private ILogService _log;
        private ITime _time;

        public FeedbackBlackListEmailRule(ILogService logService,
            ITime time)
        {
            _log = logService;
            _time = time;
        }

        public void Process(IUnitOfWork db, EmailReadingResult result)
        {
            var orderIdList = new List<string>();

            if (result.MatchedIdList != null)
                orderIdList.AddRange(result.MatchedIdList);

            orderIdList = orderIdList.Distinct().ToList();

            foreach (var orderId in orderIdList)
            {
                var existDb = db.FeedbackBlackLists.GetAll().FirstOrDefault(o => o.OrderId == orderId);
                if (existDb == null)
                {
                    _log.Debug("Add to black list orderId=" + orderId);
                    db.FeedbackBlackLists.Add(new FeedbackBlackList()
                    {
                        OrderId = orderId,
                        Reason = "[Auto] Has related email messages",
                        CreateDate = _time.GetAppNowTime()
                    });
                }
            }
            db.Commit();
        }
    }
}
