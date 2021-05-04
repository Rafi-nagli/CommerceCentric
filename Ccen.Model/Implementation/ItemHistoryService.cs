using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Orders;

namespace Amazon.Model.Implementation
{
    public class ItemHistoryService : IItemHistoryService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public ItemHistoryService(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
        }

        public void AddRecord(long itemId,
            string fieldName,
            object fromValue,
            object toValue,
            long? by)
        {
            AddRecord(itemId, fieldName, fromValue, null, toValue, null, by);
        }

        public void AddRecord(long itemId,
            string fieldName,
            object fromValue,
            string extendFromValue,
            object toValue,
            string extendToValue,
            long? by)
        {
            if (fromValue == null && toValue == null)
                return;

            if ((fromValue ?? "").ToString() == (toValue ?? "").ToString())
                return;

            try
            {
                using (var db = _dbFactory.GetRDb())
                {
                    var newRecord = new ItemChangeHistory()
                    {
                        ItemId = itemId,
                        FieldName = fieldName,
                        FromValue = StringHelper.Substring((fromValue ?? "").ToString(), 255),
                        ExtendFromValue = StringHelper.Substring(extendFromValue, 50),
                        ToValue = StringHelper.Substring((toValue ?? "").ToString(), 255),
                        ExtendToValue = StringHelper.Substring(extendToValue, 50),
                        ChangedBy = by,
                        ChangeDate = _time.GetAppNowTime()
                    };
                    db.ItemChangeHistories.Add(newRecord);
                    db.Commit();
                }
            }
            catch (Exception ex)
            {
                _log.Error("ItemHistoryService.AddRecord, itemId=" + itemId, ex);
            }
        }

        public void LogListingPrice(PriceChangeSourceType type,
            long listingId,
            string sku,
            decimal newPrice,
            decimal? oldPrice,
            DateTime when,
            long? by)
        {
            using (var db = _dbFactory.GetRDb())
            {
                db.PriceHistories.Add(new PriceHistory()
                {
                    ListingId = listingId,
                    SKU = sku,
                    Type = (int)type,

                    Price = newPrice,
                    OldPrice = oldPrice,

                    ChangeDate = when,
                    ChangedBy = by,
                });

                db.Commit();
            }            
        }
    }
}
