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
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Enums;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.Model.Models;
using Newtonsoft.Json;

namespace Amazon.Model.Implementation.Markets.Amazon.Feeds
{
    public class AcknowledgementDataUpdater : BaseFeedUpdater
    {
        private IList<SystemActionDTO> _cancelActions;
        private IDictionary<long, CancelOrderOutput> _actionOutputList;
        private ISystemActionService _actionService { get; set; }

        protected override AmazonFeedType Type
        {
            get { return AmazonFeedType.OrderAcknowledgement; }
        }

        protected override string AmazonFeedName
        {
            get { return "_POST_ORDER_ACKNOWLEDGEMENT_DATA_"; }
        }

        public AcknowledgementDataUpdater(ISystemActionService actionService, 
            ILogService log, 
            ITime time,
            IDbFactory dbFactory)
            : base(log, time, dbFactory)
        {
            _actionService = actionService;
        }

        protected override DocumentInfo ComposeDocument(IUnitOfWork db, 
            long companyId, 
            MarketType market, 
            string marketplaceId,
            IList<string> asinList)
        {
            //NOTE: reprocess cancel request every 2 hours
            var maxLastAttemptDate = Time.GetUtcTime().AddHours(-2);

            _cancelActions = _actionService.GetUnprocessedByType(db, SystemActionType.UpdateOnMarketCancelOrder, maxLastAttemptDate, null);
            _actionOutputList = new Dictionary<long, CancelOrderOutput>();

            if (_cancelActions.Any())
            {
                var orderMessages = new List<XmlElement>();
                var index = 0;
                var merchant = db.Companies.Get(companyId).AmazonFeedMerchantIdentifier;
                foreach (var action in _cancelActions)
                {
                    var data = JsonConvert.DeserializeObject<CancelOrderInput>(action.InputData);
                    var order = db.Orders.GetByOrderNumber(data.OrderNumber);
                    if (order == null)
                    {
                        Log.Info(marketplaceId + ": Can't find orderId=" + data.OrderNumber);
                        if (action.CreateDate < Time.GetAppNowTime().AddHours(-21)) //NOTE: Take care about 2 hours
                        {
                            var output = new CancelOrderOutput()
                            {
                                IsProcessed = false,
                                ResultMessage = "Mark as cancelled w/o order by timeout"
                            };
                            _actionService.SetResult(db, action.Id, SystemActionStatus.Done, output);
                            db.Commit();

                            Log.Info("Mark as cancelled w/o order by timeout");
                        }
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

                    if (order.SourceOrderStatus == OrderStatusEnumEx.Canceled)
                    {
                        var output = new CancelOrderOutput()
                        {
                            IsProcessed = false,
                            ResultMessage = "Already cancelled"
                        };
                        _actionService.SetResult(db, action.Id, SystemActionStatus.Done, output);
                        db.Commit();
                        continue;
                    }

                    index++;
                    
                    var items = db.OrderItems.GetByOrderIdAsDto(data.OrderNumber)
                        //Remove canceled items with 0 price
                            .Where(m => m.ItemPrice > 0 || m.QuantityOrdered > 0).ToList();

                    OrderHelper.PrepareSourceItemOrderId(items);
                    items = OrderHelper.GroupBySourceItemOrderId(items);

                    orderMessages.Add(FeedHelper.ComposeOrderAcknowledgementMessage(index, 
                        data.OrderNumber,
                        items));

                    _actionOutputList.Add(new KeyValuePair<long, CancelOrderOutput>(action.Id, new CancelOrderOutput()
                    {
                        Identifier = index
                    }));
                }

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
                var groupActions = _actionService.GetByTypeAndGroupId(db, SystemActionType.UpdateOnMarketCancelOrder,
                    groupId);

                foreach (var action in groupActions)
                {
                    var input = SystemActionHelper.FromStr<CancelOrderInput>(action.InputData);
                    var output = SystemActionHelper.FromStr<CancelOrderOutput>(action.OutputData);

                    if (errorList.All(e => e.MessageId != output.Identifier))
                    {
                        output.IsProcessed = true;
                        _actionService.SetResult(db, action.Id, SystemActionStatus.Done, output);
                    }
                    else
                    {
                        if (action.AttemptNumber > 10)
                            _actionService.SetResult(db, action.Id, SystemActionStatus.Fail, output);
                        else
                            _actionService.SetResult(db, action.Id, SystemActionStatus.None, output);

                        Log.Warn("Order not acknowledgement, orderId=" + input.OrderNumber + ", attemptNumber=" +
                                 action.AttemptNumber);
                    }
                }

                db.Commit();
            }
        }
    }
}
