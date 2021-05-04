﻿using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Newtonsoft.Json;
using Walmart.Api;

namespace Amazon.Model.Implementation.Markets.Walmart
{
    public class WalmartOrderCancellation
    {
        private IWalmartApi _api;
        private ILogService _log;
        private ITime _time;
        private ISystemActionService _actionService;

        public WalmartOrderCancellation(IWalmartApi api, 
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

                    var itemIdList = (data.ItemId ?? "").Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var orderIdList = orders.Select(o => o.Id).ToList();
                    var allOrderItems = db.OrderItems.GetWithListingInfo().Where(i => i.OrderId.HasValue && orderIdList.Contains(i.OrderId.Value)).ToList();

                    OrderHelper.PrepareSourceItemOrderId(allOrderItems);
                    allOrderItems = OrderHelper.GroupBySourceItemOrderId(allOrderItems);

                    var orderItemToCancel = allOrderItems;

                    //NOTE: we will cancel always the whole order
                    //if (itemIdList.Count() != allOrderItems.Count)
                    //{
                    //    orderItemToCancel = allOrderItems.Where(i => itemIdList.Contains(i.SourceMarketId) || itemIdList.Contains(i.SourceMarketId)).ToList();
                    //}                    
                    
                    CallResult<DTOOrder> result;
                    if (orderItemToCancel.Count >= 1)
                    {
                        var orderToCancel = orders.FirstOrDefault(o => o.Id == orderItemToCancel[0].OrderId);
                        result = _api.CancelOrder(orderToCancel.AmazonIdentifier, orderItemToCancel);
                    }
                    else
                    {
                        result = CallResult<DTOOrder>.Fail("No items to cancel", null);
                    }


                    if (result.IsSuccess)
                    {
                        var output = new CancelOrderOutput()
                        {
                            IsProcessed = true,
                            ResultMessage = "Cancelled by API"
                        };
                        _actionService.SetResult(db, action.Id, SystemActionStatus.Done, output);
                        db.Commit();
                    }
                    else
                    {
                        var output = new CancelOrderOutput()
                        {
                            IsProcessed = true,
                            ResultMessage = "Fail: " + result.Message
                        };

                        var status = SystemActionStatus.None;
                        if (StringHelper.ContainsNoCase(result.Message, "Order not found"))
                            status = SystemActionStatus.Fail;
                        _actionService.SetResult(db, action.Id, status, output);
                        db.Commit();
                    }
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
