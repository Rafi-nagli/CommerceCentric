using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.General;
using Amazon.Core.Models;
using Amazon.Model.Implementation.Markets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.Implementation.Charts
{
    public class ListingErrorChartMaker
    {
        private IDbFactory _dbFactory;
        private ILogService _log;
        private ITime _time;

        public ListingErrorChartMaker(IDbFactory dbFactory,
            ITime time,
            ILogService log)
        {
            _dbFactory = dbFactory;
            _log = log;
            _time = time;
        }

        public void AddPoints()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var marketList = new MarketplaceKeeper(_dbFactory, false).GetAll();
                var today = _time.GetAppNowTime().Date;
                foreach (var market in marketList)
                {
                    var listingErrorCount = (from i in db.Items.GetAll()
                                            join l in db.Listings.GetAll() on i.Id equals l.ItemId
                                            where
                             l.RealQuantity > 0 
                             && !l.IsRemoved                             
                             && (i.ItemPublishedStatus == (int)PublishedStatuses.PublishingErrors
                                || i.ItemPublishedStatus == (int)PublishedStatuses.PublishedInProgress)
                             && i.Market == market.Market
                             && (i.MarketplaceId == market.MarketplaceId || String.IsNullOrEmpty(market.MarketplaceId))
                             select i.Id
                        ).Count();

                    var marketTag = market.Market + "_" + market.MarketplaceId;
                    var chart = db.Charts.GetAll().FirstOrDefault(ch => ch.ChartName == ChartHelper.ListingErrorChartName
                        && ch.ChartTag == marketTag);

                    if (chart == null)
                    {
                        chart = new Chart()
                        {
                            ChartName = ChartHelper.ListingErrorChartName,
                            ChartSubGroup = MarketHelper.GetMarketName(market.Market, market.MarketplaceId),
                            ChartTag = marketTag,
                            CreateDate = _time.GetAppNowTime()
                        };
                        db.Charts.Add(chart);
                        db.Commit();
                    }

                    var existPoint = db.ChartPoints.GetAll().FirstOrDefault(p => p.ChartId == chart.Id
                        && p.Date == today);

                    if (existPoint == null)
                    {
                        existPoint = new ChartPoint()
                        {
                            ChartId = chart.Id,
                            Date = today,
                        };
                        db.ChartPoints.Add(existPoint);
                    }
                    existPoint.Value = listingErrorCount;
                }
                db.Commit();
            }
        }
    }
}
