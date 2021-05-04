using System;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation.Sync;
using Amazon.Web.Models;
using Magento.Api.Wrapper;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateMagentoInfo
{
    public class UpdateMagentoOrderDataThread : ThreadBase
    {
        private readonly Magento20MarketApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateMagentoOrderDataThread(Magento20MarketApi api,
                    long companyId,
                    ISystemMessageService messageService,
                    TimeSpan? callbackInterval,
                    TimeSpan betweenProcessingInverval)
                    : base("UpdateMagento" + api.MarketplaceId + "OrderData", companyId, messageService, callbackInterval)
        {
            _api = api;
            _betweenProcessingInverval = betweenProcessingInverval;
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var settings = new SettingsService(dbFactory);
            var log = GetLogger();

            _api.Connect();

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
