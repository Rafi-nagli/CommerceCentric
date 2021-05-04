using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class RefundStatusSupportNotification : ISupportNotification
    {
        public string Name { get { return "Refunds Status"; } }

        private IDbFactory _dbFactory;
        private IEmailService _emailService;
        private ILogService _log;
        private ITime _time;

        public IList<TimeSpan> When
        {
            get { return new List<TimeSpan>() { TimeSpan.FromHours(3) }; }
        }

        public RefundStatusSupportNotification(IDbFactory dbFactory,
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
            var warningPeriod = DateTime.Now.Subtract(TimeSpan.FromHours(48));
            var minDate = DateTime.Now.AddMonths(-3);

            var messages = new List<string>();
            using (var db = _dbFactory.GetRWDb())
            {
                var actions = db.SystemActions.GetAllAsDto()
                        .Where(a => a.Type == (int) SystemActionType.UpdateOnMarketReturnOrder 
                            //&& (a.Status == (int) SystemActionStatus.Fail //NOTE: all statuses, in case some of action isn't process
                            && a.Status != (int) SystemActionStatus.Done
                            && a.CreateDate < warningPeriod
                            && a.CreateDate > minDate)
                        .ToList();

                foreach (var action in actions)
                {
                    var data = JsonConvert.DeserializeObject<ReturnOrderInput>(action.InputData);
                    DTOOrder order = null;
                    if (data.OrderId > 0)
                        order = db.Orders.GetAllAsDto().FirstOrDefault(o => o.Id == data.OrderId);
                    if (order == null)
                        order = db.Orders.GetAllAsDto().FirstOrDefault(o => o.OrderId == data.OrderNumber);

                    if (order == null)
                    {
                        _log.Info("Unable to find order: orderId=" + data.OrderId + ", orderNumber=" + data.OrderNumber);
                        continue;
                    }

                    var requestedAmount = data.Items.Sum(i => i.RefundItemPrice
                        + (data.IncludeShipping ? i.RefundShippingPrice : 0)
                        - (data.DeductShipping ? i.DeductShippingPrice : 0)
                        - (data.IsDeductPrepaidLabelCost ? i.DeductPrepaidLabelCost : 0));

                    messages.Add("Order #: " + order.OrderId
                                 + ", market: " + MarketHelper.GetMarketName(order.Market, order.MarketplaceId)
                                 + ", status: " + SystemActionHelper.GetName((SystemActionStatus)action.Status)
                                 + ", amount: " + requestedAmount 
                                 + ", create date: " + action.CreateDate);
                }
            }

            if (messages.Any())
            { 
                _log.Info(String.Join("\r\n", messages));
                _emailService.SendSystemEmailToAdmin("Support Notification: " + Name + " - To Review (" + messages.Count() + ")",
                    "Unprocessed refunds:<br/>" + String.Join("<br/>", messages));
            }
            else
            {
                _emailService.SendSystemEmailToAdmin("Support Notification: " + Name + " - Success", "");
            }
        }
    }
}
