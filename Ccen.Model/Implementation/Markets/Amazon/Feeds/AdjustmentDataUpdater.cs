using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Amazon.Api.Feeds;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Enums;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.Model.Models;
using Newtonsoft.Json;

namespace Amazon.Model.Implementation.Markets.Amazon.Feeds
{
    public class AdjustmentDataUpdater : BaseFeedUpdater
    {
        private IDictionary<long, ReturnOrderOutput> _actionOutputList;
        private ISystemActionService _actionService { get; set; }
        private IEmailService _emailService { get; set; }

        protected override AmazonFeedType Type
        {
            get { return AmazonFeedType.OrderAdjustment; }
        }

        protected override string AmazonFeedName
        {
            get { return "_POST_PAYMENT_ADJUSTMENT_DATA_"; }
        }

        public AdjustmentDataUpdater(ISystemActionService actionService, 
            IEmailService emailService,
            ILogService log, 
            ITime time,
            IDbFactory dbFactory)
            : base(log, time, dbFactory)
        {
            _actionService = actionService;
            _emailService = emailService;
        }

        protected override DocumentInfo ComposeDocument(IUnitOfWork db, 
            long companyId, 
            MarketType market, 
            string marketplaceId,
            IList<string> asinList)
        {
            var tag = asinList != null && asinList.Count == 1 ? asinList[0] : null;
            var actions = _actionService.GetUnprocessedByType(db, 
                SystemActionType.UpdateOnMarketReturnOrder, 
                null,
                tag);
            if (asinList != null)
            {
                actions = actions.Where(a => asinList.Contains(a.Tag)).ToList();
            }

            _actionOutputList = new Dictionary<long, ReturnOrderOutput>();

            if (actions.Any())
            {
                var orderMessages = new List<XmlElement>();
                var index = 0;
                var merchant = db.Companies.Get(companyId).AmazonFeedMerchantIdentifier;
                foreach (var action in actions)
                {
                    var data = JsonConvert.DeserializeObject<ReturnOrderInput>(action.InputData);
                    var order = db.Orders.GetByOrderNumber(data.OrderNumber);
                    var unsuitableData = false;
                    var unsuitableMessage = "";
                    if (order == null)
                    {
                        unsuitableMessage = "Can't find orderId";
                        unsuitableData = true;
                    }

                    if (data.Items == null)
                    {
                        unsuitableMessage = "No data items";
                        unsuitableData = true;
                    }

                    if (data.Items != null && !data.Items.Any())
                    {
                        unsuitableMessage = "No return items";
                        unsuitableData = true;
                    }

                    if (data.Items != null && !data.Items.Any(i => i.RefundShippingPrice > 0 || i.RefundItemPrice > 0))
                    {
                        unsuitableMessage = "All refund price = 0";
                        unsuitableData = true;
                    }

                    if (unsuitableData)
                    {
                        Log.Info(marketplaceId + ": " + unsuitableMessage);
                        _actionService.SetResult(db, action.Id, SystemActionStatus.Suspended, new ReturnOrderOutput()
                        {
                            ResultMessage = unsuitableMessage
                        });
                        continue;
                    }

                    if (order.Market != (int) market
                        || (!String.IsNullOrEmpty(marketplaceId)
                            && order.MarketplaceId != marketplaceId))
                    {
                        //Log.Info("skip order=" + data.OrderNumber + ", market=" + order.Market + ", marketplace=" + order.MarketplaceId);
                        continue;
                    }

                    Log.Info(marketplaceId + ": add order " + index + ", actionId=" + action.Id + ", orderId=" + data.OrderNumber);

                    index++;

                    var returnItems = data.Items;
                    var orderItems = db.OrderItems.GetAll().Where(oi => oi.OrderId == order.Id).ToList();
                    foreach (var returnItem in data.Items)
                    {
                        var existOrderItem = orderItems.FirstOrDefault(i => i.ItemOrderIdentifier == returnItem.ItemOrderId);
                        if (existOrderItem != null && returnItem.RefundItemPrice > 0 && existOrderItem.ItemPaid > 0 && existOrderItem.ItemTax > 0)
                            returnItem.RefundItemTax = (existOrderItem.ItemTax ?? 0) * returnItem.RefundItemPrice / (existOrderItem.ItemPaid.Value - existOrderItem.ItemTax.Value);
                    }
                    
                    orderMessages.Add(FeedHelper.ComposeOrderAdjustmentMessage(index, 
                        data.OrderNumber,
                        data.IncludeShipping,
                        data.DeductShipping,
                        data.IsDeductPrepaidLabelCost,
                        order.TotalPriceCurrency,
                        returnItems));

                    _actionOutputList.Add(new KeyValuePair<long, ReturnOrderOutput>(action.Id, new ReturnOrderOutput()
                    {
                        Identifier = index
                    }));
                }

                db.Commit(); //NOTE: Save SetResulte changes

                if (orderMessages.Any())
                {
                    Log.Info(marketplaceId + ": Compose feed");
                    var document = FeedHelper.ComposeFeed(orderMessages, merchant, Type.ToString());
                    return new DocumentInfo
                    {
                        XmlDocument = document,
                        NodesCount = index
                    };
                }
            }
            return null;
        }

        protected override void UpdateEntitiesBeforeSubmitFeed(IUnitOfWork db, long feedId)
        {
            foreach (var output in _actionOutputList)
            {
                output.Value.FeedId = feedId;
                _actionService.SetResult(db, output.Key, SystemActionStatus.InProgress, output.Value, feedId.ToString());
            }
            db.Commit();
        }

        protected override void UpdateEntitiesAfterResponse(long feedId, IList<FeedResultMessage> errorList)
        {
            using (var db = DbFactory.GetRWDb())
            {
                var groupId = feedId.ToString();
                var groupActions = _actionService.GetByTypeAndGroupId(db, SystemActionType.UpdateOnMarketReturnOrder,
                    groupId);

                foreach (var action in groupActions)
                {
                    var input = SystemActionHelper.FromStr<ReturnOrderInput>(action.InputData);
                    var output = SystemActionHelper.FromStr<ReturnOrderOutput>(action.OutputData);

                    var error = errorList.FirstOrDefault(e => e.MessageId == output.Identifier);
                    if (error == null)
                    {
                        output.IsProcessed = true;
                        _actionService.SetResult(db, action.Id, SystemActionStatus.Done, output);
                    }
                    else
                    {
                        output.ResultMessage = error.ResultCode + ": " + error.MessageCode + ": " + error.Message;
                        _actionService.SetResult(db, action.Id, SystemActionStatus.Fail, output);

                        _emailService.SendSystemEmail(
                            "System can't process refund, order #" + input.OrderNumber,
                            "Details: " + output.ResultMessage,
                            EmailHelper.RafiEmail + ";" + EmailHelper.RaananEmail,
                            EmailHelper.SupportDgtexEmail);

                        Log.Warn("Order not adjustment, orderId=" + input.OrderNumber);
                    }
                }

                db.Commit();
            }
        }
    }
}
