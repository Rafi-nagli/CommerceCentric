using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Groupon.Api;
using Newtonsoft.Json;
using Walmart.Api;

namespace Amazon.Model.Implementation.Markets.Groupon
{
    public class GrouponOrderCancellation
    {
        private GrouponApi _api;
        private ILogService _log;
        private ITime _time;
        private ISystemActionService _actionService;

        public GrouponOrderCancellation(GrouponApi api, 
            ISystemActionService actionService,
            ILogService log, 
            ITime time)
        {
            _api = api;
            _log = log;
            _time = time;
            _actionService = actionService;
        }

        public void ProcessCancellations(IUnitOfWork db)
        {
            ProcessCancellations(db, null);
        }

        public void ProcessCancellations(IUnitOfWork db, string tag)
        {
            //NOTE: reprocess cancel request every 2 hours
            var maxLastAttemptDate = _time.GetUtcTime().AddHours(-2);

            var cancelActions = _actionService.GetUnprocessedByType(db, SystemActionType.UpdateOnMarketCancelOrder, maxLastAttemptDate, tag);
            var actionOutputList = new Dictionary<long, CancelOrderOutput>();

            foreach (var action in cancelActions)
            {
                try
                {
                    var data = JsonConvert.DeserializeObject<CancelOrderInput>(action.InputData);
                    var orders = db.Orders.GetAllByCustomerOrderNumber(data.OrderNumber);
                    if (!orders.Any())
                    {
                        _log.Info(_api.Market + "_" + _api.MarketplaceId + ": Can't find orderId=" + data.OrderNumber);
                        continue;
                    }

                    if (orders[0].Market != (int) _api.Market
                        || (!String.IsNullOrEmpty(_api.MarketplaceId)
                            && orders[0].MarketplaceId != _api.MarketplaceId))
                    {
                        //_log.Info("skip order=" + data.OrderNumber + ", market=" + order.Market + ", marketplace=" + order.MarketplaceId);
                        continue;
                    }

                    _log.Info("Order to cancel: " + _api.Market + "_" + _api.MarketplaceId + ", actionId=" + action.Id +
                              ", orderId=" + data.OrderNumber + ", itemId=" + data.ItemId);


                    foreach (var order in orders)
                    {
                        order.OrderStatus = OrderStatusEnumEx.Canceled;
                    }
                    db.Commit();

                    var output = new CancelOrderOutput()
                    {
                        IsProcessed = true,
                        ResultMessage = "Cancelled by API"
                    };
                    _actionService.SetResult(db, action.Id, SystemActionStatus.Done, output);
                    db.Commit();
                }
                catch (Exception ex)
                {
                    if (action.AttemptNumber > 10)
                    {
                        var output = new CancelOrderOutput()
                        {
                            IsProcessed = true,
                            ResultMessage = ex.Message
                        };
                        _actionService.SetResult(db, action.Id, SystemActionStatus.Fail, output);
                        db.Commit();
                    }

                    _log.Error("WalmartOrderCancellation.Process", ex);
                }
            }
        }
    }
}
