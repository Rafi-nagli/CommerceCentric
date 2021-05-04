using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;

namespace Amazon.Common.Models
{
    public class WebPageParserFactory : IWebPageParserFactory
    {
        public IWebPageParser GetPageParser(MarketType market)
        {
            if (market == MarketType.Amazon || market == MarketType.AmazonEU)
                return new AmazonPageParser();
            if (market == MarketType.Walmart)
                return new WalmartPageParser();
            return null;
        }
    }
}
