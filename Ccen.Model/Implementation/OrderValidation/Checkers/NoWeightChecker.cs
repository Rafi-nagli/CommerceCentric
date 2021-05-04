using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Validation;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.Models;
using Amazon.Model.Models.EmailInfos;
using Amazon.Web.Models;

namespace Amazon.Model.Implementation.Validation
{
    public class NoWeightChecker
    {
        private ILogService _log;
        private IEmailService _emailService;
        private ITime _time;
        private CompanyDTO _company;

        public NoWeightChecker(ILogService log,
            IEmailService emailService,
            ITime time,
            CompanyDTO company)
        {
            _time = time;
            _emailService = emailService;
            _log = log;
            _company = company;
        }

        public void ProcessResult(CheckResult result, Order dbOrder)
        {
            if (result.IsSuccess)
            {
                _log.Debug("Set OnHold by NoWeightChecker");
                dbOrder.OnHold = true;
            }
        }

        public CheckResult Check(IUnitOfWork db, DTOMarketOrder order, IList<ListingOrderDTO> items)
        {
            var wasSend = false;
            var woWeightList = items.Where(i => !i.Weight.HasValue || i.Weight == 0).ToList();
            if (woWeightList.Any())
            {
                foreach (var item in woWeightList)
                {
                    if (!item.StyleId.HasValue)
                    {
                        _log.Info("NoWeightChecker, order item hasn't weight");
                        continue;
                    }

                    var styleString = item.StyleID;
                    var lastNotify = db.OrderEmailNotifies.GetAll().Where(n => n.Type == (int) OrderEmailNotifyType.OutputNoWeightEmailToSeller
                                                && n.OrderNumber == styleString
                                                && n.CreateDate.HasValue)
                                        .OrderByDescending(n => n.CreateDate)
                                        .FirstOrDefault();

                    var isSend = true;
                    if (lastNotify != null)
                    {
                        var now = _time.GetUtcTime();
                        var bizHours = _time.GetBizHoursCount(lastNotify.CreateDate.Value, now);
                        if (bizHours <= 24)
                        {
                            _log.Info("Send was skipped, hours passed=" + bizHours);
                            isSend = false;
                        }
                    }

                    if (isSend)
                    {
                        var styleLocations = db.StyleLocations.GetByStyleId(item.StyleId.Value)
                            .OrderByDescending(l => l.IsDefault)
                            .ThenBy(l => l.SortIsle)
                            .ThenBy(l => l.SortSection)
                            .ThenBy(l => l.SortShelf)
                            .ToList();
                        var styleItems = db.StyleItems.GetByStyleIdAsDto(item.StyleId.Value);

                        var woWeightSizes = styleItems
                            .Where(si => !si.Weight.HasValue)
                            .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                            .Select(si => si.Size)
                            .ToList();

                        if (woWeightSizes.Any())
                        {
                            var emailInfo = new NoWeightToSellerEmailInfo(_emailService.AddressService,
                                item.StyleID,
                                woWeightSizes,
                                styleLocations,
                                _company.SellerWarehouseEmailName,
                                _company.SellerWarehouseEmailAddress,
                                "",
                                _company.SellerEmail + ";" + EmailHelper.RaananEmail);

                            _emailService.SendEmail(emailInfo, CallSource.Service);
                            _log.Info("Send weight request email, orderId=" + order.Id + ", styleId=" + styleString);

                            db.OrderEmailNotifies.Add(new OrderEmailNotify()
                            {
                                OrderNumber = styleString,
                                Reason = "System emailed, styleId hasn't weight",
                                Type = (int) OrderEmailNotifyType.OutputNoWeightEmailToSeller,
                                CreateDate = _time.GetUtcTime(),
                            });

                            db.Commit();

                            wasSend = true;
                        }
                        else
                        {
                            _log.Info("Style hasn't sizes w/o weight");
                        }
                    }
                }
            }

            _log.Info("Check, result=" + wasSend);

            return new CheckResult()
            {
                IsSuccess = woWeightList.Any()
            };
        }
    }
}
