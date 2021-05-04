using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models.Histories.HistoryDatas;
using Newtonsoft.Json;

namespace Amazon.Model.Implementation
{
    public class StyleItemHistoryService : IStyleItemHistoryService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public StyleItemHistoryService(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
        }

        public void AddRecord(string actionName,
            long styleItemId,
            IHistoryData data,
            long? by)
        {
            var text = JsonConvert.SerializeObject(data);
            _log.Info("AddRecord, actionName=" + actionName + ", data=" + text);

            AddRecord(actionName, styleItemId, text, by);
        }

        public void AddRecord(string actionName,
            long styleItemId,
            string data,
            long? by)
        {
            if (actionName == null)
                return;
            
            try
            {
                using (var db = _dbFactory.GetRDb())
                {
                    var newRecord = new StyleItemActionHistory()
                    {
                        ActionName = actionName,
                        StyleItemId = styleItemId,
                        
                        Data = data,
                        CreateDate = _time.GetAppNowTime(),
                        CreatedBy = by,
                    };
                    db.StyleItemActionHistories.Add(newRecord);
                    db.Commit();
                }
            }
            catch (Exception ex)
            {
                _log.Error("StyleItemActionHistoryService.AddRecord, styleItemId=" + styleItemId, ex);
            }
        }
    }
}
