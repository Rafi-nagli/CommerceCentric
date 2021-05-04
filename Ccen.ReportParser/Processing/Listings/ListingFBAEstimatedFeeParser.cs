using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.ReportParser.LineParser;

namespace Amazon.ReportParser.Processing.Listings
{
    public class ListingFBAEstimatedFeeParser : BaseParser
    {
        public ListingFBAEstimatedFeeParser()
        {
        }


        public override ILineParser GetLineParser(ILogService log, MarketType market, string marketplaceId)
        {
            return new ListingFBAEstFeeLineParser(log);
        }

        public override void Process(IMarketApi api, ITime time, AmazonReportInfo reportInfo, IList<IReportItemDTO> reportItems)
        {
            using (var uow = new UnitOfWork(Log))
            {
                var items = reportItems.Cast<ListingFBAEstFeeDTO>().ToList();

                Log.Debug("Begin process items");
                ProcessItems(uow, items);
                Log.Debug("End process items");
            }
        }

        private void ProcessItems(IUnitOfWork db, IList<ListingFBAEstFeeDTO> items)
        {
            Log.Debug("ProcessItems begin");

            //Mark all except parsed as removed
            var removedList = db.ListingFBAEstFees.ProcessRemoved(items.Select(r => r.SKU));
            foreach (var removedItem in removedList)
            {
                Log.Debug("Mark as removed, SKU=" + removedItem);
            }

            foreach (var item in items)
            {
                Log.Debug("Stored item:");
                ToStringHelper.ToString(item);
                db.ListingFBAEstFees.CreateOrUpdate(item, DateTime.UtcNow);
            }

            Log.Debug("ProcessItems end");
        }
    }
}
