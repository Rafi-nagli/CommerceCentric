using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Api;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.Implementation;
using Amazon.Web.Models;

namespace Amazon.Model.SyncService.Threads.Simple.UpdateAmazonInfo
{
    public class UpdateAmazonBuyBoxStatusThread : ThreadBase
    {
        private readonly IList<AmazonApi> _apiList;
        private TimeSpan _betweenProcessingInverval;

        public UpdateAmazonBuyBoxStatusThread(IList<AmazonApi> apiList, 
            long userId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval,
            TimeSpan betweenProcessingInverval)
            : base("UpdateBuyBoxStatus", userId, messageService, callbackInterval)
        {
            _apiList = apiList;
            _betweenProcessingInverval = betweenProcessingInverval;
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();
            CompanyDTO company = null;
            IList<MarketplaceDTO> marketplaces = new List<MarketplaceDTO>();
            using (var db = dbFactory.GetRDb())
            {
                company = db.Companies.GetByIdWithSettingsAsDto(CompanyId);
                marketplaces = db.Marketplaces.GetAllAsDto();
            }
            var settings = new SettingsService(dbFactory);

            var buyBoxService = new BuyBoxService(log, dbFactory, time);
            var amazonSQSAccount = company.SQSAccounts.FirstOrDefault(a => a.Type == (int)SQSAccountType.Amazon);
            var amazonSQS = new AmazonSQSReader(log,
                time,
                amazonSQSAccount.AccessKey,
                amazonSQSAccount.SecretKey,
                amazonSQSAccount.EndPointUrl);

            var sellerIds = marketplaces
                .Where(m => m.Market == (int) MarketType.Amazon || m.Market == (int) MarketType.AmazonEU || m.Market == (int)MarketType.AmazonAU)
                .Select(m => m.SellerId)
                .ToList();
            
            //Process Change Notifications, doing it each call

            //buyBoxService.ProcessOfferChanges(amazonSQS, sellerIds);

            //Update for every marketplace
            foreach (var api in _apiList)
            {
                var lastSyncDate = settings.GetBuyBoxSyncDate(api.Market, api.MarketplaceId);
                LogWrite(String.Format("MarketplaceId={0}, last sync date={1}", api.MarketplaceId, lastSyncDate));
                
                if (!lastSyncDate.HasValue ||
                    (time.GetUtcTime() - lastSyncDate) > _betweenProcessingInverval)
                {
                    using (var db = dbFactory.GetRWDb())
                    {
                        db.DisableValidation();

                        buyBoxService.Update(api);
                    }

                    settings.SetBuyBoxSyncDate(time.GetUtcTime(), api.Market, api.MarketplaceId);
                }
            }
        }
    }
}
