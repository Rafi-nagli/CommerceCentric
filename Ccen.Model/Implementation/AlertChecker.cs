using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Search;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO.Users;
using Amazon.Model.Implementation.Labels;
using Amazon.Model.Models;
using Amazon.Model.Models.EmailInfos;
using Newtonsoft.Json;

namespace Amazon.Model.Implementation
{
    public class AlertChecker
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private IEmailService _emailService;
        private CompanyDTO _company;

        public AlertChecker(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            IEmailService emailService,
            CompanyDTO company)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _emailService = emailService;
            _company = company;
        }

        public void CheckUnprocessedRefunds()
        {
            using (var db = _dbFactory.GetRWDb())
            { 
                var fromDate = _time.GetAppNowTime().AddMonths(-1);

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
                        "The following refunds have \"failed\" status:<br/> " + messageText,
                        EmailHelper.RafiEmail, EmailHelper.SupportDgtexEmail + ", " + EmailHelper.IldarDgtexEmail);
                }
                else
                {
                    _emailService.SendSystemEmailToAdmin("Unprocessed refunds - Success",
                        "There are no unprocessed refunds");
                }
            }
        }

        public void CheckSameDay(IUnitOfWork db)
        {
            var originalSameDaysOrderCount = db.Orders.GetAll()
                .Count(o => o.OrderStatus == OrderStatusEnumEx.Unshipped
                    && o.InitialServiceType == ShippingUtils.SameDayServiceName);

            _log.Info("Same Day orders, original count=" + originalSameDaysOrderCount);

            if (originalSameDaysOrderCount > 0)
            {
                var queryTotalSameDayOrderIdList = from o in db.Orders.GetAll()
                    join sh in db.OrderShippingInfos.GetAll()
                        on o.Id equals sh.OrderId
                    where o.OrderStatus == OrderStatusEnumEx.Unshipped
                          && sh.ShippingMethodId == ShippingUtils.DynamexPTPSameShippingMethodId
                    select o.Id;

                var totalSameDaysOrderCount = queryTotalSameDayOrderIdList.Distinct().Count();
                _log.Info("Same Day orders, total count=" + totalSameDaysOrderCount);

                var message = String.Format("{0} Same days orders are not processed", totalSameDaysOrderCount);

                _emailService.SendSmsEmail(message,
                    _company.SellerAlertEmail);

                _emailService.SendSystemEmailToAdmin(message, message);
            }
        }

        public void CheckOverdue(IUnitOfWork db, 
            DateTime when,
            TimeSpan autoPrintTime)
        {
            var overdueOrdersInfoes = LabelAutoBuyService.GetOverdueOrderInfos(db, when);

            _log.Info("Overdue orders count=" + overdueOrdersInfoes.Count);
            _log.Info("Overdue orders: " + String.Join(", ", overdueOrdersInfoes.Select(o => o.OrderId).ToList()));

            if (overdueOrdersInfoes.Count > 5)
            {
                var message = String.Format("{0}: Total/In batches: {1}/{2} orders are not processed and will be overdue. Auto purchase at: {3}", 
                    PortalEnum.PA.ToString(),
                    overdueOrdersInfoes.Count,
                    overdueOrdersInfoes.Where(i => i.BatchId.HasValue).Count(),
                    autoPrintTime.ToString("hh\\:mm"));

                _emailService.SendSmsEmail(message,
                    _company.SellerAlertEmail);

                _emailService.SendSystemEmailToAdmin(message, message);
                _emailService.SendSystemEmail(message, message, "ildar@dgtex.com", null, null);
            }
        }

        public bool CheckDhlInvoice(IUnitOfWork db, DateTime? lastSendDate, DateTime when)
        {
            var lastInvoiceDate = db.DhlInvoices.GetAllAsDto().Max(i => (DateTime?)i.InvoiceDate);

            _log.Info("Last invoice date=" + lastInvoiceDate + ", last send date=" + lastSendDate);

            var fromDate = when.AddDays(-10);

            var dhlOrderCount = db.Orders.GetAll().Where(o => o.ShipmentProviderType == (int)ShipmentProviderType.Dhl
                && o.OrderStatus == OrderStatusEnumEx.Shipped
                && o.OrderDate > fromDate).Count();

            _log.Info("Order Count: " + dhlOrderCount);
            if (dhlOrderCount == 0)
            {
                return false;
            }

            //давай ждать еще 3 дня и посылать notification
            //to est up 5 days after invoice was expected
            var expectDate = (lastInvoiceDate ?? when).AddDays(10);

            if (expectDate <= when 
                && (!lastSendDate.HasValue || lastSendDate.Value.AddDays(10) <= when))
            {
                var startWeek = DateHelper.GetStartOfWeek((lastInvoiceDate ?? when).AddDays(1));
                var endWeek = DateHelper.GetEndOfWeek((lastInvoiceDate ?? when).AddDays(1));
                var emailInfo = new DhlInvoiceEmailInfo(_emailService.AddressService,
                    startWeek,
                    endWeek,
                    EmailHelper.DhlInvoiceRecipient,
                    EmailHelper.RafiEmail + "; " + EmailHelper.SupportDgtexEmail);
                _emailService.SendEmail(emailInfo, CallSource.Service);

                var message = "Dhl Invoice expected";
                _emailService.SendSystemEmailToAdmin(message, message);

                return true;
            }

            return false;
        }
    }
}
