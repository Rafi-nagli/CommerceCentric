using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets.Groupon;
using Amazon.Model.Implementation.Markets.Magento;
using Amazon.Web.Models;
using Groupon.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateGrouponInfo
{
    public class UpdateGrouponOrderCancellationThread : ThreadBase
    {
        private readonly GrouponApi _api;
        private TimeSpan _betweenProcessingInverval;


        public UpdateGrouponOrderCancellationThread(GrouponApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval,
            TimeSpan betweenProcessingInverval)
            : base("Update" + api.Market + "OrderCancellation", companyId, messageService, callbackInterval)
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
            var actionService = new SystemActionService(log, time);

            var lastSyncDate = settings.GetOrdersCancellationDate(_api.Market, _api.MarketplaceId);

            _api.Connect();

            using (var db = dbFactory.GetRWDb())
            {
                LogWrite("Last sync date=" + lastSyncDate);

                if (!lastSyncDate.HasValue ||
                    (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
                {
                    var updater = new GrouponOrderCancellation(_api, actionService, log, time);
                    updater.ProcessCancellations(db);
                    settings.SetOrdersCancellationtDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
                }
            }
        }
    }
}
