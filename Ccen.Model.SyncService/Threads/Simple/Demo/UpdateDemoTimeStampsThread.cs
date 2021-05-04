using System;
using System.Linq;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO.Users;
using Amazon.Web.Models;

namespace Amazon.Model.SyncService.Threads.Simple.Demo
{
    public class UpdateDemoTimeStampsThread : ThreadBase
    {
        public UpdateDemoTimeStampsThread(long companyId, ISystemMessageService messageService, TimeSpan? callbackInterval = null)
            : base("UpdateDemoTimeStamps", companyId, messageService, callbackInterval)
        {
        }

        protected override void RunCallback()
        {
            CompanyDTO company = null;
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();

            var utcNow = time.GetUtcTime();
            var nowMinusDay = time.GetAppNowTime().AddDays(-1);

            var settings = new SettingsService(dbFactory);

            using (var db = dbFactory.GetRWDb())
            {
                db.DisableValidation();
                var orders = db.Orders.GetAll().Where(o => o.OrderDate < nowMinusDay
                    && !o.BatchId.HasValue).ToList();
                log.Info("Updating orders, count=" + orders.Count);
                foreach (var order in orders)
                {
                    var diffDays = Math.Ceiling((nowMinusDay - (order.OrderDate ?? nowMinusDay)).TotalDays);
                    order.OrderDate = (order.OrderDate ?? nowMinusDay).AddDays(diffDays);
                    order.EstDeliveryDate = (order.EstDeliveryDate ?? nowMinusDay).AddDays(diffDays);
                    order.LatestDeliveryDate = (order.LatestDeliveryDate ?? nowMinusDay).AddDays(diffDays);
                    order.EarliestShipDate = (order.EarliestShipDate ?? nowMinusDay).AddDays(diffDays);
                    order.LatestShipDate = (order.LatestShipDate ?? nowMinusDay).AddDays(diffDays);
                }

                log.Info("Commit updates");
                db.Commit();

                log.Info("Updating sync timestamps");

                var marketplaces = new MarketplaceKeeper(dbFactory, false);
                marketplaces.Init();

                foreach (var market in marketplaces.GetAll())
                {
                    settings.SetOrderSyncDate(utcNow, (MarketType)market.Market, market.MarketplaceId);
                    settings.SetOrdersFulfillmentDate(utcNow, (MarketType) market.Market, market.MarketplaceId);
                    settings.SetListingsSendDate(utcNow, (MarketType)market.Market, market.MarketplaceId);
                    settings.SetListingsPriceSyncDate(utcNow, (MarketType)market.Market, market.MarketplaceId);
                    settings.SetListingsQuantityToAmazonSyncDate(utcNow, (MarketType)market.Market, market.MarketplaceId);
                    settings.SetListingsQtyAlert(0, (MarketType)market.Market, market.MarketplaceId);
                    settings.SetListingsPriceAlert(0, (MarketType)market.Market, market.MarketplaceId);
                }
            }
        }
    }
}
