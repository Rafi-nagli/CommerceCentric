using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Emails;
using Amazon.Common.Helpers;
using Amazon.Common.Threads;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Stamps;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Trackings.Rules;
using Shopify.Api;

namespace Ccen.Model.SyncService.Threads.Simple.UpdateShopifyInfo
{
    public class UpdateShopifyPaymentStatusThread : ThreadBase
    {
        private readonly ShopifyApi _api;

        public UpdateShopifyPaymentStatusThread(ShopifyApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval = null)
            : base("UpdateShopifyPaymentStatus", companyId, messageService, callbackInterval)
        {
            _api = api;
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();
            var paymentService = new PaymentService(dbFactory, log, time);

            paymentService.ReCheckPaymentStatuses(_api);
        }
    }
}
