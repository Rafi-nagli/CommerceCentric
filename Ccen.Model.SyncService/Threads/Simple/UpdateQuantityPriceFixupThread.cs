using System;
using Amazon.Common.Helpers;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Web.Models;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class UpdateQuantityPriceFixupThread : ThreadBase
    {
        public UpdateQuantityPriceFixupThread(long companyId, ISystemMessageService messageService, TimeSpan? callbackInterval = null)
            : base("UpdateQuantityPriceFixup", companyId, messageService, callbackInterval)
        {

        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();
            var settings = new SettingsService(dbFactory);
            var quantityManager = new QuantityManager(GetLogger(), time);
            var actionService = new SystemActionService(GetLogger(), time);
            var priceManager = new PriceManager(GetLogger(), time, dbFactory, actionService, settings);

            using (var db = dbFactory.GetRWDb())
            {
                db.DisableValidation();

                RetryHelper.ActionWithRetries(() => quantityManager.FixupListingQuantity(db, settings), log, retryCount: 1, throwException: false);

                RetryHelper.ActionWithRetries(() => priceManager.FixupListingPrices(db), log, retryCount: 1, throwException: false);

                //RetryHelper.ActionWithRetries(() => priceManager.FixupBusinessPrices(db), log, retryCount: 1, throwException: false);
                //RetryHelper.ActionWithRetries(() => priceManager.FixupFBAPrices(db), log, retryCount: 1, throwException: false);

                RetryHelper.ActionWithRetries(() => priceManager.FixupWalmartPrices(db), log, retryCount: 1, throwException: false);
            }
        }
    }
}
