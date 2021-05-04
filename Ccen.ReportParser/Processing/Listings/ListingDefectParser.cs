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
using Amazon.DTO.Listings;
using Amazon.ReportParser.LineParser;

namespace Amazon.ReportParser.Processing.Listings
{
    public class ListingDefectParser : BaseParser
    {
        public ListingDefectParser()
        {
        }

        public override ILineParser GetLineParser(ILogService log, MarketType market, string marketplaceId)
        {
            return new ListingDefectLineParser(log);
        }

        public override void Process(IMarketApi api, ITime time, AmazonReportInfo reportInfo, IList<IReportItemDTO> reportItems)
        {
            using (var uow = new UnitOfWork(Log))
            {
                uow.DisableValidation();

                var items = reportItems.Cast<ListingDefectDTO>().ToList();

                string reportId = reportInfo.ReportId;

                foreach (var item in items)
                {
                    item.MarketplaceId = api.MarketplaceId;
                    item.MarketType = (int)api.Market;
                    item.ReportId = reportId;
                }

                Log.Debug("Begin process items");
                ProcessItems(uow, reportId, items);
                Log.Debug("End process items");
            }
        }

        private void ProcessItems(IUnitOfWork db, string reportId, IList<ListingDefectDTO> items)
        {
            Log.Debug("ProcessItems begin");

            foreach (var item in items)
            {
                Log.Debug("Stored item:" + ToStringHelper.ToString(item));
                db.ListingDefects.CreateOrUpdate(item, DateTime.UtcNow);
            }

            //Mark all except parsed as removed
            db.ListingDefects.MarkExceptFromFeedAsRemoved(reportId);

            Log.Debug("ProcessItems end");
        }
    }
}
