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
using Amazon.DTO;
using Amazon.DTO.Orders;
using Amazon.DTO.Users;
using Amazon.Model.Models.EmailInfos;
using Amazon.Web.Models;

namespace Amazon.Model.Implementation.Emails.Rules
{
    public class ReturnRequestEmailRule : IEmailRule
    {
        private ILogService _log;
        private IEmailService _emailService;
        private ITime _time;
        private ISystemActionService _systemAction;
        private CompanyDTO _company;
        private bool _enableSendEmail;
        private bool _existanceCheck;

        public ReturnRequestEmailRule(ILogService log,
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

        private enum ReasonCode
        {
            None = 0,
            TooSmall = 1,
            TooLarge = 2
        }

        public void Process(IUnitOfWork db, EmailReadingResult result)
        {
            if (result.Status != EmailMatchingResultStatus.New
                || !result.HasMatches)
                return;

            var subject = result.Email.Subject ?? "";
            var returnRequest = subject.StartsWith("Return authorization request for order")
                || subject.StartsWith("Return authorisation request for order")
                || subject.StartsWith("Return authorisation notification for order")
                || subject.StartsWith("Return authorization notification for order");

            if (returnRequest)
            {
                result.WasEmailProcessed = true; //NOTE: skipped comment added to other Rules
                _log.Info("ReturnRequsetEmailRule, WasEmailProcessed=" + result.WasEmailProcessed);

                var orderNumber = result.MatchedIdList.FirstOrDefault();
                _log.Info("Received Return authorization request, orderNumber=" + orderNumber);

                if (_existanceCheck)
                {
                    var existReturn = db.ReturnRequests.GetAll().FirstOrDefault(r => r.OrderNumber == orderNumber);
                    if (existReturn != null)
                    {
                        //Already exist
                        return;
                    }
                }

                var order = db.Orders.GetByOrderNumber(orderNumber);
                var orderItems = db.Listings.GetOrderItems(order.Id);
                var orderShippings = db.OrderShippingInfos.GetByOrderIdAsDto(order.Id).Where(sh => sh.IsActive).ToList();

                //Return codes: https://sellercentral.amazon.com/gp/help/help-page.html/ref=ag_200453320_cont_scsearch?ie=UTF8&itemID=200453320&language=en_CA

                /*Order ID: # 112-4884600-1049826
                Item: Toy Story Woody Buzz Boys 4 pc Cotton Pajamas Set (6)
                Quantity: 1
                Return reason: Too small
                Customer comments: I need 5T or 6T sizes. (Optional)
                Details: ... (Optional)
                Request received: February 27, 2016*/

                /*
                 Order ID: # 114-8683360-8244230
                    Item: Shopkins Girls' Little Girls' Luxe Plush 2-Piece Pajama Snuggle Set, Pink, 6
                    Quantity: 1
                    Return reason: Too small

                    Item: Shopkins Girls' Little Girls' 2-Piece Fleece Pajama Set, Pink, 6
                    Quantity: 1
                    Return reason: Too small
                 */

                var messages = result.Email.Message.Split(new string[] {"<br />", "<br>", "<br/>", "/r/n"},
                    StringSplitOptions.RemoveEmptyEntries);

//                var regex = new Regex(@"Order ID: # (?<OrderId>.*)
//                    ([\s\S]+)Item: (?<ItemName>.*)
//                    ([\s\S]+)Quantity: (?<Quantity>.*)
//                    ([\s\S]+)Return reason: (?<Reason>.*)  
//                    ([\s\S]+)Request received: (?<Date>.*)");

//                var match = regex.Match(message);
//                string receiveDate = match.Groups["Date"].Value;
//                string orderId = match.Groups["OrderId"].Value;
//                string itemName = match.Groups["ItemName"].Value;
//                string quantity = match.Groups["Quantity"].Value;
//                string reasonText = match.Groups["Reason"].Value;

                var returns = new List<ReturnRequestDTO>();

                ReturnRequestDTO currentReturn = null;

                foreach (var message in messages)
                {
                    if (message.StartsWith("Item: "))
                    {
                        currentReturn = new ReturnRequestDTO();
                        returns.Add(currentReturn);
                        currentReturn.ItemName = message.Replace("Item: ", "");
                    }

                    if (currentReturn != null)
                    {
                        if (message.StartsWith("Return reason: "))
                            currentReturn.Reason = StringHelper.Substring(message.Replace("Return reason: ", ""), 255);
                        if (message.StartsWith("Quantity: "))
                            currentReturn.Quantity = StringHelper.TryGetInt(message.Replace("Quantity: ", ""));

                        if (message.StartsWith("Request received:"))
                            currentReturn.ReceiveDate =
                                DateHelper.FromDateString(message.Replace("Request received: ", ""));
                        if (message.StartsWith("Customer comments:"))
                            currentReturn.CustomerComments =
                                StringHelper.Substring(message.Replace("Customer comments: ", ""), 255);
                        if (message.StartsWith("Details:"))
                            currentReturn.Details = StringHelper.Substring(message.Replace("Details: ", ""), 255);
                    }
                }

                //var reason = ReasonCode.None;

                //ListingOrderDTO itemToCheck = null;
                //var itemsToExchange = new List<StyleItemCache>();

                foreach (var ret in returns)
                {
                    var itemToCheck = orderItems.FirstOrDefault(i => i.Title == ret.ItemName);
                    //If not found check all items
                    if (itemToCheck == null && orderItems.Count == 1)
                    {
                        itemToCheck = orderItems.FirstOrDefault();
                    }
                    _log.Info("Item to check=" + (itemToCheck != null ? itemToCheck.SKU : "[null}"));

                    if (itemToCheck != null)
                    {
                        ret.StyleId = itemToCheck.StyleId;
                        ret.StyleItemId = itemToCheck.StyleItemId;
                        ret.SKU = itemToCheck.SKU;
                        ret.StyleString = itemToCheck.StyleID;
                    }
                }
                
                foreach (var ret in returns)
                {
                    var requestDb = new ReturnRequest()
                    {
                        ItemName = StringHelper.Substring(ret.ItemName, 255),
                        OrderNumber = orderNumber,
                        Quantity = ret.Quantity,
                        Reason = ret.Reason,
                        CustomerComments = StringHelper.Substring(ret.CustomerComments, 255),
                        Details = StringHelper.Substring(ret.Details, 255),
                        ReceiveDate = ret.ReceiveDate,
                        CreateDate = _time.GetAppNowTime(),

                        StyleId = ret.StyleId,
                        StyleItemId = ret.StyleItemId,
                        SKU = ret.SKU,
                    };

                    db.ReturnRequests.Add(requestDb);
                    db.Commit();
                }

                _log.Info("Request saved into DB");

                if (!_enableSendEmail)
                {
                    //Skip logic with composing comment / email message 
                    return;
                }

                if (ShippingUtils.IsInternational(order.ShippingCountry))
                {
                    //Don't offer exchange to international order
                    return;
                }

                var comments = new List<string>();
                foreach (var request in returns)
                {
                    var reason = GetReasonCode(request.Reason);
                    var itemsToExchange = GetItemsToExchange(db, request);
                    if (reason == ReasonCode.TooSmall || reason == ReasonCode.TooLarge)
                    {
                        var itemsToExchangeWithQty = itemsToExchange.Where(i => i.RemainingQuantity > 0).ToList();

                        //TASK: Don’t send those emails to orders which can’t be returned already, because more than 30 days passed like order 105-5461372-1374643.
                        //TASK: orders, which were placed  Nov 1-Jan 31, can be returned/exchanged by January 31
                        var returnRequestToLate = false;
                        if (order.OrderDate.HasValue)
                        {
                            returnRequestToLate = OrderHelper.AcceptReturnRequest(order.OrderDate.Value,
                                order.EstDeliveryDate,
                                orderShippings.Max(sh => sh.ActualDeliveryDate),
                                result.Email.ReceiveDate,
                                _time.GetAppNowTime());
                        }

                        if (returnRequestToLate)
                        {
                            var message = String.Format("Return request received (reason: {0}, style: {1}). Return request came too late. No email was sent to client by the system.",
                                request.Reason,
                                request.StyleString);
                            comments.Add(message);
                        }
                        else
                        {
                            //TASK: don’t send Too small/big emails to clients who already purchased bigger/smaller size of same pajama, like this client - 105-1300286-6499443.
                            var alreadyPurchasedOrdersByCustomer =
                                db.Orders.GetAll().Where(o => o.OrderStatus == OrderStatusEnumEx.Unshipped
                                                              && o.BuyerEmail == order.BuyerEmail)
                                    .Select(o => o.Id).ToList();

                            var alreadyPurchasedItems = db.OrderItems.GetWithListingInfo()
                                .Where(oi => alreadyPurchasedOrdersByCustomer.Contains(oi.OrderId.Value))
                                .ToList();

                            var alreadyPurchasedAnotherSize = false;
                            if (request.StyleId.HasValue)
                                alreadyPurchasedAnotherSize =
                                    alreadyPurchasedItems.Any(i => i.StyleEntityId == request.StyleId
                                                                   && i.StyleItemId != request.StyleItemId);

                            if (alreadyPurchasedAnotherSize)
                            {
                                var message = String.Format("Return request received (reason: {0}, style: {1}). Customer already purchased bigger/smaller size of same pajama. No email was sent to client by the system.",
                                        request.Reason,
                                        request.StyleString);
                                comments.Add(message);
                            }
                            else
                            {
                                if (itemsToExchangeWithQty.Any())
                                {
                                    var sizeList = itemsToExchangeWithQty
                                        .OrderBy(i => SizeHelper.GetSizeIndex(i.Size))
                                        .Select(i => i.Size)
                                        .ToList();
                                    var sizeString = String.Join(" or ", sizeList);

                                    _log.Info("Send email for customer: " + reason + ", sizes=" + sizeString);

                                    var commentText = String.Format("Return request received (reason: {0}, style: {1}).",
                                            request.Reason,
                                            request.StyleString,
                                            reason == ReasonCode.TooLarge ? "smaller" : "bigger");
                                    comments.Add(commentText);
                                    //TEMP: Disabled email
                                    //var commentText = String.Format("Return request received (reason: {0}, style: {1}). System offered client {2} size. Exchange request email sent.",
                                    //        request.Reason,
                                    //        request.StyleString,
                                    //        reason == ReasonCode.TooLarge ? "smaller" : "bigger");
                                    //comments.Add(commentText);

                                    //var emailInfoToBuyer = new AcceptReturnRequestEmailInfo(null,
                                    //    orderNumber,
                                    //    (MarketType) order.Market,
                                    //    reason == ReasonCode.TooLarge,
                                    //    sizeString,
                                    //    order.BuyerName,
                                    //    order.BuyerEmail);

                                    //_emailService.SendEmail(emailInfoToBuyer, CallSource.Service);
                                }
                                else
                                {
                                    var sizeList = itemsToExchange
                                        .OrderBy(i => SizeHelper.GetSizeIndex(i.Size))
                                        .Select(i => i.Size)
                                        .ToList();
                                    var sizeString = String.Join(", ", sizeList);

                                    var message = String.Format("Return request received (reason: {0}). System didn't find any items to offer exchange (style: {1}, size: {2}). No email was sent to client by the system.",
                                            request.Reason,
                                            request?.StyleString ?? "[null]",
                                            sizeString);
                                    comments.Add(message);
                                }
                            }
                        }
                        db.Commit();
                    }
                }

                if (comments.Any())
                {
                    db.OrderComments.Add(new OrderComment()
                    {
                        OrderId = order.Id,
                        Message = "[System] " + String.Join(", ", comments),
                        Type = (int) CommentType.ReturnExchange,
                        CreateDate = _time.GetAppNowTime(),
                        UpdateDate = _time.GetAppNowTime()
                    });
                }
            }
        }

        private IList<StyleItemCache> GetItemsToExchange(IUnitOfWork db, ReturnRequestDTO request)
        {
            var itemsToExchange = new List<StyleItemCache>();
            //Check reason
            if (!String.IsNullOrEmpty(request.Reason))
            {
                var reasonText = request.Reason;
                var reason = GetReasonCode(reasonText);
                _log.Info("Reason code=" + reason);

                if (request.StyleItemId.HasValue)
                {
                    _log.Info("Checked styleItemId=" + request.StyleItemId);
                    var styleQuery = from si in db.StyleItems.GetAll()
                                     join s in db.Sizes.GetAll() on si.SizeId equals s.Id
                                     where si.StyleId == request.StyleId
                                     orderby s.SortOrder ascending
                                     select new
                                     {
                                         Size = s.Name,
                                         SortOrder = s.SortOrder,
                                         SizeGroupId = s.SizeGroupId,
                                         StyleItemId = si.Id
                                     };

                    var styleItems = styleQuery.ToList();
                    var index = styleItems.FindIndex(si => si.StyleItemId == request.StyleItemId);

                    var styleItemIdListToExchange = new List<long>();
                    if (reason == ReasonCode.TooSmall)
                    {
                        if (styleItems.Count > index + 1)
                        {
                            styleItemIdListToExchange.Add(styleItems[index + 1].StyleItemId);
                        }

                        if (styleItems.Count > index + 2)
                        {
                            styleItemIdListToExchange.Add(styleItems[index + 2].StyleItemId);
                        }
                    }
                    if (reason == ReasonCode.TooLarge)
                    {
                        if (index > 0)
                        {
                            styleItemIdListToExchange.Add(styleItems[index - 1].StyleItemId);
                        }

                        if (index > 1)
                        {
                            styleItemIdListToExchange.Add(styleItems[index - 2].StyleItemId);
                        }
                    }
                    _log.Info("StyleItem to exchange=" + String.Join(", ", styleItemIdListToExchange));

                    if (styleItemIdListToExchange.Any())
                    {
                        itemsToExchange = db.StyleItemCaches
                            .GetFiltered(sic => styleItemIdListToExchange.Contains(sic.Id))
                            .ToList();

                        foreach (var itemToExchange in itemsToExchange)
                        {
                            _log.Info("Exchange styleItemId"
                                      + ", quantity=" +
                                      (itemToExchange != null
                                          ? itemToExchange.RemainingQuantity.ToString()
                                          : "[null]")
                                      + ", size=" + (itemToExchange != null ? itemToExchange.Size : "[null]"));
                        }
                    }
                }
            }

            return itemsToExchange;
        }

        private ReasonCode GetReasonCode(string reasonText)
        {
            var reason = ReasonCode.None;
            if (reasonText == "Too small" || reasonText == "APPAREL_TOO_SMALL")
            {
                reason = ReasonCode.TooSmall;
            }
            if (reasonText == "Too big" || reasonText == "Too large" || reasonText == "APPAREL_TOO_LARGE")
            {
                reason = ReasonCode.TooLarge;
            }
            return reason;
        }
    }
}
