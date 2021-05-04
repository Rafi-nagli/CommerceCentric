using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Notifications;
using Amazon.Core.Contracts.Notifications.NotificationParams;
using Amazon.Core.Entities.Messages;
using Amazon.Core.Models;
using Amazon.Core.Models.Enums;
using Amazon.DTO.Listings;
using Amazon.DTO.Messages;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels.Messages;

namespace Amazon.Web.ViewModels.Feeds
{
    public class FeedViewModel
    {
        public long Id { get; set; }

        public long Index { get; set; }

        public int Type { get; set; }
        
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public string AmazonIdentifier { get; set; }

        public string RequestFilename { get; set; }
        public string ResponseFilename { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public int MessageCount { get; set; }

        public int Status { get; set; }

        public string FormattedStatus
        {
            get { return FeedStatusHelper.ToString((FeedStatus)Status); }
        }

        public string FeedUrl
        {
            get { return UrlHelper.GetFeedUrl(Id); }
        }

        public string FormattedName
        {
            get
            {
                var name = "";
                if (Market == (int) MarketType.Amazon
                    || Market == (int) MarketType.AmazonEU
                    || Market == (int) MarketType.AmazonAU)
                    name += ((AmazonFeedType) Type).ToString() + " Feed";
                if (Market == (int) MarketType.Walmart)
                    name += ((WalmartReportType)Type).ToString() + " Feed";
                if (Market == (int)MarketType.WalmartCA)
                    name += ((WalmartReportType)Type).ToString() + " Feed";

                name += ", message count: " + MessageCount;

                return name;
            }
        }

        public DateTime? SubmitDate { get; set; }
        
        public FeedViewModel()
        {
            
        }

        public FeedViewModel(FeedDTO feed)
        {
            Id = feed.Id;
            Type = feed.Type;

            Market = feed.Market;
            MarketplaceId = feed.MarketplaceId;

            AmazonIdentifier = feed.AmazonIdentifier;
            Name = feed.Name;
            Description = feed.Description;
            RequestFilename = feed.RequestFilename;
            ResponseFilename = feed.ResponseFilename;
            MessageCount = feed.MessageCount;

            Status = feed.Status;

            SubmitDate = feed.SubmitDate;
        }
        
        public static IQueryable<FeedViewModel> GetAll(IUnitOfWork db,
            ITime time,
            FeedFilterViewModel filter)
        {
            var query = from n in db.Feeds.GetAllAsDto()
                        select new FeedViewModel()
                        {
                            Id = n.Id,
                            Type = n.Type,
                            
                            Market = n.Market,
                            MarketplaceId = n.MarketplaceId,

                            Name = n.Name,
                            Description = n.Description,

                            MessageCount = n.MessageCount,

                            Status = n.Status,

                            SubmitDate = n.SubmitDate,
                        };

            if (filter.Market != MarketType.None)
                query = query.Where(n => n.Market == (int)filter.Market);

            if (!String.IsNullOrEmpty(filter.MarketplaceId))
                query = query.Where(n => n.MarketplaceId == filter.MarketplaceId);

            if (filter.Type.HasValue)
                query = query.Where(n => n.Type == filter.Type.Value);

            if (filter.DateFrom.HasValue)
                query = query.Where(n => n.SubmitDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(n => n.SubmitDate <= filter.DateTo.Value);

            if (filter.Type.HasValue)
                query = query.Where(n => n.Type == filter.Type.Value);
            
            return query;
        }

        public static string GetFilepath(IUnitOfWork db, long feedId)
        {
            var dbFeed = db.Feeds.Get(feedId);
            if (dbFeed != null)
            {
                if (dbFeed.Market == (int)MarketType.Walmart
                        || dbFeed.Market == (int)MarketType.WalmartCA)
                {
                    var path = UrlHelper.GetWalmartFeedPath();
                    if (dbFeed.Type == (int)WalmartReportType.Items)
                        path += "/Item/";
                    path += dbFeed.RequestFilename;
                    return path;
                }
            }
            return null;
        }
    }
}