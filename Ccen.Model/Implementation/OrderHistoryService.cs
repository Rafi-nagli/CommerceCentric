using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Orders;

namespace Amazon.Model.Implementation
{
    public class OrderHistoryService : IOrderHistoryService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public OrderHistoryService(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
        }

        public void AddRecord(long orderId,
            string fieldName,
            object fromValue,
            object toValue,
            long? by)
        {
            AddRecord(orderId, fieldName, fromValue, null, toValue, null, by);
        }

        public void AddRecord(long orderId,
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
                    var newRecord = new OrderChangeHistory()
                    {
                        OrderId = orderId,
                        FieldName = fieldName,
                        FromValue = StringHelper.Substring((fromValue ?? "").ToString(), 50),
                        ExtendFromValue = StringHelper.Substring(extendFromValue, 50),
                        ToValue = StringHelper.Substring((toValue ?? "").ToString(), 50),
                        ExtendToValue = StringHelper.Substring(extendToValue, 50),
                        ChangedBy = by,
                        ChangeDate = _time.GetAppNowTime()
                    };
                    db.OrderChangeHistories.Add(newRecord);
                    db.Commit();
                }
            }
            catch (Exception ex)
            {
                _log.Error("OrderHistoryService.AddRecord, orderId=" + orderId, ex);
            }
        }
    }
}
