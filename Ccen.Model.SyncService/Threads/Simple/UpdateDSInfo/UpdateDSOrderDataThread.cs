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
using DropShipper.Api;
using Magento.Api.Wrapper;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateDSInfo
{
    public class UpdateDSOrderDataThread : ThreadBase
    {
        private readonly DropShipperApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateDSOrderDataThread(DropShipperApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval,
            TimeSpan betweenProcessingInverval)
            : base("UpdateDropShipper" + api.MarketplaceId + "OrderData", companyId, messageService, callbackInterval)
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
