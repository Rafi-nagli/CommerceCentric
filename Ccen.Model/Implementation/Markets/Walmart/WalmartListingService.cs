using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.Implementation.Markets.Walmart
{
    public class WalmartListingService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public WalmartListingService(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
        }

        public void RepublishListingWithImageIssue()
        {
            _log.Info("RepublishListingWithImageIssue");
            var lastDate = _time.GetAppNowTime().AddDays(-1);

            using (var db = _dbFactory.GetRWDb())
            {
                var items = db.Items.GetAll()
                    .Where(i => i.ItemPublishedStatus == (int)PublishedStatuses.PublishedInactive
                        && (i.Market == (int)MarketType.Walmart || i.Market == (int)MarketType.WalmartCA)
                        && i.ItemPublishedStatusReason.Contains("Primary Image Missing")
                        && (!i.LastForceRepublishedDate.HasValue || i.LastForceRepublishedDate.Value < lastDate))
                    .ToList();

                _log.Info("Count to republish (with image issue): " + items.Count);

                foreach (var item in items)
                {
                    item.ItemPublishedStatusBeforeRepublishing = item.ItemPublishedStatus;
                    item.LastForceRepublishedDate = _time.GetAppNowTime();
                    item.ItemPublishedStatus = (int)PublishedStatuses.HasChanges;

                    _log.Info("Updated item status, ASIN=" + item.ASIN + ", market=" + (MarketType)item.Market);
                }

                db.Commit();
            }            
        }

        public void RepublishListingWithSKUIssue()
        {
            _log.Info("RepublishListingWithSKUIssue");
            var lastDate = _time.GetAppNowTime().AddDays(-1);

            using (var db = _dbFactory.GetRWDb())
            {
                var items = db.Items.GetAll()
                    .Where(i => i.ItemPublishedStatus != (int)PublishedStatuses.Unpublished
                        && (i.Market == (int)MarketType.Walmart || i.Market == (int)MarketType.WalmartCA)
                        && i.ItemPublishedStatusReason.Contains("If you want to change the SKU, please send us a SKU Override.")
                        && (!i.LastForceRepublishedDate.HasValue || i.LastForceRepublishedDate.Value < lastDate))
                    .ToList();

                _log.Info("Count to republish (with SKU issue): " + items.Count);

                foreach (var item in items)
                {
                    item.ItemPublishedStatusBeforeRepublishing = item.ItemPublishedStatus;
                    item.LastForceRepublishedDate = _time.GetAppNowTime();
                    item.ItemPublishedStatus = (int)PublishedStatuses.HasChangesWithSKU;

                    _log.Info("Updated item status, ASIN=" + item.ASIN + ", market=" + (MarketType)item.Market);
                }

                db.Commit();
            }
        }

        public void RepublishListingWithBarcodeIssue()
        {
            _log.Info("RepublishListingWithBarcodeIssue");
            var lastDate = _time.GetAppNowTime().AddDays(-1);

            using (var db = _dbFactory.GetRWDb())
            {
                var items = db.Items.GetAll()
                    .Where(i => i.ItemPublishedStatus != (int)PublishedStatuses.Unpublished
                        && (i.Market == (int)MarketType.Walmart || i.Market == (int)MarketType.WalmartCA)
                        && i.ItemPublishedStatusReason.Contains("This SKU is already set up with a different Product ID")
                        && (!i.LastForceRepublishedDate.HasValue || i.LastForceRepublishedDate.Value < lastDate))
                    .ToList();

                _log.Info("Count to republish (with barcode issue): " + items.Count);

                foreach (var item in items)
                {
                    item.ItemPublishedStatusBeforeRepublishing = item.ItemPublishedStatus;
                    item.LastForceRepublishedDate = _time.GetAppNowTime();
                    item.ItemPublishedStatus = (int)PublishedStatuses.HasChangesWithProductId;

                    _log.Info("Updated item status, ASIN=" + item.ASIN + ", market=" + (MarketType)item.Market);
                }

                db.Commit();
            }
        }
    }
}
