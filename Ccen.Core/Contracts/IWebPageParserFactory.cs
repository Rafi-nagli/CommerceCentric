using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Amazon.Core.Models;

namespace Amazon.Core.Contracts
{
    public interface IWebPageParserFactory
    {
        IWebPageParser GetPageParser(MarketType market);
    }
}
