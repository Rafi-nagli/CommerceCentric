using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Models;
using Amazon.DTO;

namespace Amazon.Core.Contracts
{
    public interface IReportParser
    {
        ILineParser GetLineParser(ILogService log, MarketType market, string marketplaceId);
        void Init(ParseContext context);
        bool Open(string filePath);
        void Process(IMarketApi api, ITime time, AmazonReportInfo reportInfo, IList<IReportItemDTO> reportItems);
        List<IReportItemDTO> GetReportItems(MarketType market, string marketplaceId);
        void Close();
    }
}
