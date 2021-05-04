using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;


namespace Amazon.Model.Implementation.Errors
{
    public class DbSyncInformer : ISyncInformer
    {
        private IDbFactory _dbFactory;
        private ILogService _logger;
        private ITime _time;

        private long? _syncHistoryId;
        private SyncType _syncType;
        private string _marketplaceId;
        private MarketType _market;
        private string _additionalData;

        private List<string> _withErrors = new List<string>();
        private List<string> _withWarning = new List<string>();
        private SynchronizedCollection<string> _orderList = new SynchronizedCollection<string>();

        private object _syncListObj = new object();

        public DbSyncInformer(IDbFactory dbFactory, 
            ILogService logger, 
            ITime time,
            SyncType syncType, 
            string marketplaceId, 
            MarketType market,
            string additionalData)
        {
            _logger = logger;
            _time = time;

            _dbFactory = dbFactory;
            _syncType = syncType;
            _marketplaceId = marketplaceId;
            _market = market;
            _additionalData = additionalData;
        }

        public void OpenLastSync()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var lastSync = db.SyncHistory.GetAll().Where(s => s.Type == (int) _syncType
                                                                   && s.Market == (int) _market
                                                                   && s.MarketplaceId == _marketplaceId)
                    .OrderByDescending(s => s.StartDate)
                    .FirstOrDefault();
                if (lastSync != null)
                {
                    _syncHistoryId = lastSync.Id;
                }
            }
        }

        public void SyncBegin(int? count)
        {
            if (_syncHistoryId.HasValue)
                return;

            using (var db = _dbFactory.GetRWDb())
            {
                var history = new SyncHistory
                {
                    StartDate = _time.GetUtcTime(),
                    Type = (int) _syncType,
                    Market = (int) _market,
                    MarketplaceId = _marketplaceId,
                    AdditionalData = _additionalData,
                    PingDate = _time.GetUtcTime(),
                    Status = (int) SyncStatus.InProgresss,
                    CountToProcess = count
                };
                db.SyncHistory.Add(history);
                db.Commit();

                _syncHistoryId = history.Id;
                _withErrors = new List<string>();
                _withWarning = new List<string>();
                _orderList = new SynchronizedCollection<string>();

                _logger.Info("[Sync] start, type=" + _syncType
                             + ", market=" + _market
                             + ", marketplaceId=" + _marketplaceId
                             + ", id=" + _syncHistoryId);
            }
        }

        public void SyncEnd()
        {
            var count = 0;
            if (_syncHistoryId.HasValue)
            {
                using (var db = _dbFactory.GetRWDb())
                {
                    var history = db.SyncHistory.Get(_syncHistoryId.Value);
                    var status = SyncStatus.Complete;
                    if (_withWarning.Any(String.IsNullOrEmpty))
                        status = SyncStatus.CompleteWithWarnings;
                    if (_withErrors.Any(String.IsNullOrEmpty))
                        status = SyncStatus.CompleteWithErrors;

                    if (history != null)
                    {
                        history.EndDate = DateTime.UtcNow;
                        lock (_syncListObj)
                        {
                            count = _orderList.Count(v => !String.IsNullOrEmpty(v)); 
                        }
                        history.ProcessedTotal = count;
                        history.ProcessedWithWarning = _withWarning.Count(v => !String.IsNullOrEmpty(v));
                        history.ProcessedWithError = _withErrors.Count(v => !String.IsNullOrEmpty(v));
                        history.Status = (int) status;
                        db.Commit();
                    }
                }
            }

            _logger.Info("[Sync] end, type=" + _syncType 
                + ", market=" + _market 
                + ", marketplaceId=" + _marketplaceId 
                + ", id=" + _syncHistoryId 
                + ", total=" + count 
                + ", withErrors=" + _withErrors.Count 
                + ", withWarning=" + _withWarning.Count);

            _syncHistoryId = null;
        }

        public bool IsSyncInProgress()
        {
            var inactiveLimit = DateTime.UtcNow.AddMinutes(-8);

            using (var db = _dbFactory.GetRDb())
            {
                var items = db.SyncHistory.GetFiltered(t => t.Type == (int) _syncType
                                                             && t.MarketplaceId == _marketplaceId
                                                             && t.Market == (int) _market
                                                             && !t.EndDate.HasValue && t.PingDate > inactiveLimit)
                    .ToList();
                return items.Count > 0;
            }
        }

        public void PingSync()
        {
            if (_syncHistoryId.HasValue)
            {
                using (var db = _dbFactory.GetRWDb())
                {
                    var history = db.SyncHistory.Get(_syncHistoryId.Value);
                    if (history != null)
                    {
                        history.PingDate = _time.GetUtcTime();
                        db.Commit();
                    }
                }
            }
        }

        public void AddEntities(SyncMessageStatus status, IList<string> entities, string message)
        {
            foreach (var entity in entities)
            {
                TryAddToList(_orderList, entity);
                switch (status)
                {
                    case SyncMessageStatus.Error:
                        TryAddToList(_withErrors, entity);
                        break;
                    case SyncMessageStatus.Warning:
                        TryAddToList(_withWarning, entity);
                        break;
                }
            }

            AddMessage("", message + " - count=" + entities.Count, status);
        }

        public void AddError(string entityId, string message, Exception ex)
        {
            TryAddToList(_orderList, entityId);
            TryAddToList(_withErrors, entityId);
            AddMessage(entityId, message + (ex != null ? " (Exception details: " + ex.Message + ")" : String.Empty), SyncMessageStatus.Error);
        }

        public void AddWarning(string entityId, string message)
        {
            TryAddToList(_orderList, entityId);
            TryAddToList(_withWarning, entityId);
            AddMessage(entityId, message, SyncMessageStatus.Warning);
        }

        public void AddInfo(string entityId, string message)
        {
            TryAddToList(_orderList, entityId);
            AddMessage(entityId, message, SyncMessageStatus.Info);
        }

        public void AddSuccess(string entityId, string message)
        {
            lock (_syncListObj)
            {
                TryAddToList(_orderList, entityId);
            }
            AddMessage(entityId, message, SyncMessageStatus.Success);
        }

        public void UpdateProgress(string entityId)
        {
            if (_syncHistoryId.HasValue)
            {
                using (var db = _dbFactory.GetRWDb())
                {
                    var history = db.SyncHistory.Get(_syncHistoryId.Value);
                    if (history != null)
                    {
                        var count = 0;
                        lock (_syncListObj)
                        {
                            TryAddToList(_orderList, entityId);
                            count = _orderList.Count(v => !String.IsNullOrEmpty(v));
                        }
                        history.PingDate = _time.GetUtcTime();
                        history.ProcessedTotal = count;
                        db.Commit();
                    }
                }
            }
        }

        private void TryAddToList(IList<string> entityList, string newValue)
        {
            var checkValue = newValue ?? String.Empty;
            if (entityList != null
                //&& !String.IsNullOrEmpty(checkValue)
                && !entityList.Contains(checkValue))
                entityList.Add(checkValue);
        }

        private void AddMessage(string entityId, string message, SyncMessageStatus status)
        {
            if (_syncHistoryId.HasValue)
            {
                using (var db = _dbFactory.GetRWDb())
                {
                    var dbMessage = new SyncMessage
                    {
                        SyncHistoryId = _syncHistoryId.Value,
                        EntityId = entityId,

                        Message = StringHelper.Substring(message, 1024),
                        Status = (int) status,

                        CreateDate = DateTime.UtcNow
                    };

                    db.SyncMessages.Add(dbMessage);
                    db.Commit();
                }
                _logger.Info("[Sync] message=" + message + ", status=" + status);
            }
        }
    }
}
