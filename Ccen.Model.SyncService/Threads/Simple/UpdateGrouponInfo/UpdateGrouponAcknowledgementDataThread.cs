using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
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
    public class UpdateGrouponOrderAcknowledgementThread : ThreadBase
    {
        private readonly GrouponApi _api;
        private TimeSpan _betweenProcessingInverval;

        public UpdateGrouponOrderAcknowledgementThread(GrouponApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval,
            TimeSpan betweenProcessingInverval)
            : base("Update" + api.Market + "OrderAcknowledgement", companyId, messageService, callbackInterval)
        {
            _api = api;
            _betweenProcessingInverval = betweenProcessingInverval;
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var settings = new SettingsService(dbFactory);

            var lastSyncDate = settings.GetOrdersAcknowledgementDate(_api.Market, _api.MarketplaceId);

            _api.Connect();

            using (var db = dbFactory.GetRWDb())
            {
                LogWrite("Last sync date=" + lastSyncDate);

                if (!lastSyncDate.HasValue ||
                    (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
                {
                    var updater = new GrouponOrderAcknowledgement(_api, GetLogger(), time);
                    updater.UpdateOrders(db);
                    settings.SetOrdersAcknowledgementDate(time.GetUtcTime(), _api.Market, _api.MarketplaceId);
                }
            }
        }
    }
}
