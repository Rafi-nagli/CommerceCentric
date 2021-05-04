using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets.Walmart;
using Amazon.Web.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;

namespace Amazon.Web.ViewModels.Walmart
{
    public class WalmartCACatalogFeedViewModel
    {
        public string FileName { get; set; }
        
        public IList<MessageString> Validate()
        {
            return new List<MessageString>();
        }

        public CallMessagesResult<bool> UpdateFeed(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            ISystemActionService actionService,
            IItemHistoryService itemHistoryService,
            long companyId)
        {
            var sourceFeed = Path.Combine(UrlHelper.GetImportCatalogFeedPath(), FileName);

            var marketplaceManager = new MarketplaceKeeper(dbFactory, false);
            marketplaceManager.Init();

            IWalmartApi api = (IWalmartApi)new MarketFactory(marketplaceManager.GetAll(), time, log, dbFactory, null)
                .GetApi(companyId, MarketType.WalmartCA, null);

            var service = new WalmartListingInfoReader(log, time, api, dbFactory, actionService, itemHistoryService, null, null);
            service.UpdateListingInfo(sourceFeed);
            
            return new CallMessagesResult<bool>()
            {
                Data = true
            };
        }
    }
}