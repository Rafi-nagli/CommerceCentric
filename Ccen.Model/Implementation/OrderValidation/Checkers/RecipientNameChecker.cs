using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Validation;
using Amazon.DTO;
using Amazon.Model.Models;

namespace Amazon.Model.Implementation.Validation
{
    public class RecipientNameChecker
    {
        private ILogService _log;
        private IEmailService _emailService;
        private ITime _time;
        private Action _recheckAddressCallabck;

        public RecipientNameChecker(ILogService log,
            IEmailService emailService,
            ITime time,
            Action recheckAddressCallabck)
        {
            _time = time;
            _emailService = emailService;
            _log = log;
            _recheckAddressCallabck = recheckAddressCallabck;
        }

        public void ProcessResult(CheckResult result, Order dbOrder)
        {
            if (result.IsSuccess)
            {
                _log.Debug("Update receipient, from=" + dbOrder.PersonName + "/" + dbOrder.PersonName + ", to=" +
                           result.AdditionalData[0]);

                //NOTE: update original PersonName, in case we haven't manually flag, 
                //TODO: or we should set it, and move all chenges to manually fields
                if (dbOrder.IsManuallyUpdated)
                    dbOrder.ManuallyPersonName = result.AdditionalData[0];
                else
                    dbOrder.PersonName = result.AdditionalData[0];

                _recheckAddressCallabck?.Invoke();
            }
            else
            {
                if (result.AdditionalData != null
                    && result.AdditionalData.Any()
                    && result.AdditionalData[0] == "OnHold")
                {
                    _log.Debug("Set OnHold by CheckRecipientName");
                    dbOrder.OnHold = true;
                }
            }
        }

        public CheckResult Check(IUnitOfWork db,
            DTOMarketOrder order,
            IList<ListingOrderDTO> items,
            AddressValidationStatus addressValidationStatus)
        {
            if (order.Id == 0)
                throw new ArgumentOutOfRangeException("order.Id", "Should be non zero");

            if (order.OrderStatus == OrderStatusEnumEx.Pending)
                throw new ArgumentException("order.OrderStatus", "Not supported status Pending");

            //International order has issue with PersonName
            if (!AddressHelper.IsStampsValidPersonName(order.FinalPersonName)
                && (order.AddressValidationStatus == (int)AddressValidationStatus.InvalidRecipientName
                || ShippingUtils.IsInternationalState(order.FinalShippingState)
                || ShippingUtils.IsInternational(order.FinalShippingCountry)))
            {
                //If can resolved using BuyerName
                var nameKeywords = (order.FinalPersonName ?? "").Split(", .".ToCharArray()).Where(n => n.Length > 2).ToArray();
                if (!nameKeywords.Any())
                    nameKeywords = (order.FinalPersonName ?? "").Split(", .".ToCharArray()).ToArray();

                if (AddressHelper.IsStampsValidPersonName(order.BuyerName)
                    //NOTE: #1 Exclude prefix Mr., initials, like a.cheszes
                    //NOTE: #2 Include when name has only one letter
                    && StringHelper.ContrainOneOfKeywords(order.BuyerName, nameKeywords)) 
                {
                    db.OrderComments.Add(new OrderComment()
                    {
                        OrderId = order.Id,
                        Message = "[System] Incomplete recipient name was replaced with buyer name: "
                                  + order.FinalPersonName + "=>" + order.BuyerName,
                        Type = (int)CommentType.Address,
                        CreateDate = _time.GetAppNowTime()
                    });
                    
                    if (order.IsManuallyUpdated)
                        order.ManuallyPersonName = order.BuyerName;
                    else
                        order.PersonName = order.BuyerName;

                    return new CheckResult()
                    {
                        IsSuccess = true,
                        AdditionalData = new[] { order.PersonName }
                    };
                }
                //Send email
                else
                {
                    var emailInfo = new IncompleteNameEmailInfo(_emailService.AddressService, 
                        null,
                        order.OrderId,
                        (MarketType)order.Market, 
                        order.GetAddressDto(),
                        items,
                        order.BuyerName,
                        order.BuyerEmail);

                    _emailService.SendEmail(emailInfo, CallSource.Service);
                    _log.Info("Send incomplete person name email, orderId=" + order.Id);

                    db.OrderEmailNotifies.Add(new OrderEmailNotify()
                    {
                        OrderNumber = order.OrderId,
                        Reason = "System emailed, incomplete name",
                        Type = (int)OrderEmailNotifyType.OutputIncompleteNameEmail,
                        CreateDate = _time.GetUtcTime(),
                    });

                    db.OrderComments.Add(new OrderComment()
                    {
                        OrderId = order.Id,
                        Message = "[System] Incomplete name email sent",
                        Type = (int)CommentType.Address,
                        CreateDate = _time.GetAppNowTime(),
                        UpdateDate = _time.GetAppNowTime()
                    });

                    db.Commit();

                    return new CheckResult()
                    {
                        IsSuccess = false,
                        AdditionalData = new List<string>() { "OnHold" }
                    };
                }
            }

            return new CheckResult()
            {
                IsSuccess = false
            };
        }
    }
}
