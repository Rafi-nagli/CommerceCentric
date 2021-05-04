using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.ReportParser.LineParser;

namespace Amazon.ReportParser.Processing.Listings
{
    //public class ListingCombineDataParser : BaseParser
    //{
    //    private ListingDataParser _dataParser = null;
    //    private ListingUpdateParser _updateParser = null;

    //    public ListingCombineDataParser(bool isProcessInactive, bool canCreateStyleInfo)
    //        : base()
    //    {
    //        _dataParser = new ListingDataParser(isProcessInactive, canCreateStyleInfo);            
    //        _updateParser = new ListingUpdateParser();
    //    }

    //    public override ILineParser GetLineParser(ILogService log, MarketType market, string marketplaceId)
    //    {
    //        return new ListingLineParser(log);
    //    }

    //    public override void Init(ILogService log, ParseContext context)
    //    {
    //        base.Init(log, context);
    //        _dataParser.Init(log, context);
    //        _updateParser.Init(log, context);
    //    }


    //    public override void Process(IMarketApi api, ITime time, AmazonReportInfo reportInfo, IList<IReportItemDTO> reportItems)
    //    {
    //        _dataParser.Process(api, time, reportInfo, reportItems);
    //        _updateParser.Process(api, time, reportInfo, reportItems);
    //    }
    //}
}
