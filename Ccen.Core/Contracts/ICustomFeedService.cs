using Amazon.Core.Models.DropShippers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface ICustomFeedService
    {
        IList<string> GetSourceFieldsListForIncomingFeed(DSFileTypes feedType, DSProductType productType);
        IList<string> GetSourceFieldsListForCustomOutgoingFeed();
        byte[] GenerateFeed(long customFeedId);
    }
}
