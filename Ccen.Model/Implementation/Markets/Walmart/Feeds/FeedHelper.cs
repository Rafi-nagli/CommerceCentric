using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Amazon.Api;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Enums;
using Amazon.DTO.Listings;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Markets.Amazon.Readers;

namespace Amazon.Model.Implementation.Markets.Walmart.Feeds
{
    //public class FeedHelper
    //{
    //    public static FeedDTO IterateFeed(IUnitOfWork db,
    //        IFeed feed,
    //        bool onlyCheck)
    //    {
            

    //        feed.Init(feedDto);

    //        if (feed.CurrentFeed != null)
    //        {
    //            feed.CheckFeedStatus();
    //        }
    //        else
    //        {
    //            if (!onlyCheck)
    //                feed.SubmitFeed();
    //        }

    //        return feed.CurrentFeed;
    //    }
    //}
}
