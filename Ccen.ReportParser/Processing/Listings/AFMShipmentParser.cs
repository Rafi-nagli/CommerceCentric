using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Api;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;

using Amazon.ReportParser.LineParser;


namespace Amazon.ReportParser.Processing
{
    public class AFMShipmentParser : BaseParser
    {
        public AFMShipmentParser()
        {
        }

        public override ILineParser GetLineParser(ILogService log, MarketType market, string marketplaceId)
        {
            return new ListingLineParser(log);
        }

        public override void Process(IMarketApi api, ITime time, AmazonReportInfo reportInfo, IList<IReportItemDTO> reportItems)
        {
            using (var uow = new UnitOfWork(Log))
            {
                var items = reportItems.Cast<ItemDTO>().ToList();

                Log.Debug("Begin process items");
                ProcessItems(uow, api, items);
                Log.Debug("End process items");
            }
        }

        private void ProcessItems(IUnitOfWork db, IMarketApi api, List<ItemDTO> items)
        {
            Log.Debug("ProcessItems begin");

            Log.Debug("ProcessItems end");
        }
    }
}
