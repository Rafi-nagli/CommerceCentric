using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Validation;
using Amazon.DTO;
using Amazon.Model.Implementation.Addresses;
using Amazon.Model.Models;

namespace Amazon.Model.Implementation.Validation
{
    public class AddressNotServedByUSPSChecker
    {
        private ILogService _log;
        private IEmailService _emailService;
        private IHtmlScraperService _htmlScraper;
        private ITime _time;

        public AddressNotServedByUSPSChecker(ILogService log,
            IHtmlScraperService htmlScraper,
            IEmailService emailService,
            ITime time)
        {
            _time = time;
            _emailService = emailService;
            _htmlScraper = htmlScraper;
            _log = log;
        }

        public void ProcessResult(CheckResult result, Order dbOrder)
        {
            if (!result.IsSuccess)
            {
                _log.Debug("Set OnHold by AddressNotServedByUSPSChecker");
                dbOrder.OnHold = true;
            }
        }

        public CheckResult Check(IUnitOfWork db,
            DTOMarketOrder order, 
            IList<ListingOrderDTO> items,
            AddressValidationStatus addressValidationStatus)
        {
            //NOTE: Skipping valid addresses
            if (addressValidationStatus < AddressValidationStatus.Invalid)
            {
                return new CheckResult()
                {
                    IsSuccess = true
                };
            }

            var checkUSPSService = new PersonatorAddressCheckService(_log, _htmlScraper, null);
            var address = order.GetAddressDto();
            var addressCheckResult = checkUSPSService.ScrappingCheckAddress(address);
            var result = new CheckResult()
            {
                IsSuccess = !addressCheckResult.IsNotServedByUSPSNote
            };

            _log.Info("AddressNotServedByUSPSChecker, hasNote=" + addressCheckResult.IsNotServedByUSPSNote);

            if (addressCheckResult.IsNotServedByUSPSNote)
            {
                var existNotifier = db.OrderEmailNotifies.IsExist(order.OrderId,
                    OrderEmailNotifyType.OutputAddressNotServedByUSPSEmail);

                if (!existNotifier)
                {
                    var emailInfo = new AddressNotServedByUSPSEmailInfo(_emailService.AddressService, 
                        null,
                        order.OrderId,
                        (MarketType)order.Market,
                        address,
                        order.BuyerName,
                        order.BuyerEmail,
                        order.EarliestShipDate ?? (order.OrderDate ?? DateTime.Today).AddDays(1));

                    _emailService.SendEmail(emailInfo, CallSource.Service);
                    _log.Info("Send address not served by USPS email, orderId=" + order.Id);

                    db.OrderEmailNotifies.Add(new OrderEmailNotify()
                    {
                        OrderNumber = order.OrderId,
                        Reason = "Address isn’t served by USPS",
                        Type = (int)OrderEmailNotifyType.OutputAddressNotServedByUSPSEmail,
                        CreateDate = _time.GetUtcTime(),
                    });
                }

                db.OrderComments.Add(new OrderComment()
                {
                    OrderId = order.Id,
                    Message = "Address isn’t served by USPS. Address verification email sent.",
                    Type = (int)CommentType.OutputEmail,
                    CreateDate = _time.GetAppNowTime(),
                    UpdateDate = _time.GetAppNowTime()
                });

                db.Commit();
            }
            return result;
        }
    }
}
