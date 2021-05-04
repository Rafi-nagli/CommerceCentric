using System.Collections.Generic;
using System.Linq;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.ReportParser.LineParser;

namespace Amazon.ReportParser.Processing.Listings
{
    public class OrderSettlementV2Parser : BaseParser
    {
        public OrderSettlementV2Parser()
        {
        }

        public override ILineParser GetLineParser(ILogService log, MarketType market, string marketplaceId)
        {
            return new OrderSettlementV2LineParser(log);
        }

        public override void Process(IMarketApi api, ITime time, AmazonReportInfo reportInfo, IList<IReportItemDTO> reportItems)
        {
            using (var uow = new UnitOfWork(Log))
            {
                var items = reportItems.Cast<OrderSettlementDTO>().ToList();

                Log.Debug("Begin process items");
                ProcessItems(uow, items);
                Log.Debug("End process items");
            }
        }

        private void ProcessItems(IUnitOfWork db, IList<OrderSettlementDTO> items)
        {
            Log.Debug("ProcessItems begin");



            Log.Debug("ProcessItems end");
        }
    }
}
