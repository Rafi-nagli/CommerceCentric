using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Orders;

namespace Amazon.Model.Implementation
{
    public class StyleHistoryService : IStyleHistoryService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public StyleHistoryService(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
        }

        public void AddRecord(long styleId,
            string fieldName,
            object fromValue,
            object toValue,
            long? by)
        {
            AddRecord(styleId, fieldName, fromValue, null, toValue, null, by);
        }

        public void AddRecord(long styleId,
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
                    var newRecord = new StyleChangeHistory()
                    {
                        StyleId = styleId,
                        FieldName = fieldName,
                        FromValue = StringHelper.Substring((fromValue ?? "").ToString(), 255),
                        ExtendFromValue = StringHelper.Substring(extendFromValue, 50),
                        ToValue = StringHelper.Substring((toValue ?? "").ToString(), 255),
                        ExtendToValue = StringHelper.Substring(extendToValue, 50),
                        ChangedBy = by,
                        ChangeDate = _time.GetAppNowTime()
                    };
                    db.StyleChangeHistories.Add(newRecord);
                    db.Commit();
                }
            }
            catch (Exception ex)
            {
                _log.Error("StyleHistoryService.AddRecord, styleId=" + styleId, ex);
            }
        }
    }
}
