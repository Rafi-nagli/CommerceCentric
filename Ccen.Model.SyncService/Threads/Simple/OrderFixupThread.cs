using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Sync;
using Walmart.Api;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class OrderFixupThread : TimerThreadBase
    {
        public OrderFixupThread(long companyId, ISystemMessageService messageService, IList<TimeSpan> callTimeStamps, ITime time)
            : base("OrderFixup", companyId, messageService, callTimeStamps, time)
        {
            
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();

            var log = GetLogger();
            var time = new TimeService(dbFactory);


            using (var db = dbFactory.GetRWDb())
            {
                var orderIdsToMove = (from o in db.Orders.GetAll()
                                      join sh in db.OrderShippingInfos.GetAll() on o.Id equals sh.OrderId
                                      where o.BatchId.HasValue
                                         && o.OrderStatus == OrderStatusEnumEx.Unshipped
                                         && sh.IsActive
                                         //&& !String.IsNullOrEmpty(sh.LabelPurchaseMessage)
                                         && sh.LabelPurchaseResult == (int)LabelPurchaseResultType.Error
                                      select o.Id)
                                      .Distinct()
                                      .ToList();
                var dbOrdersToMove = db.Orders.GetAll().Where(o => orderIdsToMove.Contains(o.Id)).ToList();
                log.Info("Orders to move: " + dbOrdersToMove.Count);
                log.Info("OrderIds to move: " + String.Join(", ", dbOrdersToMove.Select(o => o.AmazonIdentifier).ToList()));
                dbOrdersToMove.ForEach(o => o.BatchId = null);
                db.Commit();
            }


            var marketplaceManager = new MarketplaceKeeper(dbFactory, false);
            marketplaceManager.Init();

            var marketplaces = marketplaceManager.GetAll();
            var factory = new MarketFactory(marketplaceManager.GetAll(), time, log, dbFactory, null);           

            using (var db = dbFactory.GetRWDb())
            {
                foreach (var market in marketplaces)
                {
                    if (market.Market == (int)MarketType.Walmart
                        || market.Market == (int)MarketType.WalmartCA)
                    {
                        var api = factory.GetApi(CompanyId, (MarketType)market.Market, market.MarketplaceId);
                        if (api is IMarketOrderUpdaterApi)
                        {
                            log.Info("Reset IsFulfilledStatus, for market: " + api.Market + "-" + api.MarketplaceId);

                            var service = new BaseOrderUpdater(api as IMarketOrderUpdaterApi, log, time);
                            service.ResetIsFulfilledStatus(db);
                        }
                    }
                }
            }
        }
    }
}
