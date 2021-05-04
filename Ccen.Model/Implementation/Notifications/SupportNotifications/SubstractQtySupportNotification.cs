using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Notifications;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Models;
using Amazon.Web.Models;
using Newtonsoft.Json;

namespace Amazon.Model.Implementation.Notifications.SupportNotifications
{
    public class SubstractQtySupportNotification : ISupportNotification
    {
        public string Name { get { return "Substract Quantity"; } }

        public IList<TimeSpan> When
        {
            get { return new List<TimeSpan>() { TimeSpan.FromHours(9) }; }
        }

        private IDbFactory _dbFactory;
        private IEmailService _emailService;
        private ILogService _log;
        private ITime _time;

        public SubstractQtySupportNotification(IDbFactory dbFactory,
            IEmailService emailService,
            ILogService log,
            ITime time)
        {
            _time = time;
            _log = log;
            _emailService = emailService;
            _dbFactory = dbFactory;
        }

        public void Check()
        {
            var fromDate = _time.GetAppNowTime().Date.AddDays(-1);

            var bodyHtml = "<table><tr><th>Style</th><th>Size</th><th>Qty Change</th><th>Operation</th><th>Date</th><th>By</th></tr>";
            var count = 0;
            using (var db = _dbFactory.GetRWDb())
            {
                var operations = (from o in db.QuantityOperations.GetAll()
                                  join och in db.QuantityChanges.GetAll() on o.Id equals och.QuantityOperationId
                                  join si in db.StyleItems.GetAll() on och.StyleItemId equals si.Id
                                  join st in db.Styles.GetAll() on si.StyleId equals st.Id
                                  join chU in db.Users.GetAll() on och.CreatedBy equals chU.Id into withUser
                                  from chU in withUser.DefaultIfEmpty()
                                  where //och.Quantity > 0 && 
                                    och.CreateDate >= fromDate
                                  select new
                                  {
                                      StyleId = st.StyleID,
                                      Size = si.Size,
                                      Qty = och.Quantity,
                                      CreateDate = och.CreateDate,
                                      Type = o.Type,
                                      ByName = chU.Name,
                                  }).ToList();
                count = operations.Count;

                foreach (var op in operations)
                {
                    bodyHtml += String.Format(@"<tr>
                        <td>{0}</td>
                        <td>{1}</td>
                        <td>{2}</td>
                        <td>{3}</td>
                        <td>{4}</td>
                        <td>{5}</td>
                    </tr>", op.StyleId,
                        op.Size,
                        -op.Qty,
                        ((QuantityOperationType)op.Type).ToString(),
                        DateHelper.ToDateTimeString(op.CreateDate),
                        op.ByName);
                }
                bodyHtml += "</table>";
            }

            if (count > 0)
            { 
                _log.Info(bodyHtml);
                _emailService.SendSystemEmail(String.Format("Quantity change notification ({0} changes)", count), 
                    bodyHtml, 
                    EmailHelper.RafiEmail, EmailHelper.SupportDgtexEmail);
            }
        }
    }
}
