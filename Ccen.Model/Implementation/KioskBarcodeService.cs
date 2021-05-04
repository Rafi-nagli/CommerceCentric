using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;

namespace Amazon.Model.Implementation
{
    public class KioskBarcodeService
    {
        private IDbFactory _dbFactory;
        private IEmailService _emailService;
        private ITime _time;
        private ILogService _log;

        public KioskBarcodeService(IDbFactory dbFactory,
            IEmailService emailService,
            ITime time,
            ILogService log)
        {
            _time = time;
            _emailService = emailService;
            _dbFactory = dbFactory;
            _log = log;
        }

        public void CheckOrders()
        {
            var from = _time.GetAppNowTime().AddDays(-2);

            using (var invDb = _dbFactory.GetInventoryRWDb())
            {
                var ordersToCheck = invDb.Orders.GetAll().Where(o => o.OrderDate > from
                    && !o.LastCheckedDate.HasValue).ToList();

                foreach (var order in ordersToCheck)
                {
                    if (order.OrderDate < _time.GetUtcTime().AddHours(-2))
                    {
                        _log.Info("Checking order: " + order.OrderDate);
                        using (var db = _dbFactory.GetRDb())
                        {
                            var items = db.Scanned.GetScanItemAsDto()
                                .Where(i => i.OrderId == order.Id)
                                .OrderBy(i => i.Barcode)
                                .ToList();
                            var woStyleItemIds = items.Where(i => !i.StyleItemId.HasValue).ToList();

                            if (woStyleItemIds.Any())
                            {
                                _log.Info("Send notification, orderDate=" + order.OrderDate
                                          + ", barcodes=" + woStyleItemIds.Count);

                                SendWoStyleNotification(order.OrderDate,
                                    order.Description,
                                    woStyleItemIds.Select(i => i.Barcode).ToList());
                            }
                            else
                            {
                                _log.Info("All barcodes have styles");
                            }

                            order.LastCheckedDate = _time.GetAppNowTime();
                            invDb.Commit();
                        }
                    }
                }
            }
        }

        private void SendWoStyleNotification(DateTime orderDate,
            string orderName,
            IList<string> barcodes)
        {
            var dateString = DateHelper.ConvertUtcToApp(orderDate);
            var subject = "Scan results on " + dateString + " have barcodes w/o styles";
            var body = "<div>Scan date: " + DateHelper.ConvertUtcToApp(orderDate) + "</div>";
            body += "<div>The following barcodes w/o styles:</div>";
            body += "<ul>";
            foreach (var barcode in barcodes.OrderBy(b => b).ToList())
            {
                body += "<li>" + barcode + "</li>";
            }
            body += "</ul>";

            _emailService.SendSystemEmail(subject,
                body,
                EmailHelper.RafiEmail + ";" + EmailHelper.RaananEmail,
                EmailHelper.SupportDgtexEmail);
        }
    }
}
