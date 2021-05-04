using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation.Sync;
using Amazon.Web.Models;
using Shopify.Api;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateShopifyInfo
{
    public class UpdateShopifyOrderDataThread : ThreadBase
    {
        private readonly ShopifyApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateShopifyOrderDataThread(ShopifyApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval,
            TimeSpan betweenProcessingInverval,
            ILogService overrideLogger = null)
            : base("UpdateShopifyOrderData", companyId, messageService, callbackInterval, overrideLogger)
        {
            _api = api;
            _betweenProcessingInverval = betweenProcessingInverval;
        }

        protected override void RunCallback()
        {
            _api.Connect();

            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var settings = new SettingsService(dbFactory);
            var log = GetLogger();

            using (var db = dbFactory.GetRWDb())
            {
                var lastSyncDate = settings.GetOrdersFulfillmentDate(_api.Market, _api.MarketplaceId);

                LogWrite("Last sync date=" + lastSyncDate);

                if (!lastSyncDate.HasValue ||
                    (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
                {
                    var updater = new BaseOrderUpdater(_api, log, time);
                    updater.UpdateOrders(db);
                    settings.SetOrdersFulfillmentDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
                }
            }
        }
    }
}
