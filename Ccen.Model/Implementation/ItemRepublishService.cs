using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;

namespace Amazon.Model.Implementation
{
    public class ItemRepublishService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public ItemRepublishService(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
        }

        public void RepublishInactive()
        {
            _log.Info("Republish");
            using (var db = _dbFactory.GetRWDb())
            {
                var lastDate = _time.GetAppNowTime().AddMonths(-1);

                var items = db.Items.GetAll()
                    .Where(i => i.Market == (int) MarketType.Walmart
                            && i.ItemPublishedStatus == (int) PublishedStatuses.PublishedInactive
                            && (!i.LastForceRepublishedDate.HasValue || i.LastForceRepublishedDate.Value < lastDate))
                    .ToList();

                _log.Info("Count to republish: " + items.Count);

                foreach (var item in items)
                {
                    item.ItemPublishedStatusBeforeRepublishing = item.ItemPublishedStatus;
                    item.LastForceRepublishedDate = _time.GetAppNowTime();
                    item.ItemPublishedStatus = (int) PublishedStatuses.HasChanges;

                    _log.Info("Updated item status, ASIN=" + item.ASIN + ", market=" + MarketType.Walmart);
                }

                db.Commit();
            }
        }

        public void PublishUnpublishedListings(MarketType market, string marketplaceId)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var toPublishItems = (from i in db.Items.GetAll()
                                      join si in db.StyleItemCaches.GetAll() on i.StyleItemId equals si.Id
                                      join st in db.Styles.GetAll() on si.StyleId equals st.Id
                                      where (i.ItemPublishedStatus == (int)PublishedStatuses.Unpublished) //NOTE: TEMP: to fixing parentItems
                                            && si.RemainingQuantity > 0
                                            && i.Market == (int)market
                                            && (i.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))
                                      select i).ToList();

                if (toPublishItems.Any())
                {
                    var parentASINs = toPublishItems.Select(i => i.ParentASIN).ToList();
                    var toPublishParentItems = db.ParentItems.GetAll().Where(pi => parentASINs.Contains(pi.ASIN)
                            && pi.Market == (int)market
                            && (pi.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId)))
                        .ToList();

                    foreach (var item in toPublishItems)
                    {
                        _log.Info("Changed status for " + item.ASIN + ": " + item.ItemPublishedStatus + "=>" +
                                  PublishedStatuses.New);
                        item.ItemPublishedStatus = (int)PublishedStatuses.New;
                        item.SourceMarketId = null;
                        item.SourceMarketUrl = null;

                        var parentItem = toPublishParentItems.FirstOrDefault(pi => pi.ASIN == item.ParentASIN);
                        if (parentItem != null)
                        {
                            _log.Info("Reset parentItem marketInfo: " + parentItem.ASIN);
                            parentItem.SourceMarketId = null;
                            parentItem.SourceMarketUrl = null;
                        }
                    }
                    db.Commit();
                }
            }
        }
    }
}
