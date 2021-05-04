using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;
using Amazon.DTO.Listings;

namespace Amazon.Model.Implementation.Markets.Walmart.Feeds
{
    public interface IFeed
    {
        int FeedType { get; }
        MarketType Market { get; }
        string MarketplaceId { get; }

        void SubmitFeed();
        void SubmitFeed(IList<string> asinList);
        FeedDTO CheckFeedStatus(TimeSpan waitTimeForProcessing);
    }
}
