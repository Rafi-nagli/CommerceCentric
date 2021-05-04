using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Newtonsoft.Json;

namespace Amazon.Model.Implementation.Markets
{
    public class BaseOrderRefundService
    {
        private IRefundApi _api;
        private ILogService _log;
        private ITime _time;
        private ISystemActionService _actionService;
        private IEmailService _emailService;

        public BaseOrderRefundService(IRefundApi api,
            ISystemActionService actionService,
            IEmailService emailService,
            ILogService log,
            ITime time)
        {
            _api = api;
            _log = log;
            _time = time;
            _actionService = actionService;
            _emailService = emailService;
        }

        public void ProcessRefunds(IUnitOfWork db, string tag)
        {
            //NOTE: reprocess cancel request every 2 hours
            DateTime? maxLastAttemptDate = _time.GetUtcTime().AddHours(-4);
            var maxAttempts = 10;

            var returnActions = _actionService.GetUnprocessedByType(db,
                SystemActionType.UpdateOnMarketReturnOrder,
                maxLastAttemptDate,
                tag);

            foreach (var action in returnActions)
            {
                Order order = null;
                decimal refundAmount = 0M;
                Exception exception = null;
                try
                {
                    var data = JsonConvert.DeserializeObject<ReturnOrderInput>(action.InputData);
                    order = db.Orders.GetByOrderNumber(data.OrderNumber);
                    if (order == null)
                    {
                        _log.Info(_api.Market + "_" + _api.MarketplaceId + ": Can't find orderId=" + data.OrderNumber);
                        continue;
                    }

                    if (order.Market != (int)_api.Market
                        || (!String.IsNullOrEmpty(_api.MarketplaceId)
                            && order.MarketplaceId != _api.MarketplaceId))
                    {
                        //_log.Info("skip order=" + data.OrderNumber + ", market=" + order.Market + ", marketplace=" + order.MarketplaceId);
                        continue;
                    }

                    _log.Info("Order to refund: " + _api.Market + "_" + _api.MarketplaceId
                              + ", actionId=" + action.Id
                              + ", orderId=" + data.OrderNumber);

                    data.MarketOrderId = order.MarketOrderId;
                    data.Id = action.Id;
                    data.PaymentTransactionId = order.PaymentInfo;

                    #region Prepare Fee Data

                    var orderIds = new List<long>() { order.Id };
                    var orderItems = db.OrderItems.GetAllAsDto().Where(oi => orderIds.Contains(oi.OrderId)).ToList();
                    var previousRefunds = db.SystemActions.GetAllAsDto().Where(a => a.Type == (int)SystemActionType.UpdateOnMarketReturnOrder
                            && a.Tag == data.OrderNumber
                            && a.Id != action.Id)
                        .ToList();
                    var previousRefundDataList = previousRefunds.Select(i => JsonConvert.DeserializeObject<ReturnOrderInput>(i.InputData)).ToList();

                    foreach (var orderItem in orderItems)
                    {
                        var refundItem = data.Items.FirstOrDefault(oi => oi.ItemOrderId == orderItem.ItemOrderIdentifier);
                        if (refundItem == null)
                        {
                            if (order.Market != (int)MarketType.Walmart) //NOTE: Skip Walmart, builded based on return request
                            {
                                data.Items.Add(new ReturnOrderItemInput()
                                {
                                    ItemOrderId = orderItem.ItemOrderIdentifier,
                                    Quantity = 0,
                                });
                            }
                        }
                        else
                        {
                            if (refundItem.RefundItemPrice > 0)
                            {
                                decimal alreadyRefundedItemSum = 0;
                                foreach (var refundData in previousRefundDataList)
                                {
                                    alreadyRefundedItemSum += refundData.Items.Where(i => i.ItemOrderId == orderItem.ItemOrderIdentifier).Sum(i => i.RefundItemPrice);
                                }
                                decimal totalItemSum = orderItems.Where(oi => oi.ItemOrderIdentifier == orderItem.ItemOrderIdentifier).Sum(oi => oi.ItemPaid ?? 0);
                                decimal requestedRefundItemSum = refundItem.RefundItemPrice;

                                var perItemPrice = (orderItem.ItemPaid ?? 0) / (decimal)orderItem.QuantityOrdered;
                                
                                var adjustment = refundItem.RefundItemPrice -
                                                 refundItem.Quantity * perItemPrice;
                                if (adjustment > 0)
                                    refundItem.AdjustmentRefund = adjustment;
                                if (adjustment < 0)
                                    refundItem.AdjustmentFee = -adjustment;
                            }
                            else
                            {
                                refundItem.Quantity = 0;
                            }
                        }
                    }

                    //Group back for virtual styles
                    data.Items = data.Items
                        .GroupBy(i => String.Join("_", i.ItemOrderId.Split('_').Take(2)))
                        .Select(g => new ReturnOrderItemInput()
                        {
                            ItemOrderId = g.Key,
                            Quantity = g.Max(i => i.Quantity),
                            Feedback = g.FirstOrDefault()?.Feedback,
                            Notes = g.FirstOrDefault()?.Notes,
                            Reason = g.FirstOrDefault()?.Reason,
                            TotalPaidAmount = g.Sum(i => i.TotalPaidAmount),
                            RefundItemPrice = g.Sum(i => i.RefundItemPrice),
                            RefundShippingPrice = g.Sum(i => i.RefundShippingPrice),
                            DeductPrepaidLabelCost = g.Sum(i => i.DeductPrepaidLabelCost),
                            DeductShippingPrice = g.Sum(i => i.DeductShippingPrice),
                            AdjustmentRefund = g.Sum(i => i.AdjustmentRefund),
                            AdjustmentFee = g.Sum(i => i.AdjustmentFee),
                        })
                        .ToList();

                    refundAmount = data.Items.Sum(i => i.AdjustmentRefund);

                    #endregion

                    var result = _api.RefundOrder(order.AmazonIdentifier, data);

                    if (result.IsSuccess)
                    {
                        var output = new ReturnOrderOutput()
                        {
                            IsProcessed = true,
                            ResultMessage = "Refunds by API"
                        };
                        _actionService.SetResult(db, action.Id, SystemActionStatus.Done, output);
                        db.Commit();
                    }
                    else
                    {
                        _log.Info("Refund Fail: " + result.Message);
                        var output = new ReturnOrderOutput()
                        {
                            IsProcessed = true,
                            ResultMessage = "Fail: " + result.Message
                        };
                        _actionService.SetResult(db, action.Id, action.AttemptNumber > maxAttempts ? SystemActionStatus.Fail : SystemActionStatus.None, output);
                        db.Commit();
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    _log.Error("WalmartOrderRefund.Process", ex);
                }

                if (action.AttemptNumber > maxAttempts)
                {
                    var output = new ReturnOrderOutput()
                    {
                        IsProcessed = true,
                        ResultMessage = exception != null ? exception.Message : ""
                    };
                    _actionService.SetResult(db, action.Id, SystemActionStatus.Fail, output);
                    db.Commit();

                    db.OrderComments.Add(new OrderComment()
                    {
                        OrderId = order.Id,
                        CreateDate = _time.GetAppNowTime(),
                        Type = (int)CommentType.ReturnExchange,
                        Message = "Refund failed, amount: $" + PriceHelper.RoundToTwoPrecision(refundAmount)
                    });
                    db.Commit();

                    _emailService.SendSystemEmail(
                        "System can't process refund, order #" + (order != null ? order.CustomerOrderId : "n/a"),
                        "Details: " + (exception != null ? exception.Message : "-"),
                        EmailHelper.RafiEmail + ";" + EmailHelper.RaananEmail,
                        EmailHelper.SupportDgtexEmail);
                }
            }
        }
    }
}
