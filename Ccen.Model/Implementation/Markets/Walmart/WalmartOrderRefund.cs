using System;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Newtonsoft.Json;
using Walmart.Api;

namespace Amazon.Model.Implementation.Markets.Walmart
{
    //public class WalmartOrderRefund
    //{
    //    private IWalmartApi _api;
    //    private ILogService _log;
    //    private ITime _time;
    //    private ISystemActionService _actionService;
    //    private IEmailService _emailService;

    //    public WalmartOrderRefund(IWalmartApi api, 
    //        ISystemActionService actionService,
    //        IEmailService emailService,
    //        ILogService log, 
    //        ITime time)
    //    {
    //        _api = api;
    //        _log = log;
    //        _time = time;
    //        _actionService = actionService;
    //        _emailService = emailService;
    //    }

    //    public void ProcessRefunds(IUnitOfWork db, string tag)
    //    {
    //        //NOTE: reprocess cancel request every 2 hours
    //        DateTime? maxLastAttemptDate = _time.GetUtcTime().AddHours(-4);
    //        var maxAttempts = 10;

    //        var returnActions = _actionService.GetUnprocessedByType(db, 
    //            SystemActionType.UpdateOnMarketReturnOrder, 
    //            maxLastAttemptDate,
    //            tag);

    //        foreach (var action in returnActions)
    //        {
    //            Order order = null;
    //            Exception exception = null;
    //            try
    //            {
    //                var data = JsonConvert.DeserializeObject<ReturnOrderInput>(action.InputData);
    //                order = db.Orders.GetByOrderNumber(data.OrderNumber);
    //                if (order == null)
    //                {
    //                    _log.Info(_api.Market + "_" + _api.MarketplaceId + ": Can't find orderId=" + data.OrderNumber);
    //                    continue;
    //                }

    //                if (order.Market != (int) _api.Market
    //                    || (!String.IsNullOrEmpty(_api.MarketplaceId)
    //                        && order.MarketplaceId != _api.MarketplaceId))
    //                {
    //                    //_log.Info("skip order=" + data.OrderNumber + ", market=" + order.Market + ", marketplace=" + order.MarketplaceId);
    //                    continue;
    //                }

    //                _log.Info("Order to refund: " + _api.Market + "_" + _api.MarketplaceId
    //                          + ", actionId=" + action.Id
    //                          + ", orderId=" + data.OrderNumber);

    //                var result = _api.RefundOrder(order.AmazonIdentifier, data);

    //                if (result.IsSuccess)
    //                {
    //                    var output = new ReturnOrderOutput()
    //                    {
    //                        IsProcessed = true,
    //                        ResultMessage = "Refunds by API"
    //                    };
    //                    _actionService.SetResult(db, action.Id, SystemActionStatus.Done, output);
    //                    db.Commit();
    //                }
    //                else
    //                {
    //                    _log.Info("Refund Fail: " + result.Message);
    //                    var output = new ReturnOrderOutput()
    //                    {
    //                        IsProcessed = true,
    //                        ResultMessage = "Fail: " + result.Message
    //                    };
    //                    _actionService.SetResult(db, action.Id, action.AttemptNumber > maxAttempts ? SystemActionStatus.Fail : SystemActionStatus.None, output);
    //                    db.Commit();
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                exception = ex;
    //                _log.Error("WalmartOrderRefund.Process", ex);
    //            }

    //            if (action.AttemptNumber > maxAttempts)
    //            {
    //                var output = new ReturnOrderOutput()
    //                {
    //                    IsProcessed = true,
    //                    ResultMessage = exception != null ? exception.Message : ""
    //                };
    //                _actionService.SetResult(db, action.Id, SystemActionStatus.Fail, output);
    //                db.Commit();

    //                _emailService.SendSystemEmail(
    //                    "System can't process refund, order #" + (order != null ? order.CustomerOrderId : "n/a"),
    //                    "Details: " + (exception != null ? exception.Message : "-"),
    //                    EmailHelper.RafiEmail + ";" + EmailHelper.SvetaEmail,
    //                    EmailHelper.SupportDgtexEmail);
    //            }
    //        }
    //    }
    //}
}
