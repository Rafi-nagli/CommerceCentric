using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Sync;
using Amazon.Web.Models;
using Shopify.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.Model.SyncService.Threads.Simple.UpdateShopifyInfo
{
    public class UpdateShopifyCancellationDataThread : ThreadBase
    {
        private readonly ShopifyApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateShopifyCancellationDataThread(ShopifyApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval,
            TimeSpan betweenProcessingInverval)
            : base("UpdateShopifyCancellationData", companyId, messageService, callbackInterval)
        {
            _api = api;
            _betweenProcessingInverval = betweenProcessingInverval;
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var settings = new SettingsService(dbFactory);

            var actionService = new SystemActionService(GetLogger(), time);

            var lastSyncDate = settings.GetOrdersCancellationDate(_api.Market, _api.MarketplaceId);

            _api.Connect();

            using (var db = dbFactory.GetRWDb())
            {
                LogWrite("Last sync date=" + lastSyncDate);

                if (!lastSyncDate.HasValue ||
                    (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
                {
                    var updater = new BaseOrderCancellation(_api, actionService, GetLogger(), time);
                    updater.ProcessCancellations(db);
                    settings.SetOrdersCancellationtDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
                }
            }
        }
    }
}
