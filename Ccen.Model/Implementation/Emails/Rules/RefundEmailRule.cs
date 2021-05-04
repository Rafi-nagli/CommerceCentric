using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Caches;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.DTO.Orders;
using Amazon.DTO.Users;
using Amazon.Model.Models.EmailInfos;
using Amazon.Web.Models;
using Newtonsoft.Json;

namespace Amazon.Model.Implementation.Emails.Rules
{
    public class RefundEmailRule : IEmailRule
    {
        private ILogService _log;
        private IEmailService _emailService;
        private ITime _time;
        private ISystemActionService _systemAction;
        private CompanyDTO _company;
        private bool _enableSendEmail;
        private bool _existanceCheck;

        public RefundEmailRule(ILogService log,
            ITime time,
            IEmailService emailService,
            ISystemActionService systemAction,
            CompanyDTO company,
            bool enableSendEmail,
            bool existanceCheck)
        {
            _log = log;
            _time = time;
            _systemAction = systemAction;
            _emailService = emailService;
            _company = company;

            _enableSendEmail = enableSendEmail;
            _existanceCheck = existanceCheck;
        }

        public void Process(IUnitOfWork db, EmailReadingResult result)
        {
            if (result.Status != EmailMatchingResultStatus.New
                || !result.HasMatches)
                return;

            var subject = result.Email.Subject ?? "";
            var refundRequest = subject.StartsWith("Refund initiated for order");

            if (refundRequest)
            {
                result.WasEmailProcessed = true; //NOTE: skipped comment added to other Rules
                _log.Info("ReturnRequsetEmailRule, WasEmailProcessed=" + result.WasEmailProcessed);

                var orderNumber = result.MatchedIdList.FirstOrDefault();
                _log.Info("Received Return authorization request, orderNumber=" + orderNumber);

                var order = db.Orders.GetByOrderNumber(orderNumber);
                //var orderItems = db.Listings.GetOrderItems(order.Id);
                //var orderShippings = db.OrderShippingInfos.GetByOrderIdAsDto(order.Id).Where(sh => sh.IsActive).ToList();

                //-- Refund Amount: $15.66                
                var amountText = StringHelper.GetTextBetween(result.Email.Message, "Refund Amount: ", new string[] { "\r", "\n", " " });
                amountText = StringHelper.RemoveWhitespace(amountText ?? "").Replace("$", "");
                var amountValue = StringHelper.TryGetDecimal(amountText);
                if (!amountValue.HasValue
                    || amountValue == 0)
                    return;

                if (_existanceCheck)
                {
                    var existRefunds = db.SystemActions.GetAll().Where(r => r.Tag == orderNumber
                        && r.Type == (int)SystemActionType.UpdateOnMarketReturnOrder).ToList();
                    if (existRefunds.Any())
                    {
                        foreach (var existRefund in existRefunds)
                        {
                            var data = JsonConvert.DeserializeObject<ReturnOrderInput>(existRefund.InputData);
                            var existAmount = RefundHelper.GetAmount(data);
                            if (existAmount == amountValue)
                            {
                                //Already exist
                                if (existRefund.Status == (int)SystemActionStatus.Fail)
                                {
                                    _log.Info("Mark exist refund as done: " + amountValue + ", order: " + data.OrderNumber);
                                    existRefund.Status = (int)SystemActionStatus.Done;
                                    db.Commit();
                                }
                                else
                                {
                                    _log.Info("Already exist");
                                }
                                return;
                            }
                            else
                            {
                                //Not exist
                            }
                        }
                        
                    }
                }

                _systemAction.AddAction(db,
                    SystemActionType.UpdateOnMarketReturnOrder,
                    order.AmazonIdentifier,
                    new ReturnOrderInput()
                    {                
                        OrderId = order.Id,
                        OrderNumber = order.AmazonIdentifier,
                        RefundAmount = amountValue,
                        RefundReason = (int)RefundReasonCodes.Return                        
                    },
                    null,
                    null,
                    SystemActionStatus.Done);
                
                _log.Info("Refund added to DB, amount=" + amountValue);
            }
        }
    }
}
