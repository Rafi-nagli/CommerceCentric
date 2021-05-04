using System;
using System.Collections.Generic;
using Amazon.Core.Contracts;
using Amazon.Core.Models;

namespace Amazon.Model.Implementation.Errors
{
    public class EmptySyncInformer : ISyncInformer
    {
        private SyncType _syncType;
        private ILogService _log;

        public EmptySyncInformer(ILogService log, SyncType syncType)
        {
            _syncType = syncType;
            _log = log;
        }

        public void SyncBegin(int? countToProcess)
        {
            Write("SyncBegin, syncType=" + _syncType + ", countToProcess=" + countToProcess);
        }

        public void OpenLastSync()
        {
            Write("OpenLastSync, syncType=" + _syncType);
        }

        public void SyncEnd()
        {
            Write("SyncEnd, syncType=" + _syncType);
        }

        public bool IsSyncInProgress()
        {
            Write("IsSyncInProgress, syncType=" + _syncType + ", result=false");
            return false;
        }

        public void PingSync()
        {
            Write("PingSync, syncType=" + _syncType);
        }

        private string _messageTmpl = "Add {0}, syncType={1}, entityId={2}, message={3}";

        public void AddEntities(SyncMessageStatus status, IList<string> entities, string message)
        {
            Write("Add entities, count={0}, syncType={1}, message={2}", 
                entities.Count.ToString(), 
                status.ToString(), 
                message);
        }

        public void AddError(string entityId, string message, Exception ex)
        {
            Write(_messageTmpl, 
                "Error", 
                _syncType.ToString(), 
                entityId, 
                message + (ex != null ? " (exception details: " + ex.Message + ")" : String.Empty));
        }

        public void AddWarning(string entityId, string message)
        {
            Write(_messageTmpl, "Warning", _syncType.ToString(), entityId, message);
        }

        public void AddInfo(string entityId, string message)
        {
            Write(_messageTmpl, "Info", _syncType.ToString(), entityId, message);
        }

        public void AddSuccess(string entityId, string message)
        {
            Write(_messageTmpl, "Success", _syncType.ToString(), entityId, message);
        }

        public void UpdateProgress(string entityId)
        {
            Write("UpdateProgress, syncType=" + _syncType);
        }

        private void Write(string message, params string[] args)
        {
            Console.WriteLine(message, args);
            _log.Info(message, args);
        }

        private void Write(string message)
        {
            Console.WriteLine(message);
            _log.Info(message);
        }
    }
}
