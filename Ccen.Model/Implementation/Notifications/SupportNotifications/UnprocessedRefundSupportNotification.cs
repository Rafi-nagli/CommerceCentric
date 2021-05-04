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
    public class UnprocessedRefundSupportNotification : ISupportNotification
    {
        public string Name { get { return "Unprocessed refunds"; } }

        private IDbFactory _dbFactory;
        private IEmailService _emailService;
        private ILogService _log;
        private ITime _time;

        public IList<TimeSpan> When
        {
            get { return new List<TimeSpan>() { TimeSpan.FromHours(9) }; }
        }

        public UnprocessedRefundSupportNotification(IDbFactory dbFactory,
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
            using (var db = _dbFactory.GetRWDb())
            {
                var fromDate = _time.GetAppNowTime().AddMonths(-2);

                var query = from a in db.SystemActions.GetAll()
                            where a.Type == (int)SystemActionType.UpdateOnMarketReturnOrder
                                && (a.Status == (int)SystemActionStatus.Fail
                                    || a.Status == (int)SystemActionStatus.None)
                                && a.CreateDate >= fromDate
                            orderby a.CreateDate ascending
                            select a;

                var refundActions = query.ToList();

                if (refundActions.Any())
                {
                    var messages = new List<string>();
                    foreach (var action in refundActions)
                    {
                        var input = JsonConvert.DeserializeObject<ReturnOrderInput>(action.InputData);
                        var amount = input.Items != null ?
                            input.Items.Sum(i => i.RefundItemPrice + (input.IncludeShipping ? i.RefundShippingPrice : 0) - (input.DeductShipping ? i.DeductShippingPrice : 0))
                            : (decimal?)null;
                        var status = action.Status;
                        var date = action.CreateDate;
                        var orderNumber = input.OrderNumber;

                        messages.Add(orderNumber + " - $" + amount + " - " + DateHelper.ToDateTimeString(date));
                    }

                    var messageText = String.Join("<br/>", messages);

                    _log.Info("Unprocessed refunds: " + messageText);
                    _emailService.SendSystemEmail("Unprocessed refunds",
                        "The following refunds have \"failed\"\"awaiting\" statuses:<br/> " + messageText,
                        EmailHelper.RafiEmail + ", " + EmailHelper.RaananEmail, EmailHelper.SupportDgtexEmail + ", " + EmailHelper.IldarDgtexEmail);
                }
                else
                {
                    _emailService.SendSystemEmailToAdmin("Unprocessed refunds - Success",
                        "There are no unprocessed refunds");
                }
            }
        }
    }
}
